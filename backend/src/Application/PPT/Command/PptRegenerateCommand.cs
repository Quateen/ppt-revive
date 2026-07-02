using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PPTRevive.Application.Common.Models;

namespace PPTRevive.Application.PPT.Command;

public class PptRegenerateCommand : IRequest<ResponseBase>
{
    public string OriginalText { get; set; } = string.Empty;
}

public class PptRegenerateCommandHandler : IRequestHandler<PptRegenerateCommand, ResponseBase>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public PptRegenerateCommandHandler(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<ResponseBase> Handle(PptRegenerateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var updatedText = await GetRegeneratedContentFromClaude(request.OriginalText, cancellationToken);

            var response = new SlidePagesResponse
            {
                OriginalSlideContent = request.OriginalText,
                UpdatedSlideContent = updatedText,
                References = []
            };

            return new ResponseBase
            {
                Status = true,
                Data = response
            };
        }
        catch (Exception ex)
        {
            return new ResponseBase
            {
                Status = false,
                Error = ex.Message
            };
        }
    }

    private async Task<string> GetRegeneratedContentFromClaude(string originalText, CancellationToken cancellationToken)
    {
        var apiKey = _configuration["Claude:ApiKey"];
        var model = _configuration["Claude:Model"] ?? "claude-opus-4-8";
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
        httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

        var requestBody = new
        {
            model,
            max_tokens = 1024,
            system = "You are a presentation assistant. Improve the given slide text with clearer, more current phrasing while preserving its meaning. Keep it very short, professional, and ready for display. Respond with the revised slide text only.",
            messages = new[]
            {
                new { role = "user", content = originalText }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync("https://api.anthropic.com/v1/messages", content, cancellationToken);
        if (!response.IsSuccessStatusCode) return originalText;

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(responseJson);

        var text = new StringBuilder();
        foreach (var block in doc.RootElement.GetProperty("content").EnumerateArray())
        {
            if (block.GetProperty("type").GetString() == "text")
            {
                text.Append(block.GetProperty("text").GetString());
            }
        }

        var result = text.ToString().Trim();
        return string.IsNullOrWhiteSpace(result) ? originalText : result;
    }
}
