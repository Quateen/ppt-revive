using Aspose.Slides;
using DocumentFormat.OpenXml.Presentation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using PPTRevive.Application.Common.Interfaces;
using PPTRevive.Application.Common.Models;
using AsposePresentation = Aspose.Slides.Presentation;

namespace PPTRevive.Infrastructure.Repositories;
public class PPTRepository : IPPTRepository
{
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;

    public PPTRepository(IWebHostEnvironment env, IConfiguration config)
    {
        _env = env;
        _config = config;
    }

    public async Task<string> UploadFileAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty.");

        var uploadPath = Path.Combine(_env.ContentRootPath, "Uploads");
        if (!Directory.Exists(uploadPath))
            Directory.CreateDirectory(uploadPath);

        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        var filePath = Path.Combine(uploadPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return fileName;
    }

    // ---- Extract slide content ----
    public async Task<List<SlideContent>> ExtractSlidesAsync(string fileName)
    {
        return await Task.Run(() =>
        {
            var path = GetFilePath(fileName);
            if (!File.Exists(path)) throw new FileNotFoundException("PPT not found");

            var slides = new List<SlideContent>();
            using var presentation = new AsposePresentation(path);

            for (int i = 0; i < presentation.Slides.Count; i++)
            {
                var slide = presentation.Slides[i];
                var title = slide.Shapes
                    .OfType<IAutoShape>()
                    .FirstOrDefault(s => s.TextFrame != null)?.TextFrame?.Text ?? $"Slide {i + 1}";

                var paragraphs = slide.Shapes
                    .OfType<IAutoShape>()
                    .Where(s => s.TextFrame != null)
                    .SelectMany(s => s.TextFrame.Paragraphs.Select(p => p.Text))
                    .ToList();

                slides.Add(new SlideContent
                {
                    SlideNumber = i + 1,
                    Title = title,
                    Paragraphs = paragraphs
                });
            }

            return slides;
        });
    }

    // ---- Update first slide using AI ----
    public async Task<string> UpdateSlideWithAiAsync(string fileName, string newContext)
    {
        var path = GetFilePath(fileName);
        if (!File.Exists(path)) throw new FileNotFoundException("PPT not found");

        var slides = await ExtractSlidesAsync(fileName);
        var allContent = string.Join("\n", slides.Select(s => $"{s.Title}: {string.Join(", ", s.Paragraphs)}"));

        var updatedContent = await GenerateUpdatedContentAsync(allContent, newContext);
        await UpdateSlideContentAsync(path, 0, updatedContent); // Updating slide 0

        return updatedContent;
    }

    // ---- Regenerate selected slides using AI ----
    public async Task<List<int>> RegenerateSlidesAsync(string fileName, string prompt, List<int>? slideNumbers)
    {
        var path = GetFilePath(fileName);
        if (!File.Exists(path)) throw new FileNotFoundException("PPT not found");

        var slides = await ExtractSlidesAsync(fileName);
        var indices = slideNumbers ?? Enumerable.Range(1, slides.Count).ToList();

        foreach (var i in indices)
        {
            var slide = slides.FirstOrDefault(s => s.SlideNumber == i);
            if (slide == null) continue;

            var content = $"{slide.Title}\n{string.Join("\n", slide.Paragraphs)}";
            var newContent = await RegenerateSlideAsync(content, prompt);

            await UpdateSlideContentAsync(path, i - 1, newContent);
        }

        return indices;
    }

    // ---- Helper to update slide content ----
    private async Task UpdateSlideContentAsync(string path, int slideIndex, string newText)
    {
        await Task.Run(() =>
        {
            using var presentation = new AsposePresentation(path);
            var slide = presentation.Slides[slideIndex];

            foreach (var shape in slide.Shapes.OfType<IAutoShape>())
            {
                if (shape.TextFrame != null)
                {
                    shape.TextFrame.Text = newText;
                    break;
                }
            }

            presentation.Save(path, Aspose.Slides.Export.SaveFormat.Pptx);
        });
    }

    // ---- AI: Generate new content for full deck ----
    private async Task<string> GenerateUpdatedContentAsync(string existingContent, string userPrompt)
    {
        await Task.Delay(300); // Simulated processing
        return $"[AI UPDATED]\nPrompt: {userPrompt}\nContent: {existingContent}";
    }

    // ---- AI: Regenerate specific slide ----
    private async Task<string> RegenerateSlideAsync(string existingSlideContent, string userPrompt)
    {
        await Task.Delay(300); // Simulated processing
        return $"[AI REGENERATED SLIDE]\nPrompt: {userPrompt}\n{existingSlideContent}";
    }

    private string GetFilePath(string fileName) =>
        Path.Combine(_env.ContentRootPath, "Uploads", fileName);
}

