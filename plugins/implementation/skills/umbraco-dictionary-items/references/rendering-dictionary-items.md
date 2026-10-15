# Rendering dictionary items in Razor

Dictionary items are created in the backoffice (Translation > Dictionary) and read in
templates via `UmbracoHelper`, which resolves the value for the request's culture.

## Fetch these first

- [UmbracoHelper — Fetching dictionary values](https://docs.umbraco.com/umbraco-cms/develop-with-umbraco/templating-and-rendering/querying/umbracohelper.md#fetching-dictionary-values)
  — the two lookups the page documents: `GetDictionaryValue(string key)` and
  `GetDictionaryValueOrDefault(string key, string altText)`. The `CultureInfo` overloads,
  `Umbraco.CultureDictionary` and `ICultureDictionary.GetChildren(key)` are additional
  members of `UmbracoHelper` / `ICultureDictionary` (`Umbraco.Cms.Core.Dictionary`) that the
  shipped partial demonstrates.
- [Multilanguage setup](https://docs.umbraco.com/umbraco-cms/develop-with-umbraco/tutorials/multilanguage-setup.md)
  — the "Using Dictionary Items" and "Translating Multi-Value Property Editors" sections,
  plus "Adding Culture and Hostnames" for how request culture is resolved.

Do not reproduce the docs' code beyond the one-line call
`@Umbraco.GetDictionaryValue("Site.ReadMore")` — fetch the pages and link instead.

## The exact overloads

`UmbracoHelper` has four, and only four, dictionary lookups:

| Call | Returns when the key/translation is missing |
| --- | --- |
| `GetDictionaryValue(string key)` | `""` |
| `GetDictionaryValue(string key, CultureInfo culture)` | `""` |
| `GetDictionaryValueOrDefault(string key, string fallback)` | `fallback` |
| `GetDictionaryValueOrDefault(string key, CultureInfo culture, string fallback)` | `fallback` |

There is **no** `GetDictionaryValue(key, "fallback")` — a string second argument does not
compile, because that position is the `CultureInfo`. A fallback always means the
`OrDefault` name.

## Worked example

[`assets/_DictionaryLabels.cshtml`](../assets/_DictionaryLabels.cshtml) is a reference
view, not a file to add to the user's project: its keys are placeholders and one is
deliberately missing. Read it, then write the same calls inline in the site's own layout
and templates where the hard-coded text currently sits. It shows four forms:

1. `GetDictionaryValueOrDefault(key, fallback)` — request-culture lookup that never
   renders empty.
2. `GetDictionaryValue(key, new CultureInfo("da-DK"))` — the explicit-culture overload,
   for when a fixed language is wanted regardless of the request.
3. The same `OrDefault` call on a deliberately absent key — what fallback output looks
   like.
4. `Umbraco.CultureDictionary.GetChildren("Site")` — every direct child of the `Site`
   group item for the request culture, rendered as a list.

## Multi-value property editors

Dropdown/checkbox option *values* can be dictionary keys rather than display text: store
the key, render it with `GetDictionaryValue(Model.Value<string>("myProperty"))`. Editors
then translate the option labels without touching the Document Type. Details in the
"Translating Multi-Value Property Editors" section of the multilanguage tutorial linked
above.

## Culture

In a Razor request, `CurrentUICulture` is set from the domain's Culture & Hostnames
mapping (else the default language), so the parameterless overloads already follow the
visitor. Outside a request — API controllers, background work — resolve the culture
explicitly via `IVariationContextAccessor` or a `CultureInfo` overload; never assume the
default language.

## Verify

If a real `.csproj` is present, run `dotnet build` after writing the files and fix any
compile errors before finishing — noting that `.cshtml` files are compiled at runtime, so
a clean build does NOT prove the view compiles; load a page that renders the partial and
confirm the translated text appears. If a key renders empty: the item is missing, the key
casing differs (keys are case-sensitive), or the request culture has no translation and
no fallback was configured. If no buildable project exists (e.g. a sandbox with no
solution to build against), say so explicitly and state that the code targets the
documented Umbraco 17 APIs it uses — don't stay silent on build correctness either way.

## Done

Tell the user which items exist and where editors manage them (Translation > Dictionary),
which templates render them, and that values follow the request's culture — and how to
add more keys under the shared group prefix. Link the docs page(s) you followed so the
user can check the steps against the source.

## Runtime coverage

Asserted on the blank reference host by `examples/razor-rendering/`.
