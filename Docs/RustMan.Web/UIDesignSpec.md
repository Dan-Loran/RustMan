# RustMan UI Design Specification (UIDesignSpec.md)

## Purpose

This document defines the UI design intent for RustMan.

It captures:
- layout philosophy
- workspace structure
- module composition rules
- responsive behavior
- command discoverability patterns

This is **not a pixel-perfect spec**.

It defines:
> structure, priority, and behavior — not styling.

---

## Product Vision

RustMan should feel like:

> **a NOC (Network Operations Center) control panel for Rust servers**

The UI must support:
- real-time awareness
- rapid decision making
- immediate action without documentation lookup

---

## Core UX Principle

> **The worst outcome is forcing the user to consult documentation.**

All critical actions must be:
- discoverable
- contextual
- close to where they are used

---

## Application Structure

RustMan consists of two primary UI layers:

### 1. Dashboard (Fleet Overview)

Purpose:
- show system-wide and instance-level status
- highlight issues
- provide entry into instance workspaces

Characteristics:
- summary-focused
- read-heavy
- low interaction depth

---

### 2. Instance Workspace (Operational Surface)

Purpose:
- live operational control of a single Rust server instance

Characteristics:
- real-time data
- action-oriented
- module-based layout

---

## Workspace Design Model

The workspace is not a page of widgets.

It is a:

> **modular control surface composed of operational stations**

Each module represents:
- a live data stream or state
- associated actions
- contextual commands

---

## Workspace Section Order (LOCKED)

The vertical order of major sections must remain:

1. **Instance Status / Header**
2. **Player Management**
3. **Chat**
4. **Console**

This order is based on:
- operational priority
- frequency of use
- human vs system interaction

---

## Section Roles

### 1. Instance Status

Provides:
- instance name
- running state
- performance indicators (FPS, etc.)
- metadata (map, ports, player count)

Purpose:
- quick situational awareness

---

### 2. Player Management (High Priority)

Provides:
- live player list
- selected player details
- player actions (kick, ban, etc.)

Purpose:
- primary operational control surface

---

### 3. Chat (High Visibility)

Provides:
- live chat stream

Characteristics:
- short visible area
- scrollable
- frequently referenced

Purpose:
- real-time communication awareness

---

### 4. Console (Source of Truth)

Provides:
- full server console stream
- command input

Characteristics:
- deeper visible area than chat
- scrollable
- investigative tool

Purpose:
- system-level truth and diagnostics

---

## Relative Layout Rules (CRITICAL)

Do NOT use fixed pixel-based layout rules.

Instead:

- Chat must be **compact and scrollable**
- Console must be **visibly dominant compared to chat**
- Console visible area should be **at least 2× chat** in standard layouts

Rule:

> **Console height ≥ 2 × Chat height (relative, not fixed units)**

---

## Responsive Design Rules

### 1. Section Order is Immutable

Major sections must always remain in the defined vertical order.

---

### 2. Parent/Child Grouping

Each section may contain supporting widgets.

Rule:

> **Supporting widgets may move beside or below their parent section depending on available width, but must remain grouped with that parent section.**

---

### 3. Layout Adaptation

On wide screens:
- sections may contain side-by-side subcomponents

On narrow screens:
- subcomponents stack vertically

Example (Players):

Desktop:
- Player table (left)
- Selected player panel (right)

Mobile:
- Player table
- Selected player panel (below)

---

## Command Strip Pattern (CORE FEATURE)

Each module must provide contextual command access.

### Structure

Each module includes:

1. Display surface (data)
2. Direct interaction (if applicable)
3. Command strip (always present)

---

### Command Strip Characteristics

- located directly under the module it affects
- contains categorized commands
- grouped by prefix
- common commands appear first

Purpose:

> allow operators to execute commands without consulting documentation

---

### Examples

#### Player Module
- moderation commands
- teleport commands
- admin actions

#### Chat Module
- communication commands
- broadcast tools

#### Console Module
- server commands
- diagnostic commands

---

## Console Interaction Model

Console should behave like a real terminal:

- append-only stream
- scrollable history
- command input inline

Controls:
- Send command
- Clear console (future)
- Follow mode (auto-scroll toggle)

---

## Scroll Behavior

### Chat
- auto-scroll enabled by default
- pauses when user scrolls up

### Console
- auto-scroll enabled by default
- “Follow” control required for reattachment

---

## Presentation vs Web Responsibilities

### Presentation Layer

Responsible for:
- interface state
- neutral view models
- data projection

Must NOT include:
- CSS
- styling concepts
- platform-specific details

---

### Web Layer

Responsible for:
- layout
- rendering
- styling
- responsive behavior

Maps:
- `Type` → visual styling
- state → UI behavior

---

## Replaceability Rule

> Modules must be replaceable as long as inputs and outputs are preserved.

UI must not:
- depend on module internals
- assume implementation details

---

## Future Expansion

Additional modules may be added to the workspace.

Examples:
- server metrics
- map tools
- backup status
- alerts

Rules:
- must fit within the section model
- must not break section order
- must follow command strip pattern

---

## Out of Scope (Current Phase)

- instance creation workflows
- advanced styling/theming
- complex filtering/search
- command editing systems

---

## Summary

RustMan UI is:

- operator-first
- module-driven
- responsive without losing structure
- command-discoverable
- low cognitive load

It is NOT:

- a generic admin dashboard
- a form-heavy application
- documentation-driven

---

## Final Principle

> **Design for how operators think, not how systems are built.**