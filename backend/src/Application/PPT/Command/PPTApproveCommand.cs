using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using Microsoft.Extensions.Caching.Memory;
using PPTRevive.Application.Common.Interfaces;
using PPTRevive.Application.Common.Models;
using PPTRevive.Domain.Constants;
using PPTRevive.Domain.Entities;
using A = DocumentFormat.OpenXml.Drawing;
using P = DocumentFormat.OpenXml.Presentation;
using ShapeModel = DocumentFormat.OpenXml.Presentation.Shape;

namespace PPTRevive.Application.PPT.Command;

public class PPTApproveCommand : IRequest<ResponseBase>
{
    public string Id { get; set; } = string.Empty;
    public List<ApprovedSlideDto> Slides { get; set; } = new();
}

public class ApprovedSlideDto
{
    public int Id { get; set; }
    public bool IsApproved { get; set; }

    // When the user edits the suggested update before approving, the edited text is
    // sent here and takes precedence over the AI-suggested content.
    public string? EditedContent { get; set; }
}

public class FinalizeApprovedSlidesHandler : IRequestHandler<PPTApproveCommand, ResponseBase>
{
    private readonly IMemoryCache _cache;
    private readonly IFileService _fileService;
    private readonly IUser _currentUser;

    public FinalizeApprovedSlidesHandler(IMemoryCache cache, IFileService fileService, IUser currentUser)
    {
        _cache = cache;
        _fileService = fileService;
        _currentUser = currentUser;
    }

    public async Task<ResponseBase> Handle(PPTApproveCommand request, CancellationToken cancellationToken)
    {
        // Job must exist, be completed, and belong to the current user.
        if (!_cache.TryGetValue(request.Id, out ProcessingResult? result)
            || result == null
            || result.OwnerUserId != _currentUser.Id
            || result.Status != ProcessingStatus.Completed)
        {
            return new ResponseBase
            {
                Status = false,
                Error = "Processing not completed or job not found."
            };
        }

        // Defend against a completed-but-empty result (e.g. all slides failed).
        if (result.Result is not PPTResponse slideInfo)
        {
            return new ResponseBase
            {
                Status = false,
                Error = "No processed slides are available to finalize."
            };
        }
        var slideResponses = slideInfo.SlidePages;

        // Only citations backing APPROVED (or edited-and-approved) slides go into the
        // final references slides.
        var approvedSlideIds = request.Slides
                            .Where(s => s.IsApproved)
                            .Select(s => s.Id)
                            .ToHashSet();
        var allReferences = slideResponses
                            .Where(x => approvedSlideIds.Contains(x.SlideId))
                            .SelectMany(x => x.References ?? Enumerable.Empty<string>())
                            .Distinct()
                            .Where(text => !string.IsNullOrWhiteSpace(text))
                            .ToList();

        var pptBytes = await _fileService.ReadFileAsBytesAsync(slideInfo.FileName, PPTDirectories.ORIGNAL_PPT);

        using var sourceStream = new MemoryStream(pptBytes);
        using var outputStream = new MemoryStream();
        await sourceStream.CopyToAsync(outputStream);
        outputStream.Position = 0;

        using (var presentation = PresentationDocument.Open(outputStream, true))
        {
            var presentationPart = presentation.PresentationPart!;
            var slideIdList = presentationPart.Presentation.SlideIdList!;
            var slidePartsOrdered = slideIdList.Elements<P.SlideId>()
                .Select(sid => (SlidePart)presentationPart.GetPartById(sid.RelationshipId!))
                .ToList();

            if (slidePartsOrdered == null || slidePartsOrdered.Count == 0)
            {
                return new ResponseBase
                {
                    Status = false,
                    Error = "Invalid presentation structure."
                };
            }

            foreach (var slideRequest in request.Slides)
            {
                if (!slideRequest.IsApproved)
                    continue;

                var slideData = slideResponses.FirstOrDefault(s => s.SlideId == slideRequest.Id);
                if (slideData == null)
                    continue;

                var contentToApply = !string.IsNullOrWhiteSpace(slideRequest.EditedContent)
                    ? slideRequest.EditedContent
                    : slideData.UpdatedSlideContent;
                if (string.IsNullOrWhiteSpace(contentToApply))
                    continue;

                var slideIndex = slideRequest.Id - 1;
                if (slideIndex < 0 || slideIndex >= slidePartsOrdered.Count)
                    continue;

                var slidePart = slidePartsOrdered[slideIndex];

                UpdateSlideContent(slidePart, contentToApply);
                RestoreMetaText(slidePart,
titleText: slideData.TitleText ?? slideData.OriginalSlideContent.Split('\n').FirstOrDefault() ?? "",
                    authorText: ExtractAuthorLine(slideData.OriginalSlideContent));
            }

            if (allReferences != null && allReferences.Count > 0)
            {
                var referenceSlideTemplate = slidePartsOrdered.Last(); // or pick a standard layout slide
                AddReferenceSlides(presentationPart, allReferences, referenceSlideTemplate, 12);
            }

            // ✅ Save presentation and reset stream before writing to disk
            presentation.PresentationPart?.Presentation?.Save();
        }

        outputStream.Position = 0;

        // Per-job filename: concurrent users must never overwrite each other's output.
        var newFileName = $"{request.Id}-revived.pptx";
        await _fileService.SaveFile(newFileName, outputStream.ToArray(), PPTDirectories.UPDATED_PPT);

        return new ResponseBase
        {
            Status = true,
            Data = new
            {
                // Authenticated, ownership-checked download endpoint (not a static URL).
                NewFilePath = $"/api/ppt/download?jobId={request.Id}",
            }
        };
    }

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

