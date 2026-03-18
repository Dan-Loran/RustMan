# RustMan v2 - Installer Slice 1 Codex Prompt

You are implementing the **Installer Slice 1** for **RustMan v2**.

Follow this prompt exactly.

---

## Project Context

RustMan v2 is a **self-hosted web-based control console** for managing Facepunch Rust dedicated servers.

It runs as a **single application runtime** on the same Linux host as the Rust server runtime and manages multiple Rust server instances.

The product is designed as a **NOC-style operational console** with these primary UI areas:

- Dashboard
- Instance Workspace
- Settings

The Dashboard is the entry point and provides fleet/system visibility.

The Instance Workspace is the primary operational surface for managing a single Rust server instance.

Settings will handle application settings and instance creation workflows.

---

## Core Project Principles

This project is built around:

- low cognitive load
- explicit readable code
- stable contracts
- replaceable internals
- minimal framework magic
- boring predictable behavior
- long-term maintainability over speed

The most important design philosophy is:

> Modules are only touched through communication contracts.  
> Inputs and outputs are what matter.  
> Internal implementation can change freely as long as the same input produces the same output behavior.

This philosophy matters because it reduces context loss and makes handoff between chat sessions and tools easier.

---

## Collaboration Model

This project uses a strict collaboration model:

- ChatGPT and user handle planning, design, boundaries, and guardrails
- Codex handles implementation

Your job is to implement within the locked design.

Do NOT redesign the slice.  
Do NOT invent a broader framework.  
Do NOT generalize beyond the current need unless absolutely required.

Keep implementation:

- simple
- explicit
- easy to debug
- easy to reason about later

Only introduce complexity if it is directly required by the locked installer behavior.

---

## Installer Slice Goal

This slice delivers a **one-line installer** for a fresh Ubuntu machine.

Expected entry point:

`curl -sSL <url> | bash`

That bootstrap script downloads a **single `.tar.gz` package** from a **public GitHub Release asset**, extracts it to a temporary directory, and runs the installer inside the package.

This slice is focused on the installer package contents and installer workflow.

---

## Locked Installer Intent

The following decisions are already locked and must not be changed.

### Package Source

- public GitHub Release asset
- single `.tar.gz` package

### Bootstrap Behavior

- curl-only
- downloads installer package
- extracts package to temp directory
- runs installer script from extracted contents

### Package Layout

Use this layout inside the package:

    /app/
    /installer/
    /db/

Where:

- `/app/` contains published RustMan app output
- `/installer/` contains installer script(s)
- `/db/` contains SQL schema and seed files

Expected examples:

    /db/001_initial_schema.sql
    /db/001_seed_commands.sql
    /db/002_seed_server_properties.sql

### OS Dependency Model

Use `apt` only for OS-level prerequisites.

Do NOT use apt for lifecycle management of:

- RustMan application updates
- SteamCMD updates
- Rust dedicated server updates

Those belong to RustMan-managed flows after installation.

### Installer Responsibilities

The installer must:

- install OS-level prerequisites using apt
- create `rustman` group
- create `rustman` user
- create required directories
- assign ownership to `rustman:rustman`
- deploy app files to `/opt/rustman/app`
- create SQLite database
- apply schema SQL files
- apply seed SQL files
- install SteamCMD into RustMan-managed path
- download shared Rust dedicated server runtime
- create RustMan systemd service
- reload systemd
- enable the service
- start the service

### Service Ownership

RustMan systemd service must run as:

- `User=rustman`
- `Group=rustman`

### Re-run Behavior

For Slice 1:

- fail fast if RustMan already appears installed
- no upgrade path
- no reinstall path
- no repair path

### Failure Behavior

If installer fails:

- remove RustMan-owned artifacts created during the current run
- leave no practical RustMan trace behind
- apt-installed OS packages may remain
- rollback must be explicit and best-effort
- rollback should proceed in reverse order where possible

### Success Definition

Installer succeeds only if all of the following are true:

- `rustman` user and group created
- required directories created
- ownership assigned correctly
- app files installed to `/opt/rustman/app`
- SQLite database created
- schema applied
- seed data applied
- SteamCMD installed
- shared Rust dedicated server runtime downloaded
- systemd service file created
- systemd daemon reloaded
- service enabled
- service started

