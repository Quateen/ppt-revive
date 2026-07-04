using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;
using Microsoft.Extensions.Configuration;
using PPTRevive.Application.Common.Interfaces;
using PPTRevive.Application.Common.Models;
using System.Text.Json;
using ShapeModel = DocumentFormat.OpenXml.Presentation.Shape;
using A = DocumentFormat.OpenXml.Drawing;
using P = DocumentFormat.OpenXml.Presentation;
using Microsoft.AspNetCore.Http;

namespace PPTRevive.Infrastructure.Services;

public class ProcessPptJobService : IProcessPptJobService
{
    private readonly IUser _currentUser;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly IPubMedRepository _pubMedRepo;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProcessPptJobService(IUser currentUser, IHttpClientFactory httpClientFactory, IConfiguration configuration,
        IPubMedRepository pubMedRepo, IHttpContextAccessor httpContextAccessor)
    {
        _currentUser = currentUser;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _pubMedRepo = pubMedRepo;
        _httpContextAccessor = httpContextAccessor;
    }

    private ResponseBase ErrorResponse(string error)
    {
        return new ResponseBase
        {
            Status = false,
            Error = error
        };
    }

    public async Task<ResponseBase> ProcessAsync(byte[] pptBytes, string fileName, CancellationToken cancellationToken)
    {
        try
        {
            var slideResponses = await ParseAndAnalyzeSlidesAsync(pptBytes);
            if (slideResponses.Count == 0)
                return ErrorResponse("No valid slides found or processed.");

            // The finalized PPTX is generated on demand in PPTApproveCommand once the user
            // has chosen which slides to apply — no deck is built here.
            return new ResponseBase
            {
                Status = true,
                Data = new PPTResponse
                {
                    FileName = fileName,
                    SlidePageCount = slideResponses.Count,
                    ProcessedAt = DateTime.UtcNow,
                    SlidePages = slideResponses
                }
            };
        }
        catch (Exception ex)
        {
            return ErrorResponse(ex.Message);
        }
    }

    // private static async Task<string> SavePPT(string fileName, byte[] pptUpdated, string dirName)
    // {
    //     var wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
    //     var saveDirectory = Path.Combine(wwwRootPath, dirName);
    //     // Ensure the directory exists
    //     Directory.CreateDirectory(saveDirectory);

    //     var filePath = Path.Combine(saveDirectory, fileName);

    //     await File.WriteAllBytesAsync(filePath, pptUpdated);

    //     // Make relative path (e.g., "updated-ppts/xyz_Updated_file.pptx")
    //     var relativePath = Path.GetRelativePath(wwwRootPath, filePath).Replace("\\", "/");
    //     //var request = _httpContextAccessor.HttpContext!.Request;
    //     //var baseUrl = $"{request.Scheme}://{request.Host}";
    //     var fileUrl = $"/{relativePath}";
    //     return fileUrl;
    // }

    // Plain snapshot of one slide's extracted text, read single-threaded from OpenXML
    // before any parallel network work begins (DocumentFormat.OpenXml is not thread-safe).
    private sealed class SlideExtract
    {
        public int SlideIndex { get; init; }
        public int SlideNo { get; init; }
        public string OriginalText { get; init; } = string.Empty;
        public string? TitleText { get; init; }
        public List<SlideTextItem> Items { get; init; } = new();
    }

