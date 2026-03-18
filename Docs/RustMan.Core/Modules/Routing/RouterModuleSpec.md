# Router Module Spec

## Purpose

Provide a sealed boundary for inbound message classification and routing.

This module owns:
- receiving converted inbound WebRCON messages
- minimal command/response correlation by message ID
- routing messages to the correct downstream module

This module does NOT interpret presentation meaning or own application state.

---

## Responsibility

- receive converted inbound messages from WebRCON
- receive outbound command metadata when correlation is needed
- track pending command IDs in memory
- match inbound response messages to pending command IDs
- route inbound messages to downstream module boundaries
- remove matched pending command IDs

---

## Inputs (Pins In)

### InboundMessageReceived
A converted inbound WebRCON message.

- Must conform to shared contract
- Must preserve message ID if present

---

### OutboundCommandSent
Notification that a command has been sent or is about to be sent.

- Command ID
- Optional minimal metadata needed for correlation

This input exists only to support response matching.

---

### ConnectionStateChanged
Connection lifecycle notification from WebRCON.

Used to clear transient correlation state when appropriate.

Examples:
- Connected
- Disconnected
- Error

---

## Outputs (Pins Out)

### RoutedConsoleCandidate
A message routed to the Console Interpretation module.

This output means:
- the message belongs on the console processing path

It does NOT mean:
- the message will definitely become a console entry

That decision belongs to the Console Interpretation module.

---

### RoutedCommandResponse
A message matched to a pending command ID.

This output means:
- the inbound message has been correlated to a previously sent command

It does NOT mean:
- the message has been interpreted for presentation

---

### RoutedUnhandledMessage
A message that was valid inbound traffic but did not match a known command response path and was not classified into another defined route.

This output allows the system to:
- ignore it
- log it
- route it later if a new module is added

---

### RouterErrorOccurred
Indicates a routing or correlation failure.

Examples:
- invalid correlation state
- duplicate pending command ID
- unsupported routing condition

---

## Internal Responsibilities

- maintain minimal in-memory pending command ID tracking
- check inbound message ID against pending commands
- remove pending command on successful match
- classify inbound messages for downstream routing
- clear transient correlation state on disconnect/error

---

## Non-Responsibilities (Hard Rules)

The module must NOT:

- parse raw JSON
- own WebSocket connection behavior
- interpret console meaning
- interpret chat meaning
- own buffers or message history
- own presentation state
- know about UI or Presentation
- implement business workflows
- retry commands
- manage command timeout policy unless explicitly added later

---

## Contract Ownership

All input/output contracts must live in:

RustMan.Core/Modules/Routing

---

## Correlation Rule

The router may keep only the minimum transient state needed to answer:

- does this inbound message match a previously sent command?

The router must NOT evolve into command workflow management.

Allowed:
- pending command ID tracking
- match and remove

Not allowed:
- retries
- command lifecycle history
- business status tracking
- UI ownership

---

## State Ownership Rule

- Router owns transient correlation state only
- Router does NOT own message history
- Router does NOT own console state
- Router does NOT own runtime truth outside routing/correlation

---

## Delivery Rule

- WebRCON has one direct downstream consumer: Router
- Router fans messages out through explicit output contracts
- Downstream modules must not inspect Router internals

---

## Disconnect Cleanup Rule

On disconnect or terminal connection error:

- clear all pending command correlation state

Reason:
- correlation state is runtime-only and short-lived

---

## Testing Expectations

This module should be testable by:

- providing outbound command metadata
- providing inbound converted messages
- verifying matched vs unmatched routing behavior
- verifying correlation cleanup on disconnect

Tests should NOT require:
- WebSocket connection
- raw JSON parsing
- UI
- Presentation layer
- full system setup

---

## Summary

Router is a sealed classification and correlation module.

It knows:
- where a message should go
- whether a message matches a pending command

It does NOT know:
- what the message means to the user
- how the message is displayed
- how other modules do their work