---

## Linux Layout

Use this Linux layout.

### Application

    /opt/rustman/app

### Managed Data

    /var/lib/rustman
    /var/lib/rustman/steamcmd
    /var/lib/rustman/rustdedicated
    /var/lib/rustman/instances
    /var/lib/rustman/map-previews
    /var/lib/rustman/tmp

### Configuration

    /etc/rustman

### Logging

Primary runtime logs should be available via systemd journal.

Do not overbuild file-based logging in this slice.

---

## Data Initialization Rules

The installer must **not encode business metadata in code**.

It must only know:

- where SQL files are
- how to execute them
- how to stop if execution fails

It must **not** understand what command definitions or server properties mean.

That belongs in SQL files only.

---

## Existing Database Guardrail

A reference database currently exists at:

`D:\Source\RustMan\rustman.db`

You may use this database as a source for:

- schema reference
- baseline pre-seeded metadata

You must **NOT** use data from the `instances` table for installer seed/setup work.

Reason:

- `instances` contains prior test data
- it is not part of baseline installer initialization
- installer seed/setup must not import historical test instance records

This is a hard guardrail.

---

## Implementation Guidance

Use the **lowest cognitive load** approach.

Prefer:

- shell scripts for installer workflow
- explicit functions
- clear step names
- readable console output
- direct file paths
- explicit rollback tracking

Avoid:

- clever shell tricks
- dense one-liners
- hidden control flow
- generalized installer frameworks
- unnecessary indirection

### Strong Recommendation

Track created artifacts explicitly, such as:

- whether group was created
- whether user was created
- which directories were created
- which files were created
- whether DB was created
- whether SteamCMD was installed
- whether Rust runtime was downloaded
- whether service file was created
- whether service was enabled
- whether service was started

Rollback should use tracked state rather than assumptions.

If a separate rollback script increases complexity, keep rollback logic inside `install.sh`.

---

## Required Deliverables

Implement the minimum file set needed for this slice.

At minimum, provide or update:

### Installer files

- bootstrap script or placeholder bootstrap script if needed
- main installer script under `/installer/`
- helper shell scripts only if truly necessary
- systemd unit file template or generated service-file logic

### Database hookup

- SQL schema execution
- SQL seed execution
- deterministic ordering of SQL application

### Documentation

Update or create documentation describing:

- what the installer does
- what success means
- what rollback means
- what paths are created
- how to test on a dev VM

---

## Suggested First-Cut File Structure

This is guidance only. Keep it simple.

    installer/
      install.sh

    app/
      [published RustMan app files]

    db/
      001_initial_schema.sql
      001_seed_commands.sql
      002_seed_server_properties.sql

If you need one additional helper script, keep it minimal and justified.

Do not build a multi-layer installer framework.

---

## Testing Expectations

Do not overbuild automated testing if it slows delivery.

The implementation must be easy to validate manually on a fresh Ubuntu VM.

Manual validation should clearly cover:

- `rustman` user/group creation
- directory creation
- ownership
- application deployment
- database presence
- schema/seed success
- SteamCMD presence
- Rust runtime presence
- systemd service file presence
- service enabled state
- service running state

Also validate rollback by forcing a controlled failure at a known step.

---

## Constraints

Do NOT implement:

- upgrade handling
- repair handling
- reinstall handling
- uninstall handling
- authentication setup
- first-run admin creation flow
- advanced configuration system
- business seed data defined in code

Do NOT:

- import `instances` table data from the reference database
- move command/property metadata into installer logic
- over-abstract installer phases

Do:

- make the slice complete
- keep behavior explicit
- make logs readable
- make rollback understandable
- keep future evolution possible without overbuilding now

---

## Output Format Required

Provide your response in this order:

1. brief implementation plan
2. exact files to create or update
3. full file contents for each file
4. manual test steps
5. rollback test steps
6. brief as-built summary
7. git commit message

Keep explanations short.  
Prefer complete file replacements where practical.  
Do not flood the response with unnecessary commentary.

---

## Final Principle

Build the installer like the rest of RustMan:

> easy to understand later, easy to replace internally, and boring on purpose