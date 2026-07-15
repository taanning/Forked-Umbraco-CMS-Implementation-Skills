---
name: umbraco-custom-maintenance-page
description: >
  Customize the maintenance page shown while Umbraco runs upgrade migrations. Follows the
  official docs approach (a static UmbracoWebsite/maintenance.cshtml) with a branded starter
  template. Trigger: user asks to "customize maintenance page", "brand maintenance page",
  "create custom maintenance page", "style upgrade page", "change maintenance page in
  Umbraco". SKIP: non-Umbraco projects, and error pages (404/500/Boot Failed — use
  umbraco-custom-error-pages instead).
---

# Custom Maintenance Page

The page Umbraco automatically shows visitors while the runtime is in **upgrade state**
(pending database migrations after a version-bump deploy; on Umbraco Cloud this includes its
automatic upgrades). It is not an error page — for 404/500/Boot Failed use the
`umbraco-custom-error-pages` skill — and it doesn't cover app-restart windows during normal
deploys. Those are handled at the hosting layer instead:

- **Umbraco Cloud:** the [Error Pages feature](https://docs.umbraco.com/umbraco-cloud/build-and-customize-your-solution/handle-deployments-and-environments/error-pages)
  — upload a self-contained `.html` page (max 20 KB) in the portal, assigned per hostname,
  served via Cloudflare from blob storage while the site is down for platform operations.
  Offer [`assets/cloud-error-page.html`](assets/cloud-error-page.html) as a ready-to-upload
  starter — same design as the maintenance template, self-contained, and with auto-refresh
  (Cloud does not reload visitors automatically when the site comes back; the page must poll).
- **Azure / IIS:** [app_offline.htm](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/app-offline)
  (a static holding page served while the file exists) or
  [deployment slots](https://learn.microsoft.com/en-us/azure/app-service/deploy-staging-slots)
  (zero-downtime slot swaps). See also
  [Running Umbraco on Azure Web Apps](https://docs.umbraco.com/umbraco-cms/run-in-production/infrastructure-and-ops/server-setup/azure-web-apps).

Offer to reuse the same design from [`assets/maintenance.cshtml`](assets/maintenance.cshtml)
for those hosting-level pages, so visitors see consistent branding at every layer.

## How to implement

The implementation steps live in the official tutorial. Fetch it and follow it as the source
of truth — this skill intentionally does not restate the steps, so it stays in sync when the
documentation changes:

> https://docs.umbraco.com/umbraco-cms/run-in-production/tutorials/create-a-custom-maintenance-page.md

In short: create a `UmbracoWebsite` folder at the project root and add a
`maintenance.cshtml` with your markup.

The docs describe the setup but don't include a page design. Offer
[`assets/maintenance.cshtml`](assets/maintenance.cshtml) as a starting template. It includes
two optional extras (ask the user; strip what's unwanted):

- **Auto-refresh** - polls the site and reloads automatically once the upgrade finishes.
- **Multi-domain styling hook** - per-brand tweaks for multi-site installs.

Keep the file self-contained (inline CSS, no live Umbraco/Razor data) - it renders while the
content cache/DB may be mid-migration, so it must not depend on Umbraco being up.

## Disabling instead

Most upgrades don't require the site to go down; the page can be turned off entirely with
`Umbraco:CMS:Global:ShowMaintenancePageWhenInUpgradeState: false` in `appsettings.json` (see
the tutorial). Mention this option — the user may prefer no interruption at all.

## Done

Tell the user:
- The page lives at `UmbracoWebsite/maintenance.cshtml` and shows automatically during
  upgrades — no toggle needed; it disappears when migrations finish.
- It can be disabled via the config setting above.
- Works self-hosted and on Umbraco Cloud (where auto-upgrades make a branded page extra
  worthwhile).
