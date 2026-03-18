# RustMan Manual Deployment (Ubuntu)

## 1. Linux Folder Layout

RustMan uses a split layout between deployed application files and managed server data.  
This keeps the application binaries separate from mutable server data, which simplifies upgrades and runtime management.

---

## Application Files

    /opt/rustman/app

This directory contains the published RustMan web application files copied from the Windows build output.

These files are **replaceable during upgrades** and should not contain persistent data.

---

## Managed Data and Runtime Files

    /var/lib/rustman/steamcmd
    /var/lib/rustman/rustdedicated
    /var/lib/rustman/instances/{slug}/
    /var/lib/rustman/map-previews/
    /var/lib/rustman/tmp/

These directories contain mutable RustMan-managed content.

### steamcmd

    /var/lib/rustman/steamcmd

Stores the SteamCMD installation used to download and update the Rust dedicated server runtime.

---

### rustdedicated

    /var/lib/rustman/rustdedicated

Stores the **shared Rust server runtime** installed and updated by SteamCMD.

This runtime is shared by all RustMan instances.

---

### instances

    /var/lib/rustman/instances/{slug}/

Each Rust server instance created by RustMan has its own directory containing:

- server configuration
- save files
- identity files
- logs
- generated map data

Example:

    /var/lib/rustman/instances/myserver/
    /var/lib/rustman/instances/pve-community/

---

### map-previews

    /var/lib/rustman/map-previews/

Stores generated preview images created during batch map generation.

These previews are displayed in the RustMan UI during instance creation.

---

### tmp

    /var/lib/rustman/tmp/

Temporary working directory used during operations such as:

- map generation
- runtime updates
- file processing tasks

This directory can be safely cleared if necessary.

---

## Logs

    /var/log/rustman

Optional file-based logs may be stored here if RustMan is configured to write logs to disk.

Primary runtime logs are typically available via:

    journalctl -u rustman

---

## Configuration

    /etc/rustman

Reserved for machine-level configuration files.

Examples may include:

- environment files
- service configuration overrides
- installer configuration

---

## Design Notes

RustMan intentionally stores the shared SteamCMD installation and Rust runtime under:

    /var/lib/rustman

rather than inside the application directory.

This design ensures:

- SteamCMD can update runtime files without modifying application binaries
- RustMan upgrades do not overwrite runtime files
- the runtime follows a standard Linux pattern for application-managed data
- instance data remains separate from deployed application code

The RustMan application itself remains in:

    /opt/rustman/app

which allows deployments to replace the application files without affecting server data or runtimes.