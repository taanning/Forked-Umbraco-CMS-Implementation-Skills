# Rendering dictionary items in Razor

Dictionary items are created in the backoffice (Translation > Dictionary) and read in
templates via `UmbracoHelper`, which resolves the value for the request's culture.

## Fetch these first

- [UmbracoHelper: fetching dictionary values](https://docs.umbraco.com/umbraco-cms/develop-with-umbraco/templating-and-rendering/querying/umbracohelper.md#fetching-dictionary-values)
  documents the two basic lookups, `GetDictionaryValue(key)` and
  `GetDictionaryValueOrDefault(key, altText)`. The `CultureInfo` overloads,
  `Umbraco.CultureDictionary` and `GetChildren(key)` are not on that page; they are covered
  by the overload table and worked example below.
- [Multilanguage setup tutorial](https://docs.umbraco.com/umbraco-cms/develop-with-umbraco/tutorials/multilanguage-setup.md)
 - read "Using Dictionary Items", "Translating Multi-Value Property Editors", and
  "Adding Culture and Hostnames" (how the request culture is resolved).

Do not reproduce the docs' code beyond the one-line call
`@Umbraco.GetDictionaryValue("Site.ReadMore")` - fetch the pages and link instead.

## The exact overloads

`UmbracoHelper` has four, and only four, dictionary lookups:

| Call | Returns when the key/translation is missing |
| --- | --- |
| `GetDictionaryValue(string key)` | `""` |
| `GetDictionaryValue(string key, CultureInfo culture)` | `""` |
| `GetDictionaryValueOrDefault(string key, string fallback)` | `fallback` |
| `GetDictionaryValueOrDefault(string key, CultureInfo culture, string fallback)` | `fallback` |

There is **no** `GetDictionaryValue(key, "fallback")` - a string second argument does not
compile, because that position is the `CultureInfo`. A fallback always means the
`OrDefault` name.

## Worked example

[`assets/_DictionaryLabels.cshtml`](../assets/_DictionaryLabels.cshtml) is a reference
view, not a file to add to the user's project: its keys are placeholders and one is
deliberately missing. Read it, then write the same calls inline in the site's own layout
and templates where the hard-coded text currently sits. It shows four forms:

1. `GetDictionaryValueOrDefault(key, fallback)` - request-culture lookup that never
   renders empty.
2. `GetDictionaryValue(key, new CultureInfo("da-DK"))` - the explicit-culture overload,
   for when a fixed language is wanted regardless of the request.
3. The same `OrDefault` call on a deliberately absent key - what fallback output looks
   like.
4. `Umbraco.CultureDictionary.GetChildren("Site")` - every direct child of the `Site`
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
visitor. Outside a request - API controllers, background work - resolve the culture
explicitly via `IVariationContextAccessor` or a `CultureInfo` overload; never assume the
default language.

## Verify

Your final answer must contain one of these two statements about build correctness -
silence on it is the failure mode to avoid:

1. **A `.csproj` is present:** run `dotnet build` after writing the files, fix any
   errors, and report the result. If the build cannot run (no SDK, no package feed,
   restore fails), say so and quote the error rather than implying success. A clean
   build does not prove the views compile - `.cshtml` is compiled at runtime - so also
   say that the views were not exercised and recommend loading a page that renders them.
2. **No buildable project:** say so explicitly, and state that every dictionary call
   uses one of the four overloads in the table above, so you expect it to compile.

If a key renders empty at runtime: the item is missing, the key casing differs (keys
are case-sensitive), or the request culture has no translation and no fallback was
configured.

## Done

Tell the user which items exist and where editors manage them (Translation > Dictionary),
which templates render them, and that values follow the request's culture - and how to
add more keys under the shared group prefix. Include the build-correctness statement
from Verify, and link the docs page(s) you followed so the user can check the steps
against the source.

## Runtime coverage

Asserted on the blank reference host by `examples/razor-rendering/`.
