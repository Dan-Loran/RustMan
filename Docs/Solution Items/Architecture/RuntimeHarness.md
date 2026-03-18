# Runtime Harness (Authoritative)

## Purpose

This document defines how runtime modules in RustMan connect and communicate.

This is the authoritative source of truth for:

- module wiring
- signal flow
- allowed communication paths
- architectural boundaries

If code contradicts this document, the code is wrong.

---

## Core Rule (Non-Negotiable)

Router is the only module that communicates with WebRcon.

This applies in both directions:

Outbound (to server):
Any Module → Router → WebRcon → Server

Inbound (from server):
Server → WebRcon → Router → First-Level Handlers

There are no exceptions.

---

## Command Path (Strict)

Any module that needs to send a command to the Rust server MUST:

1. send a command request to Router
2. Router assigns identifier and manages correlation
3. Router forwards the command to WebRcon

No module may:

- call WebRcon directly
- reference IWebRconModule outside Router
- bypass Router for convenience

---

## Inbound Message Path (Strict)

All inbound messages MUST follow:

WebRcon → Router → First-Level Handlers

Router is responsible for:

- correlation (command responses)
- forwarding non-correlated messages
- distributing messages to appropriate handlers

Router must not drop inbound messages.

---

## Router Responsibilities

Router is the runtime signal bus.

It is responsible for:

- receiving all inbound messages from WebRcon
- receiving all outbound command requests
- assigning command identifiers
- correlating command responses
- forwarding unmatched messages to handlers
- emitting system-level signals (errors, connection state)

Router does NOT:

- interpret message meaning
- parse console/chat semantics
- build JSON
- manage UI state
- persist data

Router is intentionally simple.

---

## WebRcon Responsibilities

WebRcon is the protocol boundary.

It is responsible for:

- WebSocket transport
- connection lifecycle
- JSON serialization/deserialization
- translating between wire format and typed messages

WebRcon does NOT:

- route messages
- interpret meaning
- track command correlation
- accept commands from arbitrary modules

WebRcon receives commands only from Router.

---

## First-Level Handlers

Examples:

- Console Interpretation
- Chat Interpretation (future)
- Player Interpretation (future)

Responsibilities:

- receive routed inbound messages from Router
- determine relevance for their domain
- emit domain-specific signals

These are the entry points into feature pipelines.

---

## Downstream Feature Pipelines

After Router, modules form linear pipelines.

Example (Console):

Router → Console Interpretation → Console Stream → Presentation → Web

Rules:

- modules communicate only through contracts
- no shared mutable state across modules
- no access to another module’s internals
- pipelines must be explicit and linear
- no sideways coupling between unrelated features

Router is NOT required between downstream modules.

---

## Wiring Rules (Strict)

- WebRcon talks ONLY to Router
- Router talks to WebRcon and first-level handlers
- All server-bound commands go through Router
- All server-originated messages go through Router
- No module may reference IWebRconModule except Router
- No module may bypass Router
- No module may access another module’s internals
- No cross-feature direct communication

If a shortcut seems convenient, it is wrong.

---

## Signal Types

### Outbound

- RouterCommandRequested
- RouterCommandDispatchRequested

### Inbound

- RouterInboundMessageReceived
- RoutedCommandResponse
- RoutedInboundMessage (non-correlated)

### System

- RouterConnectionStateChanged
- RouterErrorOccurred

---

## Command Model

Commands are:

- string CommandText
- IReadOnlyList<string> Parameters

Router does NOT format commands.

WebRcon is responsible for:

- building the final command string
- wrapping JSON payloads

---

## Command Correlation

- sequential integer identifiers
- stored in-memory
- 5 second TTL
- cleared on disconnect/fault

No retries. No replay.

---

## Error Handling

Router:

- catches internal exceptions
- emits RouterErrorOccurred
- continues processing

The runtime pipeline must never break.

---

## Extension Model

To add a new feature:

1. create a first-level handler
2. attach it to Router
3. build downstream pipeline modules
4. connect modules via contracts

Do NOT:

- modify WebRcon for feature behavior
- bypass Router
- duplicate routing logic

---

## Architectural Intent

Router is the control panel.  
WebRcon is the external wiring.  
Handlers are attached devices.  
Pipelines carry signals forward.

Everything flows through the panel.

---

## Final Principle

If a module needs to reach the Rust server:

It goes through Router.

If a message comes from the Rust server:

It comes through Router.

No exceptions.