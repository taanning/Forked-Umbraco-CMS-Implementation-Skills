using System.Globalization;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Skills.Examples.Fixtures;

namespace Umbraco.Skills.Examples.DictionaryItems;

/// <summary>
/// This example's own harness — not part of the skill.
///
/// Seeds everything the demo page needs at UmbracoApplicationStarted (RuntimeLevel.Run, after
/// the host is fully up): the da-DK language, the Site dictionary items, the dictionaryDemo
/// template composed from the shipped partial, the dictionaryDemo Document Type, and the page
/// itself as a CHILD of site 2's single shared root. Every step is idempotent because the test
/// database is reused across boots.
/// </summary>
public class ExampleContentSeeder : INotificationAsyncHandler<UmbracoApplicationStartedNotification>
{
    private const string TypeAlias = "dictionaryDemo";

    private readonly ILanguageService _languageService;
    private readonly IDictionaryItemService _dictionaryItemService;
    private readonly ITemplateService _templateService;
    private readonly IContentTypeService _contentTypeService;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly IContentService _contentService;
    private readonly IContentPublishingService _publishingService;
    private readonly IDocumentUrlService _documentUrlService;
    private readonly IDatabaseCacheRebuilder _cacheRebuilder;
    private readonly ILogger<ExampleContentSeeder> _logger;

    public ExampleContentSeeder(
        ILanguageService languageService,
        IDictionaryItemService dictionaryItemService,
        ITemplateService templateService,
        IContentTypeService contentTypeService,
        IShortStringHelper shortStringHelper,
        IContentService contentService,
        IContentPublishingService publishingService,
        IDocumentUrlService documentUrlService,
        IDatabaseCacheRebuilder cacheRebuilder,
        ILogger<ExampleContentSeeder> logger)
    {
        _languageService = languageService;
        _dictionaryItemService = dictionaryItemService;
        _templateService = templateService;
        _contentTypeService = contentTypeService;
        _shortStringHelper = shortStringHelper;
        _contentService = contentService;
        _publishingService = publishingService;
        _documentUrlService = documentUrlService;
        _cacheRebuilder = cacheRebuilder;
        _logger = logger;
    }

    public async Task HandleAsync(
        UmbracoApplicationStartedNotification notification,
        CancellationToken cancellationToken)
    {
        ILanguage? english = await _languageService.GetAsync("en-US");
        ILanguage? danish = await _languageService.GetAsync("da-DK")
            ?? (await _languageService.CreateAsync(
                    new Language("da-DK", CultureInfo.GetCultureInfo("da-DK").EnglishName),
                    Constants.Security.SuperUserKey)).Result;
        if (english is null || danish is null)
        {
            _logger.LogWarning("Languages missing; the dictionary items fixture was not seeded.");
            return;
        }

        await EnsureDictionaryItemsAsync(english, danish);
        ITemplate? template = await EnsureTemplateAsync();
        await EnsureContentTypeAsync(template);

        IContent? root = FixtureSite.FindRoot(_contentService);
        if (root is null)
        {
            _logger.LogWarning(
                "Site 2 has no root node, so the dictionary items fixture content was not seeded.");
            return;
        }

        // The node's NAME becomes its URL segment: "Dictionary Demo" → /dictionary-demo/.
        FixtureSite.EnsureChild(
            _contentService, _contentTypeService, root, TypeAlias, "Dictionary Demo");

        await FixtureSite.PublishAndRefreshAsync(
            _publishingService, _documentUrlService, _cacheRebuilder, root);
    }

    private async Task EnsureItemAsync(
        string key, IDictionaryItem? parent, ILanguage english, ILanguage danish,
        string englishValue, string danishValue)
    {
        if (await _dictionaryItemService.ExistsAsync(key))
        {
            return;
        }

        DictionaryItem item = parent is null
            ? new DictionaryItem(key)
            : new DictionaryItem(parent.Key, key);
        item.Translations = danishValue.Length == 0
            ? [new DictionaryTranslation(english, englishValue)]
            : [new DictionaryTranslation(english, englishValue), new DictionaryTranslation(danish, danishValue)];
        await _dictionaryItemService.CreateAsync(item, Constants.Security.SuperUserKey);
    }

    private async Task EnsureDictionaryItemsAsync(ILanguage english, ILanguage danish)
    {
        // Parents need no values; "Site" gets one anyway so it still renders if asked.
        await EnsureItemAsync("Site", null, english, danish, "Site", string.Empty);
        IDictionaryItem? parent = await _dictionaryItemService.GetAsync("Site");
        if (parent is null)
        {
            return;
        }

        await EnsureItemAsync("Site.ReadMore", parent, english, danish, "Read more", "Læs mere");
        await EnsureItemAsync("Site.Search", parent, english, danish, "Search", "Søg");
        await EnsureItemAsync("Site.Footer.Copyright", parent, english, danish,
            "All rights reserved", "Alle rettigheder forbeholdes");
    }

    /// <summary>
    /// The demo template's content IS the shipped partial wrapped in a page shell — so a changed
    /// asset propagates on the next boot even though the test database is reused.
    /// </summary>
    private async Task<ITemplate?> EnsureTemplateAsync()
    {
        string asset = EmbeddedManifest.Text(typeof(ExampleContentSeeder).Assembly,
            "_DictionaryLabels.cshtml");

        // @inherits must stay first: split after the @inherits/@using pair and wrap the rest.
        string[] lines = asset.Split('\n');
        int head = lines.Length > 1 && lines[1].TrimStart().StartsWith("@using") ? 2 : 1;
        string content = string.Join('\n', lines.Take(head))
            + "\n@{ Layout = null; }<!DOCTYPE html>\n<html><body><h1>@Model.Name</h1>\n"
            + string.Join('\n', lines.Skip(head))
            + "\n</body></html>\n";

        ITemplate? template = await _templateService.GetAsync(TypeAlias);
        if (template is null)
        {
            await _templateService.CreateAsync(
                new Template(_shortStringHelper, "Dictionary Demo", TypeAlias) { Content = content },
                Constants.Security.SuperUserKey);
            return await _templateService.GetAsync(TypeAlias);
        }

        if (template.Content != content)
        {
            template.Content = content;
            await _templateService.UpdateAsync(template, Constants.Security.SuperUserKey);
        }

        return template;
    }

    private async Task EnsureContentTypeAsync(ITemplate? template)
    {
        IContentType? type = _contentTypeService.Get(TypeAlias);
        if (type is null)
        {
            type = new ContentType(_shortStringHelper, -1)
            {
                Alias = TypeAlias,
                Name = "Dictionary Demo",
                AllowedAsRoot = false,
            };
            if (template is not null)
            {
                type.AllowedTemplates = [template];
                type.SetDefaultTemplate(template);
            }

            await _contentTypeService.CreateAsync(type, Constants.Security.SuperUserKey);
            return;
        }

        if (template is null
            || (type.IsAllowedTemplate(TypeAlias) && type.DefaultTemplateId == template.Id))
        {
            return;
        }

        type.AllowedTemplates = (type.AllowedTemplates ?? []).Append(template);
        type.SetDefaultTemplate(template);
        await _contentTypeService.UpdateAsync(type, Constants.Security.SuperUserKey);
    }
}

/// <summary>Registers the seeder. Picked up by AddComposers() in the blank host's Program.cs.</summary>
public class ExampleHostWiringComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder) =>
        builder.AddNotificationAsyncHandler<UmbracoApplicationStartedNotification, ExampleContentSeeder>();
}
