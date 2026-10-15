# Approach A: Handle a built-in notification

Register a class that runs when Umbraco publishes one of its own notifications. Choose this
whenever the trigger is something Umbraco does (save, publish, move, delete, startup...). If the
trigger is *your* code and you want others to subscribe to it, use
[Approach B](approach-b-publish-custom.md).

## Docs: start with the relevant page

Use the shipped handler/composer when they fit. For Program.cs, put the same registration in the
existing builder chain before `Build()`. Choose the notification before fetching documentation;
start with the matching page below, not every link. Verify further only for a missing API detail
or a version mismatch.

| Chosen notification | Fetch |
|---|---|
| A cancel rule on `ContentPublishingNotification` (the shipped asset) | nothing |
| Any other Content notification, culture helpers, `Suppress()` | [ContentService Notifications](https://docs.umbraco.com/umbraco-cms/extend-your-project/server-side-extensions/notifications/contentservice-notifications.md) |
| `ContentCacheRefresherNotification` | [CacheRefresher Notifications](https://docs.umbraco.com/umbraco-cms/extend-your-project/server-side-extensions/notifications/cacherefresher-notifications.md), plus the payload facts below |
| Startup / shutdown | [Application Lifetime Notifications](https://docs.umbraco.com/umbraco-cms/extend-your-project/server-side-extensions/notifications/umbracoapplicationlifetime-notifications.md) |
| Media, Member, or anything not listed | find it in [Using Notifications](https://docs.umbraco.com/umbraco-cms/extend-your-project/server-side-extensions/notifications.md) (overview + full list) |
| Async handler details or the `State` dictionary, if the asset comments aren't enough | [Notification Handler](https://docs.umbraco.com/umbraco-cms/extend-your-project/server-side-extensions/notifications/notification-handler.md) |

These additional details were checked against Umbraco 17.5.3. Use them before searching source;
if the installed API differs, verify that specific detail rather than suppressing the check:

- **New entity?** `content.HasIdentity == false` in "-ing"; `((IRememberBeingDirty)content).WasPropertyDirty("Id")` in "-ed".
- **Cancelled publish:** `IContentService.Publish` returns `PublishResultType.FailedPublishCancelledByEvent`; no `ContentPublishedNotification` and no cache refresh with published cultures follows.
- **`ContentCacheRefresherNotification` payload:** `MessageObject` is `ContentCacheRefresher.JsonPayload[]`; each item has `Id`, `Key` (`Guid?`), `ChangeTypes` (`TreeChangeTypes` flags), `Blueprint`, `PublishedCultures`, `UnpublishedCultures`. A successful publish arrives as `RefreshNode` (or `RefreshBranch` for a branch publish, one payload for the branch root) with non-empty `PublishedCultures` (`["*"]` for invariant content); saves and cancelled publishes carry none. It runs on every server in a load-balanced setup; gate outbound calls with `IServerRoleAccessor.CurrentServerRole` (`ServerRole.Single` or `SchedulingPublisher`) if they must happen once.
- **Batch and language checks:** skip unrelated entities with `continue`, not `return`. Check
  `IsPublishingCulture` before validating a translation. A variant Document Type can still have
  invariant properties: inspect the property's `PropertyType.VariesByCulture()` and read invariant
  values without a culture. For success logs, use `HasPublishedCulture`, not `AvailableCultures`.
- **Cache work has two scopes:** invalidate local entries on every server; apply the server-role
  guard only to outbound side effects. Role selection prevents duplicate senders, not durable
  exactly-once delivery. Test `TreeChangeTypes` with bitwise flags, skip unusable payloads with
  `continue`, and treat `RefreshAll` as local invalidation rather than a publish of everything.
  Branch payloads describe the root; expand descendants when required. Deduplicate overlapping
  keys within one delivery, not forever across legitimate republishes.

## The files

- `assets/PublishingRuleHandler.cs`: an `INotificationHandler<ContentPublishingNotification>`
  that cancels the publish when a property on a given Document Type breaks a rule. It is
  variant-aware: for a culture-variant type it checks only the cultures actually being published.
  The rule itself is one method (`Violates`) marked for replacement.
  It assumes the property varies like its Document Type; adapt the lookup as described above
  when a variant type has an invariant property.
- `assets/NotificationHandlersComposer.cs`: registers the handler. Add further
  `AddNotificationHandler` / `AddNotificationAsyncHandler` lines here for other notifications.

For any other notification, copy the asset's shape (class implementing `INotificationHandler<T>`,
one `Handle` method, one `AddNotificationHandler<T, THandler>()` line) and change the type. When
the work awaits anything (HTTP, email, DB), use `INotificationAsyncHandler<T>` with
`Task HandleAsync(T notification, CancellationToken cancellationToken)` and register with
`AddNotificationAsyncHandler`; don't block on `.Result` inside a sync handler.

## Step 1: Discover project context

- Root namespace and Umbraco version (`<PackageReference Include="Umbraco.Cms"` must be 17+).
- **How handlers are already registered.** Search for `: IComposer` and for
  `AddNotificationHandler` in `Program.cs`. Follow whichever exists; if neither, create a
  composer (works for sites and packages alike).
- Folder conventions (`Composers/`, `NotificationHandlers/`, `Handlers/`...); follow them, else the
  project root.
- For a cancel rule: the Document Type alias and property alias involved, and whether that type
  varies by culture (check the Document Type in the backoffice, or `VariesByCulture()` on it).

## Step 2: Decide which notification

Ask, in this order:

1. **Veto/alter, or react?** Veto: the "-ing" notification and `CancelOperation`. React:
   the "-ed" one.
2. **Does the reaction read the published cache (`IPublishedContent`, `UmbracoHelper`)?** Then
   handle `ContentCacheRefresherNotification`. The "-ed" notification fires before the cache
   has updated, so reading `IPublishedContent` there returns the previous state.
3. **Multilingual site?** Use `IsPublishingCulture` / `IsSavingCulture` / `HasPublishedCulture`
   etc. from the ContentService page. Remember a single-culture unpublish arrives as a
   *Publishing* notification (`IsUnpublishingCulture`), not an Unpublishing one.
4. **Startup work?** `UmbracoApplicationStartedNotification`, guarded by `IsRestarting` (and
   `IRuntimeState.Level == RuntimeLevel.Run` if it must not run during install/upgrade). Keep it
   light; see `umbraco-common-pitfalls`.
5. **Bulk import/migration that itself saves content?** Wrap it in a scope and call
   `scope.Notifications.Suppress()` only if you understand it also skips cache and index
   updates (the ContentService page explains the trade-off).

## Step 3: Write the files

Substitute in the assets:

- `<Namespace>`: the project's root namespace.
- `<DocTypeAlias>` / `<PropertyAlias>`: the confirmed aliases.
- Replace the body of `Violates` with the real rule; keep the `EventMessage` user-facing, since it
  is shown to the editor in the backoffice.

Then, if a real `.csproj` exists, run `dotnet build` and fix compile errors before finishing. If
no buildable project exists, say so explicitly and state which documented APIs the code targets.
Never imply a build you didn't run.

## Done

Tell the user:

- Which notification(s) are handled and why that half of the pair was chosen.
- Where the handler and its registration live, and how to add more handlers.
- For a cancel rule: that the editor sees the `EventMessage` text as a backoffice notification
  and the publish is rolled back; the save itself still succeeds.
- Any caveat that applies: cache timing, culture behaviour, startup cost.

## Runtime coverage

**Asserted** on the Clean reference host by `examples/approach-a-handle-builtin/`: the shipped
handler is compiled and a fixture proves a publish that breaks the rule is cancelled with the
handler's message, and one that obeys it succeeds.
