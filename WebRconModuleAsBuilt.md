# WebRcon Module (As-Built)

## Purpose

WebRcon is the protocol and transport boundary for Rust server communication.

It is responsible for:

- WebSocket communication
- JSON serialization/deserialization
- command formatting
- translating between wire format and typed messages
- connection lifecycle management

It is NOT responsible for routing or message interpretation.

---

## Architectural Position

Router → WebRcon → Server

Inbound:

Server → WebRcon → Router

WebRcon communicates ONLY with Router.

---

## Responsibilities

### 1. Transport

- manages ClientWebSocket
- sends and receives UTF-8 text messages
- assembles message frames
- detects connection loss

---

### 2. Protocol Translation

- converts structured command requests into final command strings
- serializes JSON payloads
- deserializes inbound JSON into typed messages

---

### 3. Command Formatting

WebRcon builds the final outbound command string from:

- CommandText
- Parameters

Example:

CommandText: global.say  
Parameters: ["hello world"]

Result:

"global.say hello world"

This is then wrapped into JSON:

{
  "Identifier": <id>,
  "Message": "<command string>"
}

---

### 4. Connection Lifecycle

- ConnectAsync
- reconnect logic with retry policy
- receive loop management
- connection state transitions

---

### 5. Error Reporting

- emits transport and protocol errors
- does not terminate connection on translation errors
- reports terminal errors only after retry exhaustion

---

## Module Boundary

### Inputs

- SendCommandAsync(WebRconCommandRequest)
- ConnectAsync(WebRconConnectionRequest)
- SetConsumer(IWebRconConsumer)

### Outputs

- ConnectionStateChanged
- MessageReceived (typed payload)
- ErrorOccurred

---

## Integration with Router

WebRcon:

- receives commands only from Router
- sends inbound messages only to Router
- reports connection state only to Router
- reports errors only to Router

No other module may interact with WebRcon.

---

## Data Boundary Rule

All JSON handling is isolated inside WebRcon.

- outbound: structured → JSON
- inbound: JSON → structured

No raw JSON leaves the module.

---

## Internal Structure

WebRconModule  
├── Connection (WebSocket client)  
└── Protocol (translator)

### Connection

- raw socket communication
- no knowledge of JSON or message meaning

### Protocol

- serialization/deserialization
- no transport logic
- no routing logic

---

## Concurrency Model

- single receive loop
- guarded reconnect logic
- prevents duplicate reconnect attempts

---

## Design Principles

- sealed module boundary
- protocol isolated from routing
- transport isolated from meaning
- explicit behavior
- no cross-module leakage

---

## Known Limitations

- no command replay on reconnect
- retry policy not configurable
- no persistence/logging integration

---

## Summary

WebRcon is:

- the transport layer
- the protocol boundary
- the command formatter

WebRcon is NOT:

- a router
- a message interpreter
- a state manager

It converts structured requests into wire format and back.