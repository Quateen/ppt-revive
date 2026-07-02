using System.Text.Json.Serialization;

namespace PPTRevive.Application.Common.Models;
public class AnalysisResult
{
    [JsonPropertyName("suggestedUpdate")]
    public string SuggestedUpdate { get; set; } = string.Empty;

    [JsonPropertyName("explanation")]
    public string Explanation { get; set; } = string.Empty;

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    public List<SlideTextItem> ParsedItems { get; set; } = new();

    // False when the Claude call or JSON parsing failed; callers must keep the
    // original slide content and must NOT surface Explanation to end users.
    public bool Success { get; set; } = true;
}
