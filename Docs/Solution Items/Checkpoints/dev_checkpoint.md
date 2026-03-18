# DEV_CHECKPOINT.md

## Project: RustMan v2

## Area: InstanceWorkspace + Runtime Console Path

## Date: 2026-03-18

---

## 🎯 Current Focus

This checkpoint captures the current state of **InstanceWorkspace development** and the first end-to-end vertical slice:

WebRCON → Router → ConsoleStream → Presentation → Web UI

This slice was built as a **proof of concept for the module-based architecture** and to validate:

* low cognitive load design
* strict module boundaries
* replaceable components
* presentation/UI separation

---

## ✅ What Is Working

### 1. Module Architecture

All runtime behavior flows through explicit modules:

* WebRconModule (Infrastructure)
* RouterModule (Infrastructure)
* ConsoleStreamModule (Infrastructure)
* Presentation layer (Projection only)
* Web layer (Rendering only)

Each module:

* communicates through interfaces
* is replaceable if contracts are honored

---

### 2. Console Vertical Slice

The following pipeline is implemented:

WebRCON → Router → ConsoleStream → Presentation → Web

Behavior:

* WebRCON receives messages
* Router forwards messages
* ConsoleStream buffers and categorizes
* Presentation projects into neutral view models
* Web renders terminal-style console output

---

### 3. Startup Behavior (Revised)

**Previous behavior:**

* app startup blocked on WebRCON connection
* failure caused app crash

**Current behavior:**

* app always starts
* WebRCON connect happens after startup
* connection state is surfaced to UI
* failures do NOT terminate the app

---

### 4. Connection Status Flow

Connection state is now observable.

**States:**

* Disconnected
* Connecting
* Connected
* Faulted

**Flow:**

WebRconModule → Presentation → Web UI

**UI:**

* bottom status bar
* displays current connection state

---

### 5. Presentation Layer Rules (Enforced)

* no CSS
* no UI-specific logic
* neutral view models only
* UI state expressed generically (platform-agnostic)

---

### 6. Web Layer

* responsible for rendering only
* owns styling and layout
* subscribes to Presentation state
* console display implemented
* status bar implemented

---

## ⚠️ Known Limitations / Deferred Work

### 1. WebRCON Integration (Deferred)

**Current testing limitations:**

* remote dev environment (not same-host)
* RustAdmin conflicts with concurrent connections
* WebRCON listener may enter unstable states under repeated testing
* behavior differs from intended deployment model

**Conclusion:**

WebRCON integration is **functionally proven but not fully validated**.

**Final validation will occur when:**

* RustMan runs on same host as Rust server
* instances are created and managed by RustMan
* no external tools (RustAdmin) interfere

---

### 2. Transport Behavior

* minimal working handshake achieved
* no retry logic
* no reconnect logic
* no connection throttling

These are intentionally deferred.

---

### 3. Runtime Composition

* Program.cs currently wires modules and composite consumers
* acceptable for now
* may evolve into a cleaner runtime composition model later

---

## 🧱 Stubbed Major Areas (Next Phases)

These areas are intentionally NOT implemented yet.

---

### 🔧 App Installer (Stub)

**Future responsibility:**

* install RustMan runtime
* configure directories:

  * /opt/rustman
  * /var/lib/rustman
* initialize database
* seed commands and metadata
* configure system service (Linux)

**Status:**
Not started

---

### 🏗️ Instance Creation (Stub)

**Future responsibility:**

* create Rust server instances
* generate start_server.sh
* assign ports and seeds
* persist instance to database
* provision filesystem structure

**Status:**
Not started

---

### 📊 Dashboard (Stub)

**Future responsibility:**

* system overview
* instance cards
* quick status indicators
* entry point to instance workspace

**Status:**
Not started

---

### ⚙️ Settings / Admin (Stub)

**Future responsibility:**

* global configuration
* command definitions
* system tuning
* buffer sizes (e.g., console lines)
* backup configuration

**Status:**
Not started

---

## 🧠 Architectural Principles (Reaffirmed)

