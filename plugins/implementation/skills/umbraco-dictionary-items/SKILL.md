---
name: umbraco-dictionary-items
description: >
  Render translatable labels and static text in Umbraco 17+ Razor templates
  with dictionary items — the per-language strings editors manage in the
  backoffice Translation section. Use whenever the user asks to translate
  labels, button text or static text in templates, to "use dictionary items",
  calls GetDictionaryValue / GetDictionaryValueOrDefault, wants to translate
  dropdown options, or needs multilingual labels. SKIP: non-Umbraco projects,
  Umbraco < 17, translating *content* properties — that is language variants,
  not dictionary items — and backoffice extension/localization files, which
  belong to the backoffice skills.
---

# Dictionary items

A dictionary item is a named, per-language string editors manage under
Translation > Dictionary — for static UI text (labels, buttons, headings), not
content. Content that varies per language belongs in a language-variant property
on the Document Type; dictionary items are for UI chrome.

## Workflow

1. **Decide the keys** using the dotted naming convention: one parent group item
   (e.g. `Site`), children named `Group.Thing` (`Site.ReadMore`,
   `Site.Footer.Copyright`). Keys are case-sensitive identifiers referenced from
   code, so treat them like constants; grouping keeps the Translation tree
   navigable and lets `Umbraco.CultureDictionary.GetChildren("Site")` fetch a
   whole group in one call.
2. **Create the items** in the Translation section — via the Umbraco Developer
   MCP if available, otherwise the manual clicks described in
   [Dictionary items](https://docs.umbraco.com/umbraco-cms/fundamentals/data/dictionary-items.md).
   To move items between sites, use the backoffice's Export/Import of `.udt`
   files.
3. **Render them** — see
   [rendering-dictionary-items.md](references/rendering-dictionary-items.md) for
   the exact lookup overloads, a worked example, culture handling, and verification.

## Version compatibility

Umbraco 17+. Every API used here (`GetDictionaryValue`, `GetDictionaryValueOrDefault`,
`CultureDictionary`) exists since Umbraco 13 and is available in any view that
inherits `UmbracoViewPage`, with or without ModelsBuilder. The older
`ILocalizationService` is obsolete and must not be used.

## Best practices

- **Always have a fallback.** `GetDictionaryValue` returns an empty string for a
  missing key; prefer `GetDictionaryValueOrDefault(key, fallback)` or configured
  language fallbacks wherever blank UI is unacceptable. `GetDictionaryValue` has
  no string-fallback overload — its second parameter is a `CultureInfo`.
- **Let the request culture drive lookups.** Umbraco sets `CurrentUICulture` per
  request from the matched domain; use the explicit `CultureInfo` overload only
  when you deliberately want a fixed culture.
- **`CultureDictionary` is cached.** It is the safe read path for views — never
  call `IDictionaryItemService` (the management API) from a view or other hot
  path.
- **Dictionary = chrome, variants = content.** If the text is part of the page's
  editorial content, it is a language variant, not a dictionary item.

## Validation

Assertions live in [`evals/evals.json`](evals/evals.json); run them with the
`umbraco-skill-evaluator` skill.
