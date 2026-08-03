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

Targets Umbraco 17+ and covers the page shown while Umbraco is running an upgrade with pending
migrations. It is not an error page — for 404/500/Boot Failed use the
`umbraco-custom-error-pages` skill.

## Choose the right layer

| Layer | Use when | Implementation |
|---|---|---|
| **Umbraco runtime** | Umbraco is running an upgrade or migration | `UmbracoWebsite/maintenance.cshtml` |
| **Umbraco Cloud platform** | Cloud infrastructure or platform operations make the app unavailable | Cloud portal Error Pages feature |
| **Hosting/web server** | The app is offline, restarting, or cannot start | `app_offline.htm`, deployment slots, or a web-server fallback |

Do not use the runtime maintenance page as a general deployment or app-restart page. Offer the
appropriate hosting-layer option when the user is describing ordinary deployment downtime.

For consistent branding, the same self-contained visual design can be adapted for each layer:

- **Umbraco Cloud:** fetch the [Cloud Error Pages documentation](https://docs.umbraco.com/umbraco-cloud/build-and-customize-your-solution/handle-deployments-and-environments/error-pages.md)
  before giving portal instructions. The page is uploaded through the portal, assigned per
  hostname, and must be self-contained and within the documented size limit. Offer
  [`assets/cloud-error-page.html`](assets/cloud-error-page.html) as a starter.
- **Azure / IIS:** use [`app_offline.htm`](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/app-offline)
  or deployment slots. Fetch the [Azure deployment-slots documentation](https://learn.microsoft.com/en-us/azure/app-service/deploy-staging-slots)
  when recommending that approach.

## How to implement

Fetch the official tutorial first and follow it as the source of truth:

> https://docs.umbraco.com/umbraco-cms/run-in-production/tutorials/create-a-custom-maintenance-page.md

The setup is intentionally small: create an `UmbracoWebsite` folder at the project root and add
`maintenance.cshtml` with self-contained markup. The page is shown automatically during upgrade
state; no toggle is required for the normal behavior.

The official documentation warns against keeping a project in upgrade mode longer than necessary.
Test this on a staging environment with a real version-upgrade path rather than leaving production
in upgrade mode. Do not claim to have verified the page unless an actual build or upgrade test was
run.

Offer [`assets/maintenance.cshtml`](assets/maintenance.cshtml) as an optional starting template.
Ask whether auto-refresh is wanted and remove it when it is not. If the site is multi-domain,
add brand-specific styling deliberately rather than copying a hard-coded domain example.
Auto-refresh should poll a suitable public health URL and reload only after the application
responds normally again.

Keep the served file self-contained: inline CSS, no external assets, no layout, and no live
Umbraco/Razor data such as `IPublishedContent`. The content cache or database may be unavailable
while the page is being served.

## Disabling instead

Most upgrades do not require the site to go down. If the team prefers not to show this page, add
`Umbraco:CMS:Global:ShowMaintenancePageWhenInUpgradeState: false` in `appsettings.json` (see the
official tutorial).

## Done

Tell the user:

- The page lives at `UmbracoWebsite/maintenance.cshtml` and appears automatically during upgrades.
- It disappears when migrations finish; visitors are not necessarily reloaded automatically.
- The page can be disabled with `Umbraco:CMS:Global:ShowMaintenancePageWhenInUpgradeState`.
- Cloud platform downtime and ordinary app-restart downtime require their own hosting-layer page.

## Validation

Assertions for this skill live in [`evals/evals.json`](evals/evals.json). Run them with the
`umbraco-skill-evaluator` skill and validate the skill with `umbraco-skill-validator` and
`umbraco-skill-code-analyzer` before shipping.
