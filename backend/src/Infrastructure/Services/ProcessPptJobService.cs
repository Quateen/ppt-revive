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

            // string orignalFileURL = await SavePPT(fileName, pptBytes, "orignal-ppts");

            var pptUpdated = CreateUpdatedPresentation(pptBytes, slideResponses);

            // fileName = $"Updated_{fileName}";
            // // Save Updated PDF
            // string newFileURL = await SavePPT(fileName, pptUpdated, "updated-ppts");

            return new ResponseBase
            {
                Status = true,
                Data = new PPTResponse
                {
                    // FileUrl = orignalFileURL,
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

    private async Task<List<SlidePagesResponse>> ParseAndAnalyzeSlidesAsync(byte[] pptBytes)
    {
        var results = new System.Collections.Concurrent.ConcurrentBag<SlidePagesResponse>();

        using var memoryStream = new MemoryStream(pptBytes);
        using var presentationDoc = PresentationDocument.Open(memoryStream, false);

        var presentationPart = presentationDoc.PresentationPart!;
        var slideIdList = presentationPart.Presentation.SlideIdList!;
        var slideIds = slideIdList.Elements<P.SlideId>().ToList();

        // Bound concurrency so a large deck can't overwhelm the Claude/PubMed APIs
        // (NCBI allows 3 req/sec with an API key).
        var maxParallelSlides = _configuration.GetValue<int?>("Processing:MaxParallelSlides") ?? 3;
        using var semaphore = new SemaphoreSlim(maxParallelSlides);

        var tasks = slideIds.Select(async slideId =>
        {
            await semaphore.WaitAsync();
            try
            {
                var relId = slideId.RelationshipId;
                var slidePart = (SlidePart)presentationPart.GetPartById(relId!);

                var slideIndex = slideIds.IndexOf(slideId); // 0-based index
                var slideNo = slideIndex + 1; // 1-based ID for SlideId

                var allTextElements = GetAllTextElements(slidePart.Slide).ToList();
                var originalText = string.Join("\n", allTextElements.Where(t => !string.IsNullOrWhiteSpace(t))).Trim();

                Console.WriteLine($"📋 Original Text on Slide {slideNo}:\n{originalText}");

                string? titleText = slidePart.Slide.Descendants<ShapeModel>()
                    .Where(s => s.NonVisualShapeProperties?.NonVisualDrawingProperties?.Name?.Value?.ToLower().Contains("title") == true)
                    .SelectMany(s => s.TextBody?.Descendants<A.Text>() ?? [])
                    .Select(t => t.Text)
                    .FirstOrDefault();

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
                                    SlideIndex = slideIndex, // ✅ true index
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

                if (!slideTextItems.Any())
                {
                    Console.WriteLine($"⚠️ Skipping Slide {slideNo} — no textual content found.");
                    return;
                }

                var keywordText = string.Join(" ", slideTextItems.Select(item => item.Text));

                // Search PubMed with the Claude-extracted key terms, not the raw slide text —
                // raw slide prose makes a poor PubMed query.
                var keyTerms = await _pubMedRepo.ExtractKeyMedicalTerms(keywordText);
                var articles = await _pubMedRepo.SearchRelevantArticlesAsync(keyTerms);

                Console.WriteLine($"🧠 Sending Slide {slideNo} content to Claude for enhancement...");
                var analysisResult = await _pubMedRepo.AnalyzeMedicalSlideAsync(slideTextItems, articles);

                // References for THIS slide only; the frontend aggregates and dedupes,
                // and the finalize step compiles the distinct set into reference slides.
                var slideReferences = articles
                    .Select(article => $"{string.Join(", ", article.Authors)}. \"{article.Title}.\" {article.Journal}, {article.PublicationDate}. DOI: {article.Doi ?? "N/A"} – {article.Link}")
                    .Distinct()
                    .ToList();

                var updatedItems = JsonSerializer.Deserialize<List<SlideTextItem>>(analysisResult.SuggestedUpdate) ?? new();
                if (!string.IsNullOrWhiteSpace(titleText))
                {
                    updatedItems = updatedItems
                        .Where(item => !item.Text.Trim().Equals(titleText.Trim(), StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                var filteredItems = updatedItems.Where(item => item.SlideIndex == slideIndex).ToList();

                var updatedContent = string.Join("\n",
                    filteredItems.Where(item => !string.IsNullOrWhiteSpace(item.Text)).Select(u => u.Text));

                var slideGroup = updatedItems.GroupBy(x => x.SlideIndex);
                foreach (var group in slideGroup)
                {
                    Console.WriteLine($"✅ AI returned {group.Count()} items for Slide {group.Key + 1}");
                }

                results.Add(new SlidePagesResponse
                {
                    SlideId = slideNo,
                    OriginalSlideContent = originalText,
                    UpdatedSlideContent = updatedContent,
                    TitleText = titleText,
                    References = slideReferences,
                    Explanation = analysisResult.Explanation,
                    Source = analysisResult.Source
                });
                Console.WriteLine($"✅ Slide {slideNo} processed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error processing slide: {ex.Message}");
            }
            finally
            {
                semaphore.Release();
            }
        }).ToList();

        await Task.WhenAll(tasks);
        return results.OrderBy(x => x.SlideId).ToList();
    }

    //private static List<SlidePart> GetSlideNumbers(PresentationDocument presentationDoc)
    //{
    //    var slideParts = new List<SlidePart>();

    //    var presentationPart = presentationDoc.PresentationPart!;
    //    var slideIdList = presentationPart.Presentation.SlideIdList!;
    //    slideParts = new List<SlidePart>();
    //    foreach (var slideId in slideIdList.Elements<P.SlideId>())
    //    {
    //        var relId = slideId.RelationshipId;
    //        var slidePart = (SlidePart)presentationPart.GetPartById(relId!);
    //        slideParts.Add(slidePart);
    //    }

    //    return slideParts;
    //}

    private void UpdateSlideContent(SlidePart slidePart, string updatedText)
    {
        var contentLines = updatedText
            .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        if (!contentLines.Any()) return;

        var textShapes = slidePart.Slide.Descendants<ShapeModel>()
            .Where(s => s.TextBody != null)
            .ToList();

        if (!textShapes.Any()) return;

        var mainTextShape = FindMainContentShape(textShapes);
        if (mainTextShape?.TextBody == null) return;

        var textBody = mainTextShape.TextBody;
        var existingParagraphs = textBody.Elements<A.Paragraph>().ToList();
        var originalLevels = existingParagraphs
            .Select(p => p.ParagraphProperties?.Level?.Value ?? 0)
            .ToList();

        if (!existingParagraphs.Any()) return;

        var templatePara = existingParagraphs.FirstOrDefault(p =>
            p.Descendants<A.Text>().Any(t => !string.IsNullOrWhiteSpace(t.Text)))
            ?? existingParagraphs.First();

        var runTemplate = templatePara.Descendants<A.Run>().FirstOrDefault();

        textBody.RemoveAllChildren<A.Paragraph>();

        var finalLines = SmartFitLines(contentLines, existingParagraphs.Count);

        for (int i = 0; i < finalLines.Count; i++)
        {
            var para = new A.Paragraph();
            var paraProps = new A.ParagraphProperties();

            if (i < originalLevels.Count)
            {
                paraProps.Level = originalLevels[i];
            }
            else
            {
                paraProps.Level = 0;
            }
            para.ParagraphProperties = paraProps;

            var run = new A.Run();
            if (runTemplate?.RunProperties != null)
            {
                run.AppendChild((A.RunProperties)runTemplate.RunProperties.CloneNode(true));
            }

            var text = new A.Text(finalLines[i]);
            SetSpacePreservation(text);
            run.AppendChild(text);
            para.AppendChild(run);
            textBody.AppendChild(para);
        }

        slidePart.Slide.Save();
    }


    private IEnumerable<string> GetAllTextElements(OpenXmlElement element)
    {
        return element.Descendants<A.Text>()
            .Select(t => t.Text)
            .Where(t => !string.IsNullOrWhiteSpace(t));
    }

    private void RestoreMetaText(SlidePart slidePart, string titleText, string authorText)
    {
        foreach (var shape in slidePart.Slide.Descendants<ShapeModel>())
        {
            var shapeName = shape.NonVisualShapeProperties?.NonVisualDrawingProperties?.Name?.Value?.ToLower();
            if (string.IsNullOrWhiteSpace(shapeName)) continue;

            string? contentToRestore = shapeName.Contains("title") ? titleText :
                                      shapeName.Contains("author") ? authorText :
                                      null;

            if (contentToRestore != null && shape.TextBody != null)
            {
                shape.TextBody.RemoveAllChildren<A.Paragraph>();
                var para = new A.Paragraph();
                var run = new A.Run();
                run.AppendChild(new A.Text(contentToRestore));
                para.AppendChild(run);
                shape.TextBody.AppendChild(para);
            }
        }
    }

    private byte[] CreateUpdatedPresentation(byte[] originalBytes, List<SlidePagesResponse> updatedSlides)
    {
        using var inputStream = new MemoryStream(originalBytes);
        using var outputStream = new MemoryStream();
        inputStream.CopyTo(outputStream);
        outputStream.Position = 0;

        try
        {
            using (var presentation = PresentationDocument.Open(outputStream, true))
            {
                var slideParts = presentation.PresentationPart?.SlideParts?.ToList();
                if (slideParts == null || slideParts.Count == 0) return originalBytes;

                for (int i = 0; i < slideParts.Count; i++)
                {
                    var slidePart = slideParts[i];
                    var matched = updatedSlides.FirstOrDefault(s => s.SlideId == i + 1);
                    if (matched == null) continue;

                    var updatedText = matched.UpdatedSlideContent?.Trim();
                    if (string.IsNullOrWhiteSpace(updatedText) || updatedText == "N/A") continue;

                    UpdateSlideContent(slidePart, updatedText);

                    RestoreMetaText(slidePart,
                        titleText: matched.OriginalSlideContent.Split('\n').FirstOrDefault() ?? "",
                        authorText: ExtractAuthorLine(matched.OriginalSlideContent));
                }

                presentation.PresentationPart?.Presentation?.Save();
            }

            return outputStream.ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating presentation: {ex.Message}");
            return originalBytes;
        }
    }

    private static string ExtractAuthorLine(string fullText)
    {
        var lines = fullText.Split('\n');
        return lines.Length >= 2 ? lines.LastOrDefault()?.Trim() ?? "" : "";
    }
    // --- UPDATE END ---

    private List<string> SmartFitLines(List<string> lines, int maxBlocks)
    {
        if (lines.Count <= maxBlocks) return lines;

        var safeLines = lines.Take(maxBlocks - 1).ToList();
        var last = string.Join(" ", lines.Skip(maxBlocks - 1));
        safeLines.Add(last);

        return safeLines;
    }

    private void SetSpacePreservation(A.Text textElement)
    {
        textElement.SetAttribute(new OpenXmlAttribute(
            "space",
            "http://www.w3.org/XML/1998/namespace",
            "preserve"
        ));
    }

    private ShapeModel? FindMainContentShape(List<ShapeModel> textShapes)
    {
        if (textShapes == null || !textShapes.Any()) return null;

        // Strategy 1: Find the shape with the most paragraphs (likely the main content)
        var shapesByParagraphCount = textShapes
            .Select(shape => new
            {
                Shape = shape,
                ParagraphCount = shape.TextBody?.Descendants<A.Paragraph>().Count() ?? 0
            })
            .OrderByDescending(x => x.ParagraphCount);

        var bestMatch = shapesByParagraphCount.FirstOrDefault();
        return bestMatch?.Shape ?? textShapes.FirstOrDefault();
    }

    private void UpdateTextBodyWithPreservedFormatting(P.TextBody textBody, List<string> contentLines, List<A.Paragraph> existingParagraphs)
    {
        if (textBody == null || !existingParagraphs.Any() || !contentLines.Any()) return;

        var templateParagraph = existingParagraphs.FirstOrDefault(p =>
            p.Descendants<A.Text>().Any(t => !string.IsNullOrWhiteSpace(t.Text)))
            ?? existingParagraphs.First();

        textBody.RemoveAllChildren<A.Paragraph>();

        foreach (var line in contentLines)
        {
            var newParagraph = (A.Paragraph)templateParagraph.CloneNode(true);
            newParagraph.RemoveAllChildren<A.Run>();

            var templateRun = templateParagraph.Descendants<A.Run>().FirstOrDefault();
            var run = new A.Run();

            if (templateRun != null)
            {
                var runProps = templateRun.GetFirstChild<A.RunProperties>();
                if (runProps != null)
                {
                    run.AppendChild((A.RunProperties)runProps.CloneNode(true));
                }
            }


            var text = new A.Text(line);
            SetSpacePreservation(text);
            run.AppendChild(text);

            newParagraph.AppendChild(run);
            textBody.AppendChild(newParagraph);
        }
    }
}

public class UpdatedSlideContent
{
    //public string Title { get; set; } = string.Empty;
    public List<string> BulletPoints { get; set; } = [];
    //public List<string> Notes { get; set; } = [];
    //public string Summary { get; set; } = string.Empty; // optional
}
