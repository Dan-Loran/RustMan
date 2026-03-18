# RustMan Installer Slice 1

## Purpose

Installer Slice 1 provisions a fresh Ubuntu machine for RustMan from a single installer package.

## What Installer Slice 1 Does

The installer:

- installs OS prerequisites with `apt`
- creates the `rustman` user and group
- creates the RustMan directory layout
- deploys the published Linux app to `/opt/rustman/app`
- creates `/var/lib/rustman/rustman.db`
- applies SQL files from `/db` in sorted filename order
- downloads SteamCMD from Valve into `/var/lib/rustman/steamcmd`
- downloads the shared Rust dedicated runtime into `/var/lib/rustman/rustdedicated`
- installs `rustman.service`
- reloads systemd
- enables the service
- starts the service

## Success Definition

Installation succeeds only when all of the following are true:

- `rustman` user and group exist
- required directories exist
- `/opt/rustman/app` contains the published app
- `/etc/rustman/rustman.env` exists
- `/var/lib/rustman/rustman.db` exists
- required SQLite tables exist
- `/var/lib/rustman/steamcmd/steamcmd.sh` exists
- `/var/lib/rustman/rustdedicated/RustDedicated` exists
- `/etc/systemd/system/rustman.service` exists
- `rustman.service` is enabled
- `rustman.service` is active

## Rollback Definition

If any installer step fails, `install.sh` stops immediately and performs best-effort rollback.

Rollback removes artifacts created during the current run:

- `rustman.service`
- `/etc/rustman/rustman.env`
- `/opt/rustman/app`
- `/var/lib/rustman/*`
- directories created by the current run
- the `rustman` user and group if this run created them

`apt` packages remain installed.

## Paths Created

- `/opt/rustman`
- `/opt/rustman/app`
- `/var/lib/rustman`
- `/var/lib/rustman/steamcmd`
- `/var/lib/rustman/rustdedicated`
- `/var/lib/rustman/instances`
- `/var/lib/rustman/map-previews`
- `/var/lib/rustman/tmp`
- `/etc/rustman`

## Package Build Process

Build the installer package from Windows with:

```powershell
powershell -ExecutionPolicy Bypass -File .\installer\package-installer.ps1
```

The packaging script:

1. publishes `RustMan.Web` for `linux-x64`
2. stages `/app`, `/installer`, and `/db`
3. creates `artifacts/installer/rustman-installer.tar.gz`

## Bootstrap Usage

The bootstrap entry point is `installer/bootstrap.sh`.

Default installer asset URL:

`https://github.com/Dan-Loran/RustMan/releases/latest/download/rustman-installer.tar.gz`

Expected usage:

```bash
curl -sSL <bootstrap-url> | bash
```

## Dev VM Validation Steps

1. Restore a clean Ubuntu snapshot.
2. Build the installer package.
3. Publish the archive or copy the extracted package to the VM.
4. Run the bootstrap or `sudo bash installer/install.sh`.
5. Verify `id rustman` and `getent group rustman`.
6. Verify `ls -ld /opt/rustman /opt/rustman/app /var/lib/rustman /etc/rustman`.
7. Verify `sqlite3 /var/lib/rustman/rustman.db ".tables"`.
8. Verify `test -f /var/lib/rustman/steamcmd/steamcmd.sh`.
9. Verify `test -x /var/lib/rustman/rustdedicated/RustDedicated`.
10. Verify `systemctl is-enabled rustman.service`.
11. Verify `systemctl is-active rustman.service`.
12. Review `journalctl -u rustman --no-pager`.

## Rollback Validation Steps

1. Restore a clean Ubuntu snapshot.
2. Edit `installer/install.sh`.
3. Add `false` immediately before `enable_and_start_service`.
4. Run the installer.
5. Confirm `/opt/rustman`, `/var/lib/rustman`, `/etc/rustman`, and `/etc/systemd/system/rustman.service` are absent.
6. Confirm `getent passwd rustman` and `getent group rustman` return nothing.
