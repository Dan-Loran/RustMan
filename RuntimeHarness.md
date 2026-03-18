# Runtime Harness

## Purpose

This document defines how runtime modules in RustMan connect and communicate.

This is the **authoritative source of truth** for:

- module wiring
- signal flow
- allowed communication paths
- architectural boundaries

If anything in code contradicts this document, the code is wrong.

---

## Core Rule (Non-Negotiable)

> All runtime signals flow through Router.

This applies in both directions:

### Outbound (to server)

Command Sender → Router → WebRcon → Server

### Inbound (from server)

Server → WebRcon → Router → Handlers

---

## High-Level Diagram

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
   Handlers
```

Router is the central signal bus.

There are no bypass paths.

---

## Module Roles

### Router

Responsibilities:
- central signal bus
- minimal command/response correlation
- routing decisions only
- distribution of signals to downstream handlers

Router does NOT:
- build JSON
- format command strings
- interpret message meaning
- manage UI state
- persist data
- own transport behavior

Router is intentionally simple and boring.

---

### WebRcon

Responsibilities:
- WebSocket transport
- connection lifecycle
- JSON serialization/deserialization
- translating between wire format and typed messages

WebRcon does NOT:
- route messages
- interpret message meaning
- track command correlation
- manage application state

WebRcon is the protocol boundary.

---

### Handlers (Console, Chat, Player, etc.)

Responsibilities:
- interpret meaning of messages
- update state
- drive application behavior

Handlers do NOT:
- talk directly to WebRcon
- bypass Router
- perform transport logic

---

## Wiring Rules

These rules are strict.

- WebRcon talks only to Router
- Router talks to WebRcon and downstream handlers
- Handlers do not talk to WebRcon
- No module may bypass Router
- No sideways communication between modules
- All signals must pass through Router

If a shortcut seems convenient, it is wrong.

---

## Signal Flow

### Outbound Signals

1. `RouterCommandRequested`
   - enters Router from a command sender
   - contains:
     - `CommandText`
     - `Parameters`

2. `RouterCommandDispatchRequested`
   - emitted by Router
   - contains:
     - `CommandIdentifier`
     - `CommandText`
     - `Parameters`
   - consumed by WebRcon

---

### Inbound Signals

1. `RouterInboundMessageReceived`
   - emitted by WebRcon
   - consumed by Router

2. `RoutedCommandResponse`
   - emitted by Router when identifier matches
   - consumed by downstream handlers

---

### System Signals

- `RouterConnectionStateChanged`
- `RouterErrorOccurred`

---

## Command Model

Commands are treated as:

- `string CommandText`
- `IReadOnlyList<string> Parameters`

Examples:

- `status`
- `server.save`
- `global.say`

Commands are NOT split into name/subcommand.

---

### Important Note

```csharp
// NOTE:
// Parameters are currently represented as List<string> for simplicity.
// This may be revisited in the future if commands require richer typing
// (e.g., player identifiers, coordinates, typed values).
```

---

## Command Formatting

Router does NOT build the final command string.

WebRcon is responsible for:

- combining `CommandText` and `Parameters`
- building the final command string
- wrapping it in JSON:

```json
{
  "Identifier": <id>,
  "Message": "<final command string>"
}
```

---

## Router Behavior Summary

For each inbound message:

1. cleanup expired command identifiers
2. check for identifier match
3. if match:
   - emit `RoutedCommandResponse`
   - remove identifier
   - stop processing
4. otherwise:
   - do nothing (fall-through)

Router does not emit "unhandled" messages.

---

## Command Correlation

- Router assigns a sequential integer identifier
- identifiers are stored in-memory
- identifiers expire after 5 seconds
- identifiers are cleared on disconnect/fault

No retries. No replay.

---

## Error Handling

Router:

- catches internal exceptions
- emits `RouterErrorOccurred`
- continues processing

Router must never bring down the pipeline.

---

## Extension Model

To add a new module (Chat, Player, etc.):

1. define new routed signal(s) in Core
2. extend Router to emit those signals
3. connect handler via wiring

Do NOT:
- modify WebRcon
- bypass Router
- duplicate routing logic

---

## Known Simplifications

The following are intentional:

- parameters are simple strings
- no command typing system
- no retry logic
- no batching
- no persistence in Router
- no message buffering beyond correlation

These may evolve later, but only with explicit design.

---

## Final Principle

> Router is the electrical bus.  
> WebRcon is the wire protocol.  
> Handlers are the devices.

Keep them separate.

Keep them simple.

Keep them explicit.