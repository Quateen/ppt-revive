using PPTRevive.Application.Common.Models;

namespace PPTRevive.Application.Common.Interfaces;
public interface IPubMedRepository
{
    Task<List<PubMedArticle>> SearchRelevantArticlesAsync(string query);
    Task<AnalysisResult> AnalyzeMedicalSlideAsync(List<SlideTextItem> items, List<PubMedArticle> articles);
    Task<string> ExtractKeyMedicalTerms(string content);
}
