namespace PPTRevive.Application.Common.Models;

public class PPTResponse
{
    public string FileUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public int SlidePageCount { get; set; }
    public DateTime ProcessedAt { get; set; }
    public List<SlidePagesResponse> SlidePages { get; set; } = [];
}

public class SlidePagesResponse
{
    public int SlideId { get; set; }
    public string OriginalSlideContent { get; set; } = string.Empty;
    public string UpdatedSlideContent { get; set; } = string.Empty;
    public string? TitleText { get; set; }
    public List<string> References { get; set; } = new();
    public string Explanation { get; set; } = "";
    public string Source { get; set; } = "";
}
