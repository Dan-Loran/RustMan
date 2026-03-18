# Router Module (As-Built)

## Purpose

Router is the central signal bus for RustMan runtime communication.

It sits between WebRcon and all downstream modules and is responsible for:

- routing inbound messages
- correlating command responses
- forwarding outbound commands to WebRcon

Router is intentionally simple and does not interpret message meaning.

---

## Position in Runtime Harness

```
Command Sender
      ↓
    Router
      ↓
   WebRcon
      ↓
    Server
      ↓
   WebRcon
      ↓
    Router
      ↓
   Handlers (future)
```

Router is the only module that connects all runtime signals.

---

## Responsibilities

### 1. Outbound Command Handling

- accepts `RouterCommandRequested`
- assigns a sequential `CommandIdentifier`
- stores correlation state in-memory
- emits `RouterCommandDispatchRequested`

Router does NOT format commands or build JSON.

---

### 2. Inbound Message Routing

- accepts `RouterInboundMessageReceived`
- checks if the message identifier matches a pending command
- if matched:
  - emits `RoutedCommandResponse`
  - removes correlation entry
- if not matched:
  - message falls through (no action in this slice)

Router does NOT emit “unhandled” messages.

---

### 3. Connection State Handling

- accepts `RouterConnectionStateChanged`
- clears all pending command correlations when:
  - Disconnected
  - Faulted

---

### 4. Error Handling

- catches internal exceptions
- emits `RouterErrorOccurred`
- continues processing

Router must never break the signal pipeline.

---

## Correlation Model

- identifiers are `int`
- assigned sequentially starting from 1
- stored in `_pendingCommands`
- stored as:
  - `CommandIdentifier`
  - timestamp (UTC)

### Expiration

- TTL: 5 seconds
- expired entries are removed during normal processing
- no timers or background workers

### Behavior

- no retries
- no replay
- no persistence

---

## Inputs (Core Contracts)

- `RouterCommandRequested`
- `RouterInboundMessageReceived`
- `RouterConnectionStateChanged`

---

## Outputs (Core Contracts)

- `RouterCommandDispatchRequested`
- `RoutedCommandResponse`
- `RouterErrorOccurred`

---

## What Router Does NOT Do

Router is intentionally limited.

It does NOT:

- perform JSON serialization/deserialization
- build command strings
- interpret console or chat meaning
- manage UI state
- persist data
- perform logging
- manage transport or connection logic
- buffer or store messages beyond correlation

---

## Internal Structure

### State

```
Dictionary<int, DateTime> _pendingCommands
int _nextCommandIdentifier
```

### Outputs

```
Action<RouterCommandDispatchRequested>
Action<RoutedCommandResponse>
Action<RouterErrorOccurred>
```

### Behavior

- explicit methods
- no background threads
- no async workflows beyond interface contracts

---

## Wiring Relationship

Router does not know about WebRcon directly.

It is connected via `RuntimeModuleWiring`, which:

- maps dispatch requests → WebRconCommandRequest
- forwards inbound messages → Router
- forwards connection state → Router

This keeps Router transport-agnostic.

---

## Command Model

Commands are represented as:

```
string CommandText
IReadOnlyList<string> Parameters
```

Router does not combine these into a final string.

Command formatting is handled downstream (currently in wiring).

---

## Known Simplifications

- parameters are `List<string>`
- no typed command system
- no batching
- no retry logic
- no persistence
- no prioritization
- no routing beyond command correlation

---

## Design Principles

- explicit over clever
- minimal state
- no hidden behavior
- no cross-module leakage
- easy to understand in isolation

Router should feel like a simple electrical switchboard.

---

## Current Scope Boundary

Router currently supports:

- command correlation
- command dispatch
- inbound response routing
- connection reset behavior

Router does NOT yet support:

- multi-handler routing (Console, Chat, Player)
- message classification
- broadcast routing

These will be added in future slices.

---

## Summary

Router is:

- the central runtime signal bus
- a minimal correlation engine
- a routing decision point

Router is NOT:

- a processor
- a parser
- a state manager
- a workflow engine

Keep it simple.
