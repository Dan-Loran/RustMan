# WebRcon Module (Infrastructure)

## Purpose

The WebRcon module provides a sealed boundary for Rust WebRCON communication.

It is responsible for:
- establishing and maintaining a WebSocket connection to a Rust server
- serializing outbound commands
- deserializing inbound messages
- managing attachment lifecycle (connect, reconnect, fault)
- emitting typed messages to downstream modules

It is NOT responsible for:
- message interpretation
- routing
- UI or presentation logic
- server lifecycle (start/stop/restart)

---

## Architectural Position

WebRcon is the first module in the runtime pipeline:

    WebRcon → Router → Console Interpretation → Console Stream State → Presentation → Web

It is the only module allowed to:
- handle JSON
- interact with WebSocket transport

---

## Module Boundary

### Inputs (via IWebRconModule)
- ConnectAsync(WebRconConnectionRequest)
- SendCommandAsync(WebRconCommandRequest)
- SetConsumer(IWebRconConsumer)

### Outputs (via IWebRconConsumer)
- ConnectionStateChanged
- MessageReceived (typed WebRconInboundMessage)
- ErrorOccurred (WebRconError)

### Rule

Modules communicate only through contracts.  
No external code may access WebRcon internals.

---

## Internal Structure

WebRcon is composed of three internal components:

    WebRconModule (Runtime)
      ├── Connection (WebSocket transport)
      └── Protocol (JSON translation)

### Runtime (WebRconModule)

Owns:
- orchestration
- receive loop
- reconnect behavior
- state transitions
- consumer notifications

### Connection (WebRconConnectionClient)

Owns:
- ClientWebSocket
- sending raw text
- receiving raw text
- frame assembly

Does NOT:
- know JSON
- know message meaning
- implement reconnect logic

### Protocol (WebRconProtocolTranslator)

Owns:
- outbound serialization
- inbound deserialization
- conversion to typed payloads

Does NOT:
- manage transport
- manage connection state
- perform routing

---

## Data Boundary Rule

All JSON handling is isolated inside WebRcon.

- outbound: typed → JSON (Protocol)
- inbound: JSON → typed (Protocol)

No raw JSON leaves the module.

---

## Payload Types

WebRcon produces typed payloads:

- WebRconTextPayload (default)
- WebRconChatPayload (when Type == "Chat")

Rules:
- unknown or invalid chat payloads fall back to text
- invalid outer JSON throws at the protocol boundary

---

## Connection Behavior

- uses ClientWebSocket
- connects using: ws(s)://host:port/{password}
- sends UTF-8 text messages
- receives and assembles full text messages
- returns null on orderly socket closure

---

## Runtime Behavior

### ConnectAsync

- requires consumer to be set
- emits: Connecting → Connected
- starts receive loop
- retries on failure (see retry policy)

### SendCommandAsync

- serializes command via Protocol
- sends via Connection
- reports error and rethrows on failure
- triggers reconnect if transport is lost

---

## Receive Loop

- continuously reads from Connection
- translates via Protocol
- emits typed messages to consumer

Behavior:

- message received → forwarded to consumer
- ReceiveAsync returns null → triggers reconnect
- transport exception → triggers reconnect
- translation failure → report error, continue

---

## Reconnect Behavior

### Triggers
- connection closed
- transport failure
- send failure due to lost socket

### Does NOT trigger reconnect
- protocol/JSON errors
- malformed messages

---

## Retry Policy (internal)

- initial attempt + 3 retries
- 2 second delay between retries

---

## State Transitions

Initial connect:

    Connecting → Connected

Initial failure:

    Connecting → Faulted

Connection loss:

    Connected → Reconnecting → Connected

Reconnect failure:

    Reconnecting → Faulted

---

## Error Behavior

- errors are reported via WebRconError
- terminal error emitted only after retry exhaustion
- translation errors do not terminate connection

---

## Concurrency Model

- single receive loop
- single reconnect flow

Guarded by:
- _syncLock
- _reconnectTask
- _receiveLoopTask

Prevents:
- duplicate reconnect attempts
- multiple receive loops

---

## Design Principles

- sealed module boundary
- explicit contracts only
- no cross-module leakage
- boring, readable code
- transport isolated from meaning
- JSON isolated to one module
- no command replay on reconnect

---

## Known Limitations (by design)

- no reconnect configuration yet
- no disconnect API
- no command replay
- no persistence/logging integration
- no router integration yet

---

## Future Work

- Router module integration
- Console Interpretation module
- Presentation layer integration
- configurable retry policy (optional)
- structured logging module

---

## Summary

WebRcon is a self-contained attachment module that:

- attaches to a running Rust server
- maintains that attachment with retry logic
- translates all protocol data into typed messages
- exposes a clean, contract-driven interface to the rest of the system

It is intentionally isolated so it can be:
- tested independently
- replaced if needed
- reasoned about without external context
