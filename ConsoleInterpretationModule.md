# Console Interpretation Module Spec

## Purpose

Provide a sealed boundary for deciding whether a routed inbound message contributes to the console stream.

This module owns:
- filtering messages for console relevance
- converting eligible messages into ConsoleEntry objects
- assigning message identity/classification

This module does NOT decide presentation appearance.

---

## Responsibility

- receive routed inbound messages from Router
- determine whether the message should contribute to console output
- drop irrelevant messages
- convert relevant messages into ConsoleEntry contracts
- assign message type identity

---

## Inputs (Pins In)

### RoutedConsoleCandidate
A routed inbound message from Router.

- Must conform to shared routing contract
- May contain message ID
- May contain message content relevant to console output

---

## Outputs (Pins Out)

### ConsoleEntryCreated
A console entry ready for storage in the Console Stream module.

The output must include:
- console text/content
- timestamp if available or assigned by rule
- message type identity

Examples of message type identity:
- Info
- Warning
- Error
- Chat
- System

---

### MessageDropped
Indicates the routed message was examined and did not produce a console entry.

This output is optional unless needed for diagnostics.

---

### ConsoleInterpretationErrorOccurred
Indicates the message could not be interpreted correctly for console output.

Examples:
- unsupported shape
- invalid payload state
- required content missing

---

## Internal Responsibilities

- inspect routed message shape/content
- determine console relevance
- normalize console text
- assign message identity/type
- produce ConsoleEntry output

---

## Non-Responsibilities (Hard Rules)

The module must NOT:

- parse raw JSON
- own WebSocket behavior
- route messages
- store console history
- assign CSS classes
- know about UI or Presentation
- decide how a message is displayed
- own command correlation

---

## Contract Ownership

All input/output contracts must live in:

RustMan.Core/Modules/ConsoleStream

---

## Identity Rule

This module defines what the message is, not how it looks.

Allowed:
- Info
- Warning
- Error
- Chat
- System

Not allowed:
- CSS class names
- display styles
- color hints tied to rendering technology

---

## State Ownership Rule

- This module does NOT own console history
- This module does NOT own buffers
- This module is stateless apart from immediate interpretation work

---

## Testing Expectations

This module should be testable by:

- providing routed inbound messages
- verifying whether a ConsoleEntry is created
- verifying the assigned ConsoleMessageType
- verifying irrelevant messages are dropped

Tests should NOT require:
- WebSocket
- Router internals
- UI
- Presentation layer
- full system setup

---

## Summary

Console Interpretation is a sealed filtering and translation module.

It knows:
- whether a routed message belongs in console
- what type of console message it is

It does NOT know:
- how the message is rendered
- where the message is displayed