    private async Task<List<SlidePagesResponse>> ParseAndAnalyzeSlidesAsync(byte[] pptBytes)
    {
        // ---- Phase 1: extract every slide's text SEQUENTIALLY (OpenXML is not thread-safe) ----
        var extracts = new List<SlideExtract>();
        using (var memoryStream = new MemoryStream(pptBytes))
        using (var presentationDoc = PresentationDocument.Open(memoryStream, false))
        {
            var presentationPart = presentationDoc.PresentationPart!;
            var slideIdList = presentationPart.Presentation.SlideIdList!;
            var slideIds = slideIdList.Elements<P.SlideId>().ToList();

            // Free lead-magnet tier: only the first N slides are revived; the rest of the
            // deck is left untouched. This is the "revive up to N slides free" wedge.
            var freeTierSlides = _configuration.GetValue<int?>("Processing:FreeTierSlides") ?? 5;
            var slidesToProcess = Math.Min(slideIds.Count, freeTierSlides);

            for (int slideIndex = 0; slideIndex < slidesToProcess; slideIndex++)
            {
                var slideNo = slideIndex + 1;
                var relId = slideIds[slideIndex].RelationshipId;
                var slidePart = (SlidePart)presentationPart.GetPartById(relId!);

                var originalText = string.Join("\n",
                    GetAllTextElements(slidePart.Slide).Where(t => !string.IsNullOrWhiteSpace(t))).Trim();

                // Join all runs in the title shape — a title split across multiple runs
                // (bold word, autocorrect, etc.) would otherwise be truncated to the first run.
                var titleRuns = slidePart.Slide.Descendants<ShapeModel>()
                    .Where(s => s.NonVisualShapeProperties?.NonVisualDrawingProperties?.Name?.Value?.ToLower().Contains("title") == true)
                    .SelectMany(s => s.TextBody?.Descendants<A.Text>() ?? [])
                    .Select(t => t.Text)
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .ToList();
                string? titleText = titleRuns.Any() ? string.Join(" ", titleRuns) : null;

                var slideTextItems = new List<SlideTextItem>();
                foreach (var shape in slidePart.Slide.Descendants<ShapeModel>())
                {
                    var shapeName = shape.NonVisualShapeProperties?.NonVisualDrawingProperties?.Name?.Value ?? "";
                    var isFooterOrAuthor = shapeName.ToLower().Contains("footer") || shapeName.ToLower().Contains("author");
                    if (shapeName.ToLower().Contains("title")) continue; // skip title shapes for analysis

                    if (shape.TextBody != null)
                    {
                        foreach (var para in shape.TextBody.Elements<A.Paragraph>())
                        {
                            var text = string.Join(" ", para.Descendants<A.Text>()
                                .Select(t => t.Text)
                                .Where(t => !string.IsNullOrWhiteSpace(t)));

                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                slideTextItems.Add(new SlideTextItem
                                {
                                    SlideIndex = slideIndex,
                                    Text = text,
                                    ShapeName = shapeName,
                                    IsBullet = para.ParagraphProperties?.GetFirstChild<A.CharacterBullet>() != null ||
                                               para.ParagraphProperties?.GetFirstChild<A.AutoNumberedBullet>() != null,
                                    IsAuthorOrFooter = isFooterOrAuthor
                                });
                            }
                        }
                    }
                }

                if (!slideTextItems.Any() && !string.IsNullOrWhiteSpace(originalText))
                {
                    slideTextItems.Add(new SlideTextItem
                    {
                        SlideIndex = slideIndex,
                        Text = originalText,
                        ShapeName = "RawFallback",
                        IsBullet = false,
                        IsAuthorOrFooter = false
                    });
                }

                extracts.Add(new SlideExtract
                {
                    SlideIndex = slideIndex,
                    SlideNo = slideNo,
                    OriginalText = originalText,
                    TitleText = titleText,
                    Items = slideTextItems
                });
            }
        }

        // ---- Phase 2: run ONLY the network analysis in a bounded-parallel section ----
        var maxParallelSlides = _configuration.GetValue<int?>("Processing:MaxParallelSlides") ?? 3;
        using var semaphore = new SemaphoreSlim(maxParallelSlides);
        var results = new System.Collections.Concurrent.ConcurrentBag<SlidePagesResponse>();

