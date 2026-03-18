# RustMan v2

RustMan is a **self-hosted web-based control console** for managing Facepunch Rust dedicated servers.

It runs as a **single application runtime** on the same host as the Rust servers and provides centralized control over multiple server instances.

---

## Overview

RustMan is designed as a **NOC-style operational console** for Rust server administration.

It prioritizes:

- real-time visibility
- rapid decision making
- direct action without documentation lookup

This is not a generic dashboard.  
It is an **operator-first system** built for active server management.

---

## Core Capabilities

- Manage multiple Rust server instances from a single interface
- Monitor live server state (players, performance, console, chat)
- Execute commands directly against running instances
- Maintain isolated instance environments with a shared runtime
- Provide a structured operational workspace per instance

---

## Application Structure

RustMan consists of three primary UI areas:

### Dashboard (Fleet Overview)

- Entry point into the application
- Displays system-level and instance-level status
- Read-focused, low interaction
- Used to identify issues and open instance workspaces

---

### Instance Workspace (Operational Surface)

- Primary working area for managing a single Rust server
- Designed as a **modular control surface**
- Supports real-time data and direct actions
- Can be opened in multiple browser windows for tiled workflows

Core sections (fixed order):

1. Instance Status  
2. Player Management  
3. Chat  
4. Console  

---

### Settings

- Application configuration
- Instance creation and management
- Administrative controls

---

## Deployment Model

RustMan is designed for **one-line installation** on a fresh Ubuntu server.

A system administrator should be able to:

1. SSH into a clean Ubuntu host
2. Run a single command
3. Have a fully prepared RustMan environment

The installer will:

- install required OS-level dependencies via `apt`
- deploy the RustMan application
- install SteamCMD
- install and manage the shared Rust dedicated server runtime
- create required directory structure
- initialize the SQLite database
- apply schema and seed data

After installation:

- the web interface becomes accessible
- the first login flow creates the initial admin account

Authentication and authorization will be role-based and implemented in a later phase.

---

## Runtime Layout (Linux)

RustMan separates application code from managed data:

### Application

- `/opt/rustman/app`

Contains deployed application binaries.  
Safe to replace during upgrades.

---

### Managed Data

- `/var/lib/rustman/steamcmd`
- `/var/lib/rustman/rustdedicated`
- `/var/lib/rustman/instances/{slug}`
- `/var/lib/rustman/map-previews`
- `/var/lib/rustman/tmp`

Contains all mutable runtime and instance data.

---

### Configuration

- `/etc/rustman`

Reserved for machine-level configuration.

---

### Logs

- `/var/log/rustman`
- `journalctl -u rustman`

---

## Installer Responsibilities

The installer is responsible for **system provisioning**, not business logic.

### Responsibilities

- prepare directory structure
- install OS dependencies via `apt`
- deploy application files
- create SQLite database
- apply schema from SQL files
- apply seed data from SQL files
- install SteamCMD
- prepare shared Rust runtime location

### Non-Responsibilities

The installer must NOT:

- contain business logic about commands or server behavior
- hardcode command metadata
- manage runtime behavior after installation

---

## Data Initialization Model

Database structure and seed data are provided as **separate SQL files**.

Examples:

- `001_initial_schema.sql`
- `001_seed_commands.sql`
- `002_seed_server_properties.sql`

This ensures:

- clarity
- replaceability
- low cognitive load
- safe iteration

The installer only executes these files.  
It does not interpret their contents.

---

## Architecture Principles

RustMan is built on strict architectural boundaries.

---

### 1. Module-Forward Architecture

The system is composed of **sealed modules**.

Modules communicate only through:

- Inputs (requests / commands)
- Outputs (events / messages)

Internal implementation does not matter if contracts are preserved.

> Modules must be replaceable without breaking the system.

---

### 2. Separation of Responsibilities

RustMan enforces clear boundaries:

#### Infrastructure
- runtime behavior
- filesystem, processes, RCON

#### Presentation
- UI-neutral state
- view models
- interface logic

#### Web
- rendering only
- layout, styling, interaction

---

### 3. Low Cognitive Load

The system prioritizes:

- explicit code over clever abstractions
- predictable behavior
- simple, readable flows

If it is hard to understand later, it is wrong.

---

### 4. Slice-Based Development

Features are built in **vertical slices**:

- defined inputs and outputs
- end-to-end behavior
- minimal scope

Each slice must be:

- complete
- testable
- understandable in isolation

---

### 5. Replaceability Over Optimization

Design favors:

- stable contracts
- boring systems
- predictable behavior

Over:

- premature optimization
- framework-heavy abstractions
- hidden logic

---

## UI Design Philosophy

RustMan follows a **NOC-style control panel model**.

Key principles:

- operators should not need documentation
- actions must be contextual and discoverable
- console is the source of truth
- chat and player management are high-visibility operational areas

---

## Development Workflow

RustMan uses a structured collaboration model:

- ChatGPT → architecture, planning, guardrails  
- Codex → implementation  

Rules:

- design is locked before implementation
- modules communicate only through contracts
- no cross-module internal access
- no premature abstraction

---

## Current Status

RustMan v2 is in active development.

Completed:

- WebRCON → Router → Console → Presentation → Web pipeline
- Instance workspace structure
- Module-forward architecture foundation

In Progress:

- Installer (environment provisioning and setup)

---

## Guiding Principle

> Build systems that are easy to understand later, not just easy to write now.