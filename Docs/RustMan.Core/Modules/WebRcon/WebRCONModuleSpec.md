# WebRCON Module Spec

## Purpose

Provide a sealed boundary for Rust WebRCON communication.

This module owns:
- connection lifecycle
- command transmission
- inbound message reception
- protocol conversion (raw JSON → typed message)

This module does NOT interpret message purpose.

---

## Responsibility

- manage WebSocket connection to Rust server
- send commands
- receive raw messages
- convert raw JSON into defined inbound message objects
- emit connection state changes
- emit errors

---

## Inputs (Pins In)

### Connect
Establish connection to server.

- Host
- Port
- Password

---

### Disconnect
Terminate active connection.

---

### SendCommand
Send a command to the server.

- Command object (contract defined in Core)

---

## Outputs (Pins Out)

### ConnectionStateChanged

Indicates connection lifecycle changes.

Examples:
- Connecting
- Connected
- Disconnecting
- Disconnected
- Error

---

### MessageReceived

A converted inbound RCON message.

- Must NOT be raw JSON
- Must conform to shared contract
- Must include message ID if present

---

### ErrorOccurred

Indicates a failure in:
- connection
- send
- receive
- protocol conversion

---

## Internal Responsibilities

- WebSocket management
- receive loop
- send operations
- JSON parsing and conversion
- protocol normalization

---

## Non-Responsibilities (Hard Rules)

The module must NOT:

- interpret message meaning (console, chat, etc.)
- route messages
- store application-level state
- expose internal buffers
- expose raw JSON
- know about UI or Presentation
- know about downstream modules

---

## Contract Ownership

All input/output contracts must live in:

RustMan.Core/Modules/WebRcon

---

## Message Identity Rule

If a message includes an ID:
- it must be preserved
- it must be exposed in MessageReceived

This enables downstream correlation.

---

## State Ownership Rule

- This module owns connection state only
- It does NOT own message history
- It does NOT own buffers beyond immediate processing

---

## Delivery Rule

- Messages are emitted to a single downstream consumer (Router)
- No multi-subscriber behavior at this boundary
- No read-tracking flags exposed

---

## Testing Expectations

This module should be testable by:

- providing input commands
- simulating inbound raw JSON
- verifying emitted outputs

Tests should NOT require:
- UI
- Presentation layer
- full system setup

---

## Summary

WebRCON is a sealed transport + protocol module.

It knows:
- how to talk to Rust

It does NOT know:
- why the system cares