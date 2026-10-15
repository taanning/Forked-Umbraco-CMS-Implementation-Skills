using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Umbraco_CMS.Skills.TestHost.Blank;

/// <summary>
/// Deterministic runtime validation of umbraco-dictionary-items — backoffice-managed
/// items rendered in Razor via UmbracoHelper.
///
/// Proves the asset the skill ships (assets/_DictionaryLabels.cshtml, seeded as the
/// dictionaryDemo template's content and rendered against the seeded da-DK items) renders:
/// the default-culture lookup with fallback, the explicit-CultureInfo overload against the seeded
/// da-DK language, the missing-key fallback, and CultureDictionary.GetChildren grouping.
///
/// Runs on SITE 2, the host with no starter kit. The *BlankTests.cs suffix is what routes this
/// file into the blank test assembly and therefore into its own process.
/// </summary>
[TestFixture]
public class DictionaryItemsBlankTests
{
    // The node's URL derives from its NAME ("Dictionary Demo"), which the example seeds under the
    // shared fixture root — see ExampleHostWiring.cs.
    private const string PageUrl = "/dictionary-demo/";

    private static HttpClient Client => BlankSiteFixture.Client;

    private static async Task<XDocument> GetPageAsync()
    {
        HttpResponseMessage response = await Client.GetAsync(PageUrl);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            $"GET {PageUrl} should render the Dictionary Demo node");
        Assert.That(response.Content.Headers.ContentType?.MediaType, Is.EqualTo("text/html"),
            "the demo page must be rendered HTML, not an API payload");

        // The template emits a loose HTML document (<!DOCTYPE html> …), which XML can't parse —
        // strip the doctype and extract the <body> fragment, wrapped so the sibling elements have
        // a single root.
        string body = await response.Content.ReadAsStringAsync();
        Match fragment = Regex.Match(body, "<body[^>]*>(.*)</body>", RegexOptions.Singleline);
        Assert.That(fragment.Success, Is.True, "the demo page must render a <body>");
        return XDocument.Parse($"<root>{fragment.Groups[1].Value}</root>");
    }

    private static string? ElementText(XDocument doc, string id) =>
        doc.Descendants()
            .FirstOrDefault(e => e.Attribute("id")?.Value == id)
            ?.Value.Trim();

    [Test]
    public async Task Dictionary_value_renders_for_the_request_culture()
    {
        XDocument doc = await GetPageAsync();

        Assert.That(ElementText(doc, "dict-read-more"), Is.EqualTo("Read more"),
            "guards the skill's core promise: Umbraco.GetDictionaryValueOrDefault resolves the "
            + "request culture's value for Site.ReadMore (en-US on this host)");
    }

    [Test]
    public async Task Explicit_culture_overload_renders_the_other_language()
    {
        XDocument doc = await GetPageAsync();

        Assert.That(ElementText(doc, "dict-search-da"), Is.EqualTo("Søg"),
            "guards the explicit-CultureInfo overload AND that the example seeder created both "
            + "the da-DK language and the da-DK translation of Site.Search");
    }

    [Test]
    public async Task Missing_key_renders_the_fallback_not_empty()
    {
        XDocument doc = await GetPageAsync();

        Assert.That(ElementText(doc, "dict-missing"), Is.EqualTo("fallback text"),
            "guards the fallback contract: GetDictionaryValueOrDefault must emit the default for a "
            + "key that does not exist, not an empty string");
    }

    [Test]
    public async Task GetChildren_lists_the_grouped_items_with_full_keys()
    {
        XDocument doc = await GetPageAsync();

        var keys = doc.Descendants()
            .FirstOrDefault(e => e.Attribute("id")?.Value == "dict-site-children")
            ?.Elements()
            .Select(e => e.Attribute("data-key")?.Value)
            .ToList();

        Assert.That(keys, Is.Not.Null, "the dict-site-children list must render");
        Assert.That(keys, Is.EquivalentTo(
            new[] { "Site.ReadMore", "Site.Search", "Site.Footer.Copyright" }),
            "guards the grouping promise: CultureDictionary.GetChildren(\"Site\") returns the three "
            + "child items by their full dotted keys");
    }
}
