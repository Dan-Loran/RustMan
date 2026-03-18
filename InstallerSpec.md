# InstallerSpec.md

## Purpose

This document defines the **design intent** for the RustMan Installer (Slice 1).

It captures:

* what the installer is responsible for
* what success means
* what failure means
* system boundaries
* inputs and outputs

This is a **design document**, not an implementation.

---

## High-Level Goal

RustMan must support a **one-line installation** on a fresh Ubuntu server.

The expected user flow:

1. SSH into a clean Ubuntu machine
2. Run a single command
3. Receive a fully functional RustMan environment

---

## Installer Entry Point

Installer is triggered via:

curl -sSL <url> | bash

This bootstrap script:

1. downloads a single `.tar.gz` package from a public GitHub release
2. extracts it to a temporary directory
3. executes the installer script inside the package

---

## Package Source

* Hosted as a **public GitHub Release asset**
* Single distributable file:

  * rustman-installer.tar.gz

No authentication or token handling is required for Slice 1.

---

## Package Layout

The installer package must follow this structure:

/app/
/installer/
/db/

### /app

Contains the published RustMan application.

Deployed to:

/opt/rustman/app

---

### /installer

Contains installer logic.

Primary entry point:

/installer/install.sh

---

### /db

Contains SQL files for database setup.

Examples:

* 001_initial_schema.sql
* 001_seed_commands.sql
* 002_seed_server_properties.sql

Installer executes these files in order.

Installer does NOT interpret their contents.

---

## System Responsibilities

The installer is responsible for **machine provisioning only**.

It must NOT contain application business logic.

---

## Installer Responsibilities

The installer must:

### System Setup

* install OS-level dependencies using apt
* create rustman group
* create rustman user

### Directory Structure

Create:

/opt/rustman
/opt/rustman/app

/var/lib/rustman
/var/lib/rustman/steamcmd
/var/lib/rustman/rustdedicated
/var/lib/rustman/instances
/var/lib/rustman/map-previews
/var/lib/rustman/tmp

/etc/rustman

Set ownership:

rustman:rustman

---

### Application Deployment

* extract application files into /opt/rustman/app
* ensure correct permissions

---

### Database Initialization

* create SQLite database
* execute schema SQL file(s)
* execute seed SQL file(s)

Installer must only:

* locate files
* execute them
* fail if they fail

---

### SteamCMD Installation

* install SteamCMD into:

/var/lib/rustman/steamcmd

---

### Rust Runtime Installation

* download Rust dedicated server runtime via SteamCMD
* install into:

/var/lib/rustman/rustdedicated

This runtime is shared by all instances.

---

### Systemd Service

Installer must:

* create RustMan systemd unit file
* configure service to run as:

User=rustman
Group=rustman

* reload systemd daemon
* enable service
* start service

---

## Success Definition

Installer is successful only if ALL of the following are true:

* rustman user and group exist
* required directories exist
* ownership is correct
* application files are deployed
* SQLite database exists
* schema applied successfully
* seed data applied successfully
* SteamCMD installed
* Rust runtime downloaded
* systemd service exists
* systemd daemon reloaded
* service enabled
* service running

---

## Failure Behavior

If any step fails:

* installer must stop immediately
* installer must perform rollback
* installer must exit with clear error message

---

## Rollback Requirements

Installer must remove all RustMan-owned artifacts created during the current run.

This includes:

* rustman user and group (if created)
* directories created by installer
* application files
* database file
* SteamCMD files under RustMan paths
* Rust runtime files under RustMan paths
* systemd unit file
* service enable/start state

Rollback must:

* be explicit
* be best-effort
* follow reverse order of creation where possible

---

## Rollback Exclusions

Installer is NOT responsible for removing:

* apt-installed OS packages
* system-level dependencies outside RustMan scope

---

## Re-run Behavior

If installer is executed on a system where RustMan is already installed:

* installer must fail immediately
* installer must not overwrite existing installation
* installer must not attempt repair or upgrade

---

## Ownership Model

All RustMan-managed resources must be owned by:

rustman:rustman

This includes:

* application files
* runtime files
* instance data
* SteamCMD
* Rust runtime
* database files

---

## Data Initialization Model

All database structure and seed data must be defined in SQL files.

Installer must NOT:

* define schema in code
* define seed data in code
* interpret meaning of data

Installer only executes SQL files in order.

---

## Boundaries

### Apt (OS Layer)

Responsible for:

* installing system dependencies

Not responsible for:

* RustMan updates
* SteamCMD lifecycle
* Rust runtime lifecycle

---

### Installer

Responsible for:

* initial system provisioning
* initial runtime setup

---

### RustMan Application (Post-Install)

Responsible for:

* managing instances
* updating runtime
* operational workflows

---

## Design Principles

* explicit over implicit
* simple over clever
* predictable over optimized
* replaceable over tightly coupled

---

## Non-Goals (Slice 1)

Do NOT implement:

* upgrade system
* uninstall system
* repair system
* authentication setup
* first-run admin flow
* advanced configuration

---

## Guiding Principle

Build the installer so that:

* a future developer can understand it quickly
* behavior is obvious from reading the script
* failures are easy to diagnose
* rollback is predictable

The installer should be:

boring, explicit, and reliable


## Existing Database Reference

A reference database currently exists at:

D:\Source\RustMan\rustman.db

This database may be used as a source for:

- schema reference
- baseline seeded metadata

The `instances` table must NOT be used as seed input.

Reason:

- it contains prior test data
- it is not part of baseline installer initialization