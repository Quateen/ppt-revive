using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using PPTRevive.Application.Common.Interfaces;
using PPTRevive.Application.Common.Models;

namespace PPTRevive.Infrastructure.Repositories;

public class PubMedRepository : IPubMedRepository
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly JsonSerializerOptions _jsonOptions;

    public PubMedRepository(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    public async Task<List<PubMedArticle>> SearchRelevantArticlesAsync(string query)
    {
        var client = _httpClientFactory.CreateClient();
        var apiKey = _configuration["PubMed:ApiKey"];
        var baseUrl = "https://eutils.ncbi.nlm.nih.gov/entrez/eutils";

        // Constrain for quality: abstracts only, English. The query comes from Claude
        // keyword extraction, so no hardcoded domain enrichment is applied here.
        var term = $"{query} AND hasabstract[text] AND english[lang]";
        var searchUrl = $"{baseUrl}/esearch.fcgi?db=pubmed&term={Uri.EscapeDataString(term)}&retmax=8&sort=pub+date&api_key={apiKey}";

        Console.WriteLine("🔍 Searching PubMed for query: " + term);

        try
        {
            var searchResponse = await client.GetStringAsync(searchUrl);
            var xml = XDocument.Parse(searchResponse);
            var ids = xml.Descendants("Id").Select(x => x.Value).ToList();

            if (!ids.Any())
            {
                Console.WriteLine("⚠️ No PubMed IDs found for query.");
                return new List<PubMedArticle>();
            }

            var articles = new List<PubMedArticle>();
            foreach (var id in ids)
            {
                var article = await GetArticleDetailsAsync(client, id, apiKey ?? string.Empty);
                if (article != null)
                {
                    articles.Add(article);
                }

                // Respect NCBI rate limit: 3 requests/sec
                await Task.Delay(334);
            }

            Console.WriteLine($"✅ Retrieved {articles.Count} articles from PubMed.");
            return articles;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error searching PubMed: {ex.Message}");
            return new List<PubMedArticle>();
        }
    }

    private async Task<PubMedArticle?> GetArticleDetailsAsync(HttpClient client, string pmid, string apiKey)
    {
        var fetchUrl = $"https://eutils.ncbi.nlm.nih.gov/entrez/eutils/efetch.fcgi?db=pubmed&id={pmid}&retmode=xml&api_key={apiKey}";

        try
        {
            var response = await client.GetStringAsync(fetchUrl);
            var xdoc = XDocument.Parse(response);

            var pm = xdoc.Descendants("PubmedArticle").FirstOrDefault();
            if (pm is null) return null;

            var medlineCitation = pm.Element("MedlineCitation");
            var articleEl = medlineCitation?.Element("Article");
            var journalEl = articleEl?.Element("Journal");

            string journal =
                journalEl?.Element("Title")?.Value?.Trim()
                ?? journalEl?.Element("ISOAbbreviation")?.Value?.Trim()
                ?? medlineCitation?.Element("MedlineJournalInfo")?.Element("MedlineTA")?.Value?.Trim()
                ?? string.Empty;

            string articleTitle = articleEl?.Element("ArticleTitle")?.Value?.Trim() ?? string.Empty;
            string abstractText = ExtractAbstract(articleEl);

            var authors = pm
                .Descendants("Author")
                .Select(a =>
                {
                    var last = a.Element("LastName")?.Value?.Trim();
                    var fore = a.Element("ForeName")?.Value?.Trim();
                    var collab = a.Element("CollectiveName")?.Value?.Trim();
                    return !string.IsNullOrWhiteSpace(collab)
                        ? collab
                        : $"{last} {fore}".Trim();
                })
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct()
                .ToList();

            string pubDate = ExtractBestPubDate(articleEl, medlineCitation);

            string? doi = pm
                .Descendants("ArticleId")
                .FirstOrDefault(n => (string?)n.Attribute("IdType") == "doi")
                ?.Value;

            return new PubMedArticle
            {
                Pmid = pmid,
                Title = articleTitle,
                Abstract = string.IsNullOrWhiteSpace(abstractText) ? "No abstract available" : abstractText,
                Authors = authors,
                PublicationDate = pubDate,
                Journal = string.IsNullOrWhiteSpace(journal) ? "Unknown Journal" : journal,
                Doi = doi,
                Link = $"https://pubmed.ncbi.nlm.nih.gov/{pmid}/"
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error fetching article details for PMID {pmid}: {ex.Message}");
            return null;
        }
    }

    private static string ExtractAbstract(XElement? articleEl)
    {
        var abs = articleEl?.Element("Abstract");
        if (abs is null) return string.Empty;

        var parts = abs.Elements("AbstractText").Select(at =>
        {
            var label = (string?)at.Attribute("Label");
            var text = at.Value?.Trim();
            return string.IsNullOrWhiteSpace(label) ? text : $"{label}: {text}";
        })
        .Where(s => !string.IsNullOrWhiteSpace(s));

        var joined = string.Join(" ", parts);
        return NormalizeWhitespace(joined);
    }

    private static string ExtractBestPubDate(XElement? articleEl, XElement? medlineCitation)
    {
        var pubDateEl = articleEl?.Element("Journal")?.Element("JournalIssue")?.Element("PubDate");
        var year = pubDateEl?.Element("Year")?.Value;
        var month = pubDateEl?.Element("Month")?.Value;
        var day = pubDateEl?.Element("Day")?.Value;
        if (!string.IsNullOrWhiteSpace(year))
        {
            var parts = new[] { year, month, day }.Where(p => !string.IsNullOrWhiteSpace(p));
            return string.Join("-", parts);
        }

        var medlineDate = pubDateEl?.Element("MedlineDate")?.Value;
        if (!string.IsNullOrWhiteSpace(medlineDate)) return medlineDate.Trim();

        var articleDate = articleEl?.Elements("ArticleDate").FirstOrDefault();
        if (articleDate != null)
        {
            var y = articleDate.Element("Year")?.Value;
            var m = articleDate.Element("Month")?.Value;
            var d = articleDate.Element("Day")?.Value;
            var parts = new[] { y, m, d }.Where(p => !string.IsNullOrWhiteSpace(p));
            var val = string.Join("-", parts);
            if (!string.IsNullOrWhiteSpace(val)) return val;
        }

        return "N/A";
    }

    private static string NormalizeWhitespace(string s)
        => Regex.Replace(s ?? string.Empty, @"\s+", " ").Trim();

    /// <summary>
    /// Analyzes medical slide content using Claude API (replacing GPT-4).
    /// Uses Claude's structured output for reliable JSON responses.
    /// </summary>
    public async Task<AnalysisResult> AnalyzeMedicalSlideAsync(List<SlideTextItem> items, List<PubMedArticle> articles)
    {
        var apiKey = _configuration["Claude:ApiKey"];
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
        httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

        var pubmedContext = string.Join("\n---\n", articles.Select(a => $"""
        Title: {a.Title}
        Abstract: {a.Abstract}
        Authors: {string.Join(", ", a.Authors)}
        Journal: {a.Journal} ({a.PublicationDate})
        DOI: {a.Doi ?? "N/A"}
        Link: {a.Link}
    """));

        var lineInstructions = string.Join("\n", items.Select(i =>
            $"- \"{i.Text}\" (SlideIndex: {i.SlideIndex}, Bullet: {i.IsBullet}, Shape: \"{i.ShapeName}\", Footer: {i.IsAuthorOrFooter})"));

        var statsHints = string.Join("\n", items.Select(i =>
            $"- L{i.SlideIndex}: {ExtractNumbersAsHint(i.Text)}"));

        var systemPrompt = """
        You are a medical presentation updater.

        TASK
        For each input line, revise the text to be clinically sound and up-to-date using ONLY the provided PubMed article abstracts.
        If you do not find a perfect direct match, you MUST still improve the line with the most relevant, generalizable facts from the provided abstracts.
        Only leave a line unchanged if absolutely no relevant medical detail can be drawn from any provided abstract.

        PRESERVE
        - 1:1 mapping: number of output objects MUST equal the number of input lines
        - slideIndex, shapeName, isBullet, isAuthorOrFooter
        - Bullet structure (level 0 = top-level; level 1+ nested)
        - Tone and length (max 20% longer)

        STAT VALIDATION
        - If a line contains statistics, verify from abstracts or keep original unchanged.
        - Never invent numbers not present in the provided abstracts.

        OUTPUT
        Respond with a JSON object matching the provided schema: suggestedUpdate (one object
        per input line with slideIndex, text, shapeName, isBullet, isAuthorOrFooter, level),
        explanation (specific changes and why), and source (evidence summary).
        """;

        var userPrompt = $"""
        Slide content to revise:
        {lineInstructions}

        Potential numeric/stat hints from the original lines:
        {statsHints}

        PubMed Articles:
        {pubmedContext}
        """;

        // Structured output schema: guarantees valid JSON in the response text block,
        // replacing the old regex/curly-quote cleanup of free-form model output.
        var slideItemSchema = new
        {
            type = "object",
            properties = new Dictionary<string, object>
            {
                ["slideIndex"] = new { type = "integer" },
                ["text"] = new { type = "string" },
                ["shapeName"] = new { type = "string" },
                ["isBullet"] = new { type = "boolean" },
                ["isAuthorOrFooter"] = new { type = "boolean" },
                ["level"] = new { type = "integer" }
            },
            required = new[] { "slideIndex", "text", "shapeName", "isBullet", "isAuthorOrFooter", "level" },
            additionalProperties = false
        };

        var outputSchema = new
        {
            type = "object",
            properties = new Dictionary<string, object>
            {
                ["suggestedUpdate"] = new { type = "array", items = slideItemSchema },
                ["explanation"] = new { type = "string" },
                ["source"] = new { type = "string" }
            },
            required = new[] { "suggestedUpdate", "explanation", "source" },
            additionalProperties = false
        };

        // Build Claude API request body. Model is configurable; sampling parameters are
        // intentionally omitted (removed on current Opus-tier models).
        var body = new
        {
            model = GetConfiguredModel(),
            max_tokens = 8192,
            system = systemPrompt,
            output_config = new
            {
                format = new
                {
                    type = "json_schema",
                    schema = outputSchema
                }
            },
            messages = new[]
            {
                new { role = "user", content = userPrompt }
            }
        };

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(body, _jsonOptions),
            Encoding.UTF8,
            "application/json"
        );

        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsync("https://api.anthropic.com/v1/messages", jsonContent);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Claude API Error: HTTP {response.StatusCode}: {errorBody}");
                return CreateErrorResult("[]", "Claude API", $"HTTP {response.StatusCode}: {errorBody}");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"❌ Network error calling Claude: {e.Message}");
            return CreateErrorResult("[]", "Network", e.ToString());
        }

        string rawContent;
        try
        {
            var responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine("🔵 Full Claude Response JSON:\n" + responseString);

            // Claude API response structure: { content: [{ type: "text", text: "..." }] }
            using var doc = JsonDocument.Parse(responseString);
            var contentArray = doc.RootElement.GetProperty("content");
            
            rawContent = "";
            foreach (var block in contentArray.EnumerateArray())
            {
                if (block.GetProperty("type").GetString() == "text")
                {
                    rawContent += block.GetProperty("text").GetString() ?? "";
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error parsing Claude response: {ex.Message}");
            return CreateErrorResult("[]", "Parse", ex.ToString());
        }

        string cleanedJson = CleanJsonResponse(rawContent);
        if (string.IsNullOrWhiteSpace(cleanedJson))
        {
            Console.WriteLine("❌ Empty cleaned JSON from Claude response");
            return CreateErrorResult("[]", "AI Output", "Empty cleaned JSON");
        }

        try
        {
            using var doc = JsonDocument.Parse(cleanedJson);
            var root = doc.RootElement;

            var explanation = root.GetProperty("explanation").GetString() ?? "";
            var source = root.GetProperty("source").GetString() ?? "";
            var updateArray = root.GetProperty("suggestedUpdate");

            ValidateJsonStructure(updateArray.ToString());

            var updatedItems = JsonSerializer.Deserialize<List<SlideTextItem>>(
                updateArray.ToString(),
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
                }
            ) ?? new();

            // Remove duplicated title if present
            string? titleLine = items.FirstOrDefault(x => x.ShapeName.ToLower().Contains("title"))?.Text;
            if (!string.IsNullOrWhiteSpace(titleLine))
            {
                updatedItems = updatedItems
                    .Where(item => item.Text.Trim() != titleLine.Trim())
                    .ToList();
            }

            // Numeric integrity guard
            updatedItems = EnsureNumericIntegrity(items, updatedItems, explanation);

            Console.WriteLine($"✅ Claude analysis complete. Updated {updatedItems.Count} items.");

            return new AnalysisResult
            {
                SuggestedUpdate = JsonSerializer.Serialize(updatedItems, _jsonOptions),
                ParsedItems = updatedItems,
                Explanation = explanation,
                Source = source
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error processing Claude JSON: {ex.Message}");
            return CreateErrorResult(cleanedJson, "Post-Parse", ex.ToString());
        }
    }

    /// <summary>
    /// Extracts key medical terms from content using Claude API for better PubMed queries.
    /// </summary>
    public async Task<string> ExtractKeyMedicalTerms(string content)
    {
        var apiKey = _configuration["Claude:ApiKey"];
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
        httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

        var body = new
        {
            model = GetConfiguredModel(),
            max_tokens = 300,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = $"""
                    You are a medical research expert. Extract key medical terms, concepts, and potential research queries from the given text.
                    Focus on:
                    1. Medical conditions and treatments
                    2. Clinical procedures
                    3. Drug names and classes
                    4. Recent medical developments
                    5. Clinical guidelines

                    Return only a comma-separated list of terms or a concise search query string without any explanation.

                    Extract search terms from: "{content}"
                    """
                }
            }
        };

        try
        {
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(body, _jsonOptions),
                Encoding.UTF8,
                "application/json"
            );

            var response = await httpClient.PostAsync("https://api.anthropic.com/v1/messages", jsonContent);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            
            var contentArray = doc.RootElement.GetProperty("content");
            foreach (var block in contentArray.EnumerateArray())
            {
                if (block.GetProperty("type").GetString() == "text")
                {
                    var query = block.GetProperty("text").GetString()?.Trim();
                    if (!string.IsNullOrWhiteSpace(query))
                    {
                        Console.WriteLine($"✅ Extracted medical terms: {query}");
                        return query;
                    }
                }
            }

            return content;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Fallback to original content due to error: {ex.Message}");
            return content;
        }
    }

    // ------------------------
    // Helper Methods
    // ------------------------

    private string GetConfiguredModel()
        => _configuration["Claude:Model"] ?? "claude-opus-4-8";

    // Defensive fallback only: structured outputs guarantee valid JSON, but if the model
    // ever responds without it (e.g. refusal), strip markdown fences and smart quotes.
    private string CleanJsonResponse(string rawContent)
    {
        string cleaned = rawContent
            .Replace("\u201C", "\"")
            .Replace("\u201D", "\"")
            .Replace("\u2018", "'")
            .Replace("\u2019", "'");

        cleaned = Regex.Replace(cleaned, @"^```(?:json)?|```$", "", RegexOptions.IgnoreCase).Trim();

        var match = Regex.Match(cleaned, @"(\{[\s\S]*\}|\[[\s\S]*\])", RegexOptions.Singleline);
        return match.Success ? match.Value : rawContent;
    }

    private void ValidateJsonStructure(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            foreach (var element in doc.RootElement.EnumerateArray())
            {
                if (!element.TryGetProperty("slideIndex", out _) ||
                    !element.TryGetProperty("text", out _) ||
                    !element.TryGetProperty("shapeName", out _) ||
                    !element.TryGetProperty("isBullet", out _) ||
                    !element.TryGetProperty("isAuthorOrFooter", out _) ||
                    !element.TryGetProperty("level", out _))
                {
                    throw new JsonException("Missing required fields in slide object.");
                }
            }
        }
        catch (Exception ex)
        {
            throw new JsonException("Invalid JSON structure: " + ex.Message);
        }
    }

    private AnalysisResult CreateErrorResult(string content, string source, string errorDetails)
    {
        Console.WriteLine($"❌ Error [{source}]: {errorDetails}");
        return new AnalysisResult
        {
            SuggestedUpdate = content,
            ParsedItems = new(),
            Explanation = errorDetails,
            Source = source
        };
    }

    private static string ExtractNumbersAsHint(string text)
    {
        var nums = ExtractNumbers(text);
        return nums.Count == 0 ? "(no numeric content detected)" : string.Join(", ", nums);
    }

    private static List<string> ExtractNumbers(string s)
    {
        var matches = Regex.Matches(s, @"\b\d+(\.\d+)?\s?(%|percent)?\b|\b\d+(\.\d+)?\s?(\-|–|to)\s?\d+(\.\d+)?\b", RegexOptions.IgnoreCase);
        return matches.Select(m => m.Value.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static List<SlideTextItem> EnsureNumericIntegrity(
        List<SlideTextItem> original,
        List<SlideTextItem> updated,
        string explanation)
    {
        bool explanationHasNumbers = Regex.IsMatch(explanation ?? "", @"\d");
        var safe = new List<SlideTextItem>(updated.Count);

        for (int i = 0; i < updated.Count; i++)
        {
            var src = original.ElementAtOrDefault(i);
            var upd = updated[i];

            if (src == null)
            {
                safe.Add(upd);
                continue;
            }

            var srcNums = ExtractNumbers(src.Text ?? "");
            var updNums = ExtractNumbers(upd.Text ?? "");
            var changed = NumbersChanged(srcNums, updNums);

            if (changed && !explanationHasNumbers)
            {
                // Revert text only; keep mapping/shape
                safe.Add(new SlideTextItem
                {
                    SlideIndex = upd.SlideIndex,
                    Text = src.Text ?? string.Empty,
                    ShapeName = upd.ShapeName,
                    IsBullet = upd.IsBullet,
                    IsAuthorOrFooter = upd.IsAuthorOrFooter,
                    Level = upd.Level
                });
            }
            else
            {
                safe.Add(upd);
            }
        }

        return safe;
    }

    private static bool NumbersChanged(List<string> a, List<string> b)
    {
        if (a.Count == 0 && b.Count == 0) return false;
        if (a.Count != b.Count) return true;

        var aa = a.Select(x => x.ToLowerInvariant()).OrderBy(x => x).ToList();
        var bb = b.Select(x => x.ToLowerInvariant()).OrderBy(x => x).ToList();

        for (int i = 0; i < aa.Count; i++)
        {
            if (!aa[i].Equals(bb[i], StringComparison.Ordinal)) return true;
        }

        return false;
    }
}
