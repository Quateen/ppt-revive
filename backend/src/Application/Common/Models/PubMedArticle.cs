namespace PPTRevive.Application.Common.Models;

public class PubMedArticle
{
    public string Pmid { get; set; } = "";
    public string Title { get; set; } = "";
    public string Abstract { get; set; } = "";
    public List<string> Authors { get; set; } = new();
    public string PublicationDate { get; set; } = "";
    public string Journal { get; set; } = "";
    public string? Doi { get; set; }
    public string Link { get; set; } = "";
    public string? PublicationType { get; set; }
}