* modules are replaceable via contracts
* Router may depend on WebRCON (acceptable tradeoff)
* Presentation is UI-agnostic
* Web is a replaceable client
* RCON is the source of truth for runtime behavior
* avoid hidden framework magic
* prefer explicit, readable code

---

## 🧪 Testing Strategy

**Current:**

* unit tests for module behavior
* presentation state tests
* wiring tests

**Deferred:**

* full integration testing
* multi-instance testing
* long-running stability
* Linux service lifecycle

---

## 📌 Next Logical Step

Shift focus away from RCON debugging and toward:

1. Instance Creation workflow
2. Dashboard structure
3. Managed runtime lifecycle

Return to WebRCON validation after:

* instances are created through RustMan
* app runs in same-host configuration

---

## 🧾 Notes to Future Self

* do not overfit solutions to the current dev environment
* the product is designed for same-host operation
* current remote testing is a distorted environment
* resist adding complexity to fix temporary conditions

---

END OF CHECKPOINT


# DEV CHECKPOINT — Installer Slice 1 Complete

## Current State

RustMan v2 has successfully completed **Installer Slice 1**.

The application can now be provisioned from a clean Ubuntu environment using a single installer package and reaches a stable running state.

This is the first fully validated end-to-end slice of the system.

---

## What Is Working

### Installer (Slice 1)

* One-line bootstrap using curl → download → execute
* Full system provisioning:

  * OS prerequisites via apt
  * rustman system user and group
  * filesystem layout
  * application deployment
  * environment configuration
  * SQLite database creation
  * schema + seed data applied
  * SteamCMD installation
  * Rust dedicated server runtime installation
  * systemd service creation, enable, and start

### Validation Results

Validated on clean Ubuntu VM:

* rustman user and group created
* required directories created
* app deployed to `/opt/rustman/app`
* `/etc/rustman/rustman.env` exists
* `/var/lib/rustman/rustman.db` exists
* required tables present
* `instances` table exists and contains **0 rows**
* command seed count: 591
* server property seed count: 1280
* SteamCMD installed
* Rust runtime installed
* systemd service:

  * enabled
  * active

---

## Known / Accepted Behavior

* Application logs:

  * `Initial WebRcon bootstrap connection failed`
* This occurs because no instances exist yet
* This does NOT impact service startup
* This is expected for this slice

---

## Packaging Notes

* Installer packaged as:

  * `rustman-installer.tar.gz`
* Built via:

  * `installer/package-installer.ps1`

### Important Fix

* Initial packaging produced a corrupt archive
* Packaging script was corrected
* Rebuilt package installs successfully

---

## Distribution Model (Temporary)

* Installer is delivered via public GitHub release asset:

  * `/releases/latest/download/rustman-installer.tar.gz`
* Repository visibility was required to be public for download to succeed

### Future Change

* Source code will be moved to a private repository
* Public repository will remain as a distribution endpoint only

---

## Architecture Status

### Core Principles Holding

* Low cognitive load
* Explicit behavior
* Minimal magic
* Linear, readable installer pipeline
* Clear ownership boundaries
* No hidden state
* Deterministic setup

### Installer Philosophy Proven

* Steps are sequential and understandable
* Each step has clear intent
* Rollback works as a reverse pipeline
* System reaches known-good baseline

---

## What Is NOT Implemented Yet

* Instance creation
* Player management
* Full runtime module wiring in production environment
* Authentication / authorization
* Installer progress feedback
* Update / upgrade path

---

## Next Slice

### Installer Slice 2 — UX & Feedback

Focus:

* progress output improvements
* step visibility
* long-running operation feedback (SteamCMD / Rust download)
* clearer success/failure messaging

### Parallel Work (Optional)

* Move source to private repo
* Establish build → package → public release workflow

---

## Notes for Future Chats / Codex

* Installer Slice 1 is complete — do not redesign it
* Treat current installer as **baseline behavior**
* Improvements should be additive, not structural
* Do not introduce upgrade/install complexity yet
* Keep changes small and explicit

---

## Final Status

**RustMan v2 has a working, validated installer and system baseline.**

This is the foundation for all future development.
