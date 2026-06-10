namespace PPTRevive.Application.Common.Models;
public class SlideContent
{
    public int SlideNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<string> Paragraphs { get; set; } = new();
}
