# RustMan Installer Slice 1 — As Built

## Purpose

This document captures the **actual implemented behavior** of Installer Slice 1 after successful end-to-end validation on a clean Ubuntu VM.

It reflects what was built, what was verified, and any deviations or notes discovered during implementation.

---

## Summary

Installer Slice 1 provisions a fresh Ubuntu system with:

* required OS dependencies
* RustMan application
* SQLite database with schema and seed data
* SteamCMD
* shared Rust dedicated server runtime
* systemd service for RustMan

The installer executes as a **single linear pipeline** with fail-fast behavior and rollback support.

---

## Installation Flow (As Implemented)

The installer performs the following steps in order:

1. Validate execution as root
2. Validate installer package structure
3. Assert system is not already installed (fail-fast)
4. Install OS prerequisites via `apt`
5. Create `rustman` system user and group
6. Create directory structure:

   * `/opt/rustman`
   * `/opt/rustman/app`
   * `/var/lib/rustman`
   * `/var/lib/rustman/steamcmd`
   * `/var/lib/rustman/rustdedicated`
   * `/var/lib/rustman/instances`
   * `/var/lib/rustman/map-previews`
   * `/var/lib/rustman/tmp`
   * `/etc/rustman`
7. Deploy published RustMan application to `/opt/rustman/app`
8. Create environment file:

   * `/etc/rustman/rustman.env`
9. Create SQLite database:

   * `/var/lib/rustman/rustman.db`
10. Apply SQL files from `/db` in sorted order
11. Download and install SteamCMD
12. Download and install shared Rust dedicated runtime (App ID 258550)
13. Install systemd unit file (`rustman.service`)
14. Enable service
15. Start service
16. Run verification checks

---

## Filesystem Layout (As Built)

### Application

* `/opt/rustman/app`

### Data

* `/var/lib/rustman`
* `/var/lib/rustman/steamcmd`
* `/var/lib/rustman/rustdedicated`
* `/var/lib/rustman/instances`
* `/var/lib/rustman/map-previews`
* `/var/lib/rustman/tmp`
* `/var/lib/rustman/rustman.db`

### Configuration

* `/etc/rustman/rustman.env`

### Service

* `/etc/systemd/system/rustman.service`

---

## Database State

### Schema Tables Present

* `instances`
* `command_definitions`
* `command_parameters`
* `server_property_definitions`
* `server_property_allowed_values`
* `instance_property_values`

### Seed Data

* `command_definitions`: 591 rows
* `command_parameters`: seeded
* `server_property_definitions`: seeded
* `server_property_allowed_values`: 1280 rows

### Explicit Behavior

* `instances` table exists
* `instances` table contains **0 rows after install**

This confirms no instance data is seeded by the installer.

---

## SteamCMD and Rust Runtime

### SteamCMD

* Installed to: `/var/lib/rustman/steamcmd`
* Source: Valve download URL
* Executable: `steamcmd.sh`

### Rust Dedicated Server Runtime

* Installed to: `/var/lib/rustman/rustdedicated`
* Installed via SteamCMD:

  * `app_update 258550 validate`
* This is a **shared runtime**, not an instance

---

## Service Behavior

* Service name: `rustman.service`
* Installed to: `/etc/systemd/system/rustman.service`
* Enabled on install
* Started automatically

### Verified State

* `systemctl is-enabled rustman.service` → `enabled`
* `systemctl is-active rustman.service` → `active`

---

## Runtime Observation

On first startup, the application logs:

* `Initial WebRcon bootstrap connection failed`

This occurs because:

* no instance exists
* no RCON endpoint is configured

### Important

* This does **not** prevent the service from starting
* This is **expected behavior** for this slice
* This is **not an installer failure**

---

## Rollback Behavior

If any step fails:

* installer exits immediately
* rollback is triggered

Rollback removes:

* systemd service file
* environment file
* application directory
* data directory contents
* SteamCMD installation
* Rust runtime installation
* SQLite database
* created directories
* `rustman` user and group (if created in this run)

### Not Removed

* OS packages installed via `apt`

---

## Packaging (As Built)

### Package Structure

The installer package contains:

* `/app` → published RustMan application
* `/installer` → install scripts and service file
* `/db` → schema and seed SQL

### Build Process

Built via:

* `installer/package-installer.ps1`

### Important Fix

* Initial packaging produced a **corrupt `.tar.gz`**
* Root cause was in the Windows packaging process
* Packaging script was corrected
* Rebuilt archive installed successfully

---

## Distribution Model (Current)

Installer is delivered via:

* public GitHub release asset

Download URL:

* `https://github.com/Dan-Loran/RustMan/releases/latest/download/rustman-installer.tar.gz`

### Important Note

* This public distribution model is **temporary for development**
* Future versions will move to a private or controlled distribution model

---

## Validation Environment

Tested on:

* Clean Ubuntu VM
* Host: `192.168.100.40`
* Snapshot-based validation

### Validation Results

All installer success conditions were met:

* user/group created
* directories created
* application deployed
* database created and seeded
* SteamCMD installed
* Rust runtime installed
* service enabled and active

---

## Known Limitations (Accepted)

* No progress reporting during install
* Minimal user feedback during long operations (SteamCMD / Rust download)
* No authentication or first-run user setup
* No instance creation
* WebRCON connection failure message on startup (expected)

---

## Deferred to Future Slices

* Installer progress and UX improvements
* Private distribution model
* Auth/bootstrap user flow
* Instance creation workflow
* Update and upgrade path
* Health checks beyond basic verification

---

## Final Status

**Installer Slice 1 is complete and validated.**

The system can be provisioned from a clean Ubuntu environment using a single installer package and reaches a stable, running RustMan service ready for further configuration.