        var tasks = extracts.Select(async extract =>
        {
            await semaphore.WaitAsync();
            try
            {
                // A slide with no analyzable text still appears in the deck unchanged.
                if (!extract.Items.Any())
                {
                    results.Add(new SlidePagesResponse
                    {
                        SlideId = extract.SlideNo,
                        OriginalSlideContent = extract.OriginalText,
                        UpdatedSlideContent = string.Empty,
                        TitleText = extract.TitleText,
                        References = new(),
                        Explanation = "No textual content to update.",
                        Source = string.Empty
                    });
                    return;
                }

                var keywordText = string.Join(" ", extract.Items.Select(item => item.Text));

                // Search PubMed with the Claude-extracted key terms, not raw slide prose.
                var keyTerms = await _pubMedRepo.ExtractKeyMedicalTerms(keywordText);
                var articles = await _pubMedRepo.SearchRelevantArticlesAsync(keyTerms);

                // No supporting literature → do NOT ask the model to invent a change.
                // Return the slide unchanged with an honest note (physician-trust > edit volume).
                if (articles.Count == 0)
                {
                    results.Add(new SlidePagesResponse
                    {
                        SlideId = extract.SlideNo,
                        OriginalSlideContent = extract.OriginalText,
                        UpdatedSlideContent = string.Empty,
                        TitleText = extract.TitleText,
                        References = new(),
                        Explanation = "No newer evidence found for this slide — current content appears up to date.",
                        Source = string.Empty
                    });
                    return;
                }

                var analysisResult = await _pubMedRepo.AnalyzeMedicalSlideAsync(extract.Items, articles);

                var slideReferences = articles
                    .Select(article => $"{string.Join(", ", article.Authors)}. \"{article.Title}.\" {article.Journal}, {article.PublicationDate}. DOI: {article.Doi ?? "N/A"} – {article.Link}")
                    .Distinct()
                    .ToList();

                // On analysis failure, keep the original content so the slide is never lost.
                string updatedContent;
                if (analysisResult.Success)
                {
                    var updatedItems = analysisResult.ParsedItems ?? new();
                    if (!string.IsNullOrWhiteSpace(extract.TitleText))
                    {
                        updatedItems = updatedItems
                            .Where(item => !item.Text.Trim().Equals(extract.TitleText!.Trim(), StringComparison.OrdinalIgnoreCase))
                            .ToList();
                    }
                    updatedContent = string.Join("\n",
                        updatedItems.Where(item => item.SlideIndex == extract.SlideIndex && !string.IsNullOrWhiteSpace(item.Text))
                                    .Select(u => u.Text));
                }
                else
                {
                    updatedContent = string.Empty;
                }

                results.Add(new SlidePagesResponse
                {
                    SlideId = extract.SlideNo,
                    OriginalSlideContent = extract.OriginalText,
                    UpdatedSlideContent = updatedContent,
                    TitleText = extract.TitleText,
                    References = slideReferences,
                    Explanation = analysisResult.Success ? analysisResult.Explanation : "Automatic update unavailable for this slide; original content kept.",
                    Source = analysisResult.Success ? analysisResult.Source : string.Empty
                });
            }
            catch (Exception ex)
            {
                // Never drop a slide: record it with original content and a generic note.
                Console.WriteLine($"❌ Error processing slide {extract.SlideNo}: {ex.Message}");
                results.Add(new SlidePagesResponse
                {
                    SlideId = extract.SlideNo,
                    OriginalSlideContent = extract.OriginalText,
                    UpdatedSlideContent = string.Empty,
                    TitleText = extract.TitleText,
                    References = new(),
                    Explanation = "This slide could not be processed automatically; original content kept.",
                    Source = string.Empty
                });
            }
            finally
            {
                semaphore.Release();
            }
        }).ToList();

        await Task.WhenAll(tasks);
        return results.OrderBy(x => x.SlideId).ToList();
    }

    private IEnumerable<string> GetAllTextElements(OpenXmlElement element)
    {
        return element.Descendants<A.Text>()
            .Select(t => t.Text)
            .Where(t => !string.IsNullOrWhiteSpace(t));
    }

}