    private ShapeModel? FindMainContentShape(List<ShapeModel> textShapes)
    {
        if (textShapes == null || !textShapes.Any()) return null;

        var shapesByParagraphCount = textShapes
            .Where(s =>
            {
                var name = s.NonVisualShapeProperties?.NonVisualDrawingProperties?.Name?.Value?.ToLower() ?? "";
                return !name.Contains("title"); // ❌ Exclude title shapes
            })
            .Select(shape => new
            {
                Shape = shape,
                ParagraphCount = shape.TextBody?.Descendants<A.Paragraph>().Count() ?? 0
            })
            .OrderByDescending(x => x.ParagraphCount);

        var bestMatch = shapesByParagraphCount.FirstOrDefault();
        return bestMatch?.Shape;
    }


    private void SetSpacePreservation(A.Text textElement)
    {
        textElement.SetAttribute(new OpenXmlAttribute(
            "space",
            "http://www.w3.org/XML/1998/namespace",
            "preserve"
        ));
    }

    private static string ExtractAuthorLine(string fullText)
    {
        var lines = fullText.Split('\n');
        return lines.Length >= 2 ? lines.LastOrDefault()?.Trim() ?? "" : "";
    }


    private List<string> SmartFitLines(List<string> lines, int maxBlocks)
    {
        if (lines.Count <= maxBlocks) return lines;

        var safeLines = lines.Take(maxBlocks - 1).ToList();
        var last = string.Join(" ", lines.Skip(maxBlocks - 1));
        safeLines.Add(last);

        return safeLines;
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

    private void AddReferenceSlides(
    PresentationPart presentationPart,
    List<string> allReferences,
    SlidePart referenceSlideTemplate,
    int maxBulletPointsPerSlide = 10)
    {
        var slideIdList = presentationPart.Presentation.SlideIdList!;
        var existingSlideIds = slideIdList.Elements<P.SlideId>().Select(x => x.Id!.Value).ToList();

        var referenceChunks = allReferences
            .Select((refText, i) => new { refText, index = i / maxBulletPointsPerSlide })
            .GroupBy(x => x.index)
            .Select(g => g.Select(x => x.refText).ToList())
            .ToList();

        foreach (var group in referenceChunks)
        {
            var newSlidePart = presentationPart.AddNewPart<SlidePart>();

            // Build the reference slide from scratch rather than cloning the template's
            // slide XML. Cloning would carry over background/transition/timing nodes that
            // reference image or media relationships which don't exist on the new part,
            // producing a corrupt package. A minimal shape tree avoids all dangling refs.
            var shapeTree = new P.ShapeTree(
                new P.NonVisualGroupShapeProperties(
                    new P.NonVisualDrawingProperties { Id = 1U, Name = string.Empty },
                    new P.NonVisualGroupShapeDrawingProperties(),
                    new P.ApplicationNonVisualDrawingProperties()),
                new P.GroupShapeProperties(new A.TransformGroup()));

            newSlidePart.Slide = new P.Slide(new P.CommonSlideData(shapeTree));

            {
                var titleShape = CreateTextShape(
                    "References",
                    0, 0,
                    914400 * 10,
                    914400 * 1,
                    fontSize: 2400,
                    isTitle: true
                );
                shapeTree.Append(titleShape);

                var bodyShape = CreateTextShape(
                    "",
                    914400 * 1, 0,
                    914400 * 10,
                    914400 * 5,
                    fontSize: 1600
                );
                shapeTree.Append(bodyShape);

                var layoutPart = referenceSlideTemplate.SlideLayoutPart;
                if (layoutPart != null)
                {
                    newSlidePart.AddPart(layoutPart);
                }

                uint newSlideId = existingSlideIds.Max() + 1;
                string relIdNewSlide = presentationPart.GetIdOfPart(newSlidePart);
                slideIdList.Append(new P.SlideId { Id = newSlideId, RelationshipId = relIdNewSlide });
                existingSlideIds.Add(newSlideId);

                if (bodyShape.TextBody != null)
                {
                    bodyShape.TextBody.RemoveAllChildren<A.Paragraph>();

                    foreach (var fullCitation in group)
                    {
                        string title = ExtractTitleFromCitation(fullCitation);
                        string link = ExtractLinkFromCitation(fullCitation);

                        var para = new A.Paragraph
                        {
                            ParagraphProperties = new A.ParagraphProperties
                            {
                                Level = 0,
                                LeftMargin = 342900,
                                Indent = -285750,
                                Alignment = A.TextAlignmentTypeValues.Left
                            }
                        };
                        para.ParagraphProperties.Append(new A.CharacterBullet { Char = "•" });

                        // 📌 Title Run
                        var titleRun = new A.Run(
                            new A.RunProperties { FontSize = 1600 },
                            new A.Text(Truncate(title, 80))
                        );
                        if (titleRun.GetFirstChild<A.Text>() is A.Text titleText)
                            SetSpacePreservation(titleText);
                        para.Append(titleRun);

                        // 📌 Hyperlinked Run
                        if (!string.IsNullOrWhiteSpace(link))
                        {
                            var hyperlinkRel = newSlidePart.AddHyperlinkRelationship(new Uri(link), true);
                            string hyperlinkId = hyperlinkRel.Id;

                            var linkRun = new A.Run();
                            var runProps = new A.RunProperties
                            {
                                FontSize = 1600,
                                Underline = A.TextUnderlineValues.Single
                            };
                            runProps.Append(new A.SolidFill(new A.RgbColorModelHex { Val = "0000FF" }));
                            runProps.AppendChild(new A.HyperlinkOnClick { Id = hyperlinkId, Tooltip = "Open Source" });

                            linkRun.Append(runProps);
                            linkRun.Append(new A.Text(" [link]"));
                            if (linkRun.GetFirstChild<A.Text>() is A.Text linkText)
                                SetSpacePreservation(linkText);

                            para.Append(linkRun);

                        }

                        bodyShape.TextBody.Append(para);
                    }
                }
            }
        }

    }

    // ✅ Helper method to create title or content shapes
    private static P.Shape CreateTextShape(
        string text,
        long y, long x,
        long cx, long cy,
        int fontSize,
        bool isTitle = false)
    {
        var nvProps = new P.NonVisualShapeProperties(
            new P.NonVisualDrawingProperties { Id = 1U, Name = "TextBox" },
            new P.NonVisualShapeDrawingProperties(),
            isTitle
                ? new P.ApplicationNonVisualDrawingProperties(
                      new P.PlaceholderShape { Type = PlaceholderValues.Title })
                : new P.ApplicationNonVisualDrawingProperties());

        var spPr = new P.ShapeProperties(
            new A.Transform2D(
                new A.Offset { X = x, Y = y },
                new A.Extents { Cx = cx, Cy = cy }));

        var run = new A.Run(
            new A.RunProperties { FontSize = fontSize },
            new A.Text(text));
        var para = new A.Paragraph(run);

        var bodyProps = isTitle
    ? new A.BodyProperties()
    : new A.BodyProperties
    {
        LeftInset = 914400 / 2,   // 0.5 inch
        RightInset = 914400 / 2   // 0.5 inch
    };

        var textBody = new P.TextBody(
            bodyProps,
            new A.ListStyle(),
            para);


        return new P.Shape(nvProps, spPr, textBody);
    }

    private List<string> SmartWrap(string input, int maxLength = 100)
    {
        var words = input.Split(' ');
        var lines = new List<string>();
        var currentLine = new List<string>();
        int currentLength = 0;

        foreach (var word in words)
        {
            if (currentLength + word.Length + 1 > maxLength)
            {
                lines.Add(string.Join(" ", currentLine));
                currentLine.Clear();
                currentLength = 0;
            }
            currentLine.Add(word);
            currentLength += word.Length + 1;
        }

        if (currentLine.Count > 0)
            lines.Add(string.Join(" ", currentLine));

        return lines;
    }

    private string ExtractLinkFromCitation(string citation)
    {
        var match = System.Text.RegularExpressions.Regex.Match(citation, @"https?://[^\s]+");
        return match.Success ? match.Value : "";
    }

    private string ExtractTitleFromCitation(string citation)
    {
        var match = System.Text.RegularExpressions.Regex.Match(citation, "\"(.*?)\"");
        return match.Success ? match.Groups[1].Value : "Untitled";
    }

    private string Truncate(string input, int maxLength = 80)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        return input.Length <= maxLength ? input : input.Substring(0, maxLength - 3) + "...";
    }



}
