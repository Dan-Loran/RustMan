## RustMan.old UI Recovery Pack

Captured on 2026-03-18 from:

- Source repo: `C:\Users\Daniel\source\repos\RustMan.old`
- Running app URL: `http://127.0.0.1:5289`

### What is included

- `screenshots/`
  - Headless browser screenshots of the main routed pages.
- `html/`
  - HTML snapshots fetched from the running app for the same routes.
- `source/`
  - The old UI source files most useful for rebuilding the same structure in the new solution.

### Captured routes

- `/` -> `screenshots/home.png`
- `/instances` -> `screenshots/instances.png`
- `/instances/create` -> `screenshots/instance-create.png`
- `/instances/test-1` -> `screenshots/instance-workspace-test-1.png`
- `/instances/test-1/edit` -> `screenshots/instance-edit-test-1.png`
- `/settings` -> `screenshots/settings.png`
- `/settings/about` -> `screenshots/settings-about.png`
- `/settings/backups` -> `screenshots/settings-backups.png`

### Source highlights

- App shell and routing
  - `source/Components/App.razor`
  - `source/Components/Routes.razor`
- Layout and nav
  - `source/Components/Layout/MainLayout.razor`
  - `source/Components/Layout/MainLayout.razor.css`
  - `source/Components/Layout/NavMenu.razor`
  - `source/Components/Layout/NavMenu.razor.css`
- Global styling
  - `source/wwwroot/app.css`
- Major pages
  - `source/Components/Pages/Home.razor`
  - `source/Components/Pages/Instances.razor`
  - `source/Components/Pages/InstanceWorkspace.razor`
  - `source/Components/Pages/CreateInstance.razor`
  - `source/Components/Pages/Settings.razor`
  - `source/Components/Pages/About.razor`
  - `source/Components/Pages/Backups.razor`

### Notes

- The instance-specific screenshots use the sample slug `test-1` found in the old repo database.
- This pack is intended as a UI reference and recovery source, not as a backend migration plan.
