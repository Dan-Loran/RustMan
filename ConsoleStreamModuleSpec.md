# Console Stream Module Spec

## Purpose

Provide a sealed boundary for owning live console stream state.

This module owns:
- storing ConsoleEntry items
- maintaining current console stream state
- exposing a read contract for current stream contents

This module does NOT interpret transport or presentation meaning.

---

## Responsibility

- receive ConsoleEntry objects
- append them to current stream state
- enforce stream retention rules if defined
- expose current console stream snapshot

---

## Inputs (Pins In)

### ConsoleEntryCreated
A ConsoleEntry produced by the Console Interpretation module.

The input must include:
- text/content
- message type identity
- timestamp if available

---

### ClearConsoleStream
Optional control input to clear current console state.

This exists only if the operator-facing interface needs it.

---

## Outputs (Pins Out)

### ConsoleStreamStateChanged
Indicates the console stream state has changed.

This output exposes a current state snapshot suitable for Presentation consumption.

---

### ConsoleStreamCleared
Indicates the stream was cleared.

---

### ConsoleStreamErrorOccurred
Indicates a failure in stream management.

Examples:
- invalid entry state
- retention/buffer error

---

## Internal Responsibilities

- append ConsoleEntry items in order
- maintain current collection/state
- enforce maximum retained entries if defined
- clear state when instructed
- expose current snapshot

---

## Non-Responsibilities (Hard Rules)

The module must NOT:

- parse raw JSON
- route messages
- interpret console identity
- assign CSS classes
- know about Web or Blazor
- know about connection lifecycle except through explicit inputs
- decide how console entries are displayed to the user

---

## Contract Ownership

All input/output contracts must live in:

RustMan.Core/Modules/ConsoleStream

---

## State Ownership Rule

- This module owns live console stream state
- This module is the source of truth for current console entries
- No other module may mutate its internal collection directly

---

## Retention Rule

If retention limits exist, they belong here.

Examples:
- keep most recent N entries
- drop oldest when max is exceeded

Retention rules must be explicit and testable.

---

## Snapshot Rule

This module may expose:
- read-only snapshot/state contract

This module must NOT expose:
- mutable internal collections
- direct references intended for external mutation

---

## Testing Expectations

This module should be testable by:

- providing ConsoleEntry inputs
- verifying ordered storage
- verifying retention behavior
- verifying clear behavior
- verifying state snapshot output

Tests should NOT require:
- WebSocket
- Router
- UI
- Presentation layer
- full system setup

---

## Summary

Console Stream is a sealed runtime state module.

It knows:
- what console entries currently exist

It does NOT know:
- why they were created
- how they are presented to the operator