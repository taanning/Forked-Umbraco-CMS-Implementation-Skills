---
name: umbraco-custom-maintenance-page
description: >
  Customize the maintenance page shown during Umbraco upgrades. Trigger: user asks to "customize maintenance page",
  "brand maintenance page", "create custom maintenance page", "style upgrade page", "change maintenance page in Umbraco".
  SKIP: non-Umbraco projects.
---

# Custom Maintenance Page

Create a custom maintenance page that displays during Umbraco upgrades by adding a `maintenance.cshtml` file to the `UmbracoWebsite` folder at the project root.

**Note:** This page only displays during Umbraco upgrades. It is not a general maintenance mode for scheduled downtime.

---

## Step 1 - Discovery

Search the project for:
- Umbraco version (`.csproj` or `Directory.Packages.props`)
- Whether `UmbracoWebsite` folder exists at project root
- Existing `maintenance.cshtml` file
- Current `appsettings.json` configuration for maintenance settings
- Whether this is a multi-site setup

---

## Step 2 - Create UmbracoWebsite Folder

If the `UmbracoWebsite` folder doesn't exist at the project root, create it.

---

## Step 3 - Create maintenance.cshtml

Create a `maintenance.cshtml` file in the `UmbracoWebsite` folder.

Ask the user for:
- Company logo URL
- Brand colors
- Custom message text
- Contact information
- Background image or design preferences

Read `assets/maintenance.cshtml` as a template. Customize the HTML with the user's branding preferences and write to `UmbracoWebsite/maintenance.cshtml`.

---

## Step 4 - Optional - Disable Maintenance Page

To disable the maintenance page, update `appsettings.json`:

```json
{
  "Umbraco": {
    "CMS": {
      "Global": {
        "ShowMaintenancePageWhenInUpgradeState": false
      }
    }
  }
}
```

---

## Step 5 - Testing

**Method 1 - Actual upgrade:**
1. Perform an Umbraco upgrade
2. Navigate to the site - verify the custom maintenance page displays
3. Verify styling and branding appear correctly
4. Navigate to `/umbraco` - verify backoffice is accessible

**Method 2 - Simulate upgrade (for testing only):**
1. Temporarily modify the Umbraco state to simulate upgrade mode
2. Navigate to the site - verify the custom maintenance page displays
3. Revert the simulation after testing

**If disabled:**
1. Set `ShowMaintenancePageWhenInUpgradeState: false` in appsettings.json
2. Perform an upgrade - verify normal site access during upgrade

---

## Troubleshooting

**Maintenance page not showing:**
- Verify `UmbracoWebsite` folder is at the project root (not inside another folder)
- Verify file is named exactly `maintenance.cshtml` (case-sensitive on some systems)
- Check that the project is actually in an upgrade state

**Styling not appearing:**
- Verify CSS is properly included in the HTML
- Check for CSS conflicts if using external stylesheets
- Test in different browsers to rule out browser-specific issues

**File not being picked up:**
- Ensure the file is in the correct location: `ProjectRoot/UmbracoWebsite/maintenance.cshtml`
- Restart the application after creating the file
- Check file permissions ensure the application can read the file

---

## Done

Tell the user:
- Custom maintenance page created in `UmbracoWebsite/maintenance.cshtml`
- Page displays during Umbraco upgrades
- Backoffice remains accessible during upgrades
- Works for both self-hosted and Umbraco Cloud deployments
