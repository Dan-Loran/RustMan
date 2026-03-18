# ConsoleStream Module (As-Built)

## Purpose

ConsoleStream is the runtime module responsible for maintaining the live console feed for RustMan.

It:

- receives all console-routed messages from Router
- stores them in arrival order
- enforces a bounded in-memory buffer
- exposes a snapshot of the current console state to consumers

ConsoleStream represents the **runtime truth of the console feed**, independent of any UI.

---

## Position in Runtime Harness

Inbound flow:

Server → WebRcon → Router → ConsoleStream → Presentation → Web

ConsoleStream sits directly downstream of Router and is fed by the Router console shunt.

---

## Responsibilities

### 1. Message Ingestion

- receives `RoutedConsoleMessage` from Router
- converts it into `ConsoleEntry`
- preserves:
  - message text
  - message type
  - timestamp (currently assigned at Router)

---

### 2. Buffer Management

- maintains an in-memory list of console entries
- preserves strict arrival order

#### Retention Policy

- maximum entries: **500**
- when exceeded:
  - oldest entries are purged
  - newest entries are retained

No background trimming or timers are used.

---

### 3. State Emission

- emits `ConsoleStreamStateChanged` after each message
- provides a `ConsoleStreamSnapshot` containing:
  - `IReadOnlyList<ConsoleEntry>`

Snapshots are built from a copy to prevent mutation of internal state.

---

## Inputs

- `RoutedConsoleMessage`
  - Message (string)
  - Type (string)
  - TimestampUtc (DateTime)

---

## Outputs

- `ConsoleStreamStateChanged`
  - contains `ConsoleStreamSnapshot`

---

## Internal Structure

### State

- private list of `ConsoleEntry`

### Behavior

- append-only (with bounded trimming)
- no background workers
- no async concurrency beyond contract requirements

---

## What ConsoleStream Does NOT Do

ConsoleStream does NOT:

- filter messages
- interpret message meaning
- parse chat/player events
- perform routing
- manage UI state
- persist data
- access WebRcon or Router directly

---

## Design Principles

- runtime-owned state
- explicit behavior
- minimal responsibility
- no hidden processing
- no cross-module leakage

---

## Summary

ConsoleStream is:

- the runtime console buffer
- a simple append-and-trim module
- a source of truth for the live console feed

ConsoleStream is NOT:

- a parser
- a router
- a UI service

It stores and exposes the console feed exactly as received.