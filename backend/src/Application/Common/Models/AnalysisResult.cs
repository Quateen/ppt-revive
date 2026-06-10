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
}
