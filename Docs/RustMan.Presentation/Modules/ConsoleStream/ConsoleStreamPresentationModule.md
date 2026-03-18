# Console Stream Presentation Module Spec

## Purpose

Provide the actual operator-facing interface for console stream behavior.

This module owns:
- shaping Console Stream runtime state into operator-facing models
- defining console control state for the display layer
- exposing interface actions for console interaction

This module does NOT own runtime truth.

---

## Responsibility

- read Console Stream state
- shape state into presentation models
- define control state for console-related actions
- expose operator-facing interface contracts for the Web display layer

---

## Inputs (Pins In)

### ConsoleStreamStateChanged
A current snapshot of console stream state from the Console Stream module.

---

### ConnectionPresentationInput
Optional connection-related presentation input if console controls depend on connection state.

Examples:
- whether sending commands is allowed
- whether clear or pause actions are available

This input should come from Presentation-facing contracts, not directly from Web.

---

## Outputs (Pins Out)

### ConsoleStreamViewModel
A display-ready model for the Web layer.

This may include:
- console lines ready for rendering
- line identity/type
- operator-facing status text
- empty-state text
- selected-state identity if needed
- control enable/disable state

---

### ConsoleStreamInterfaceActions
Presentation-defined actions available to the Web layer.

Examples:
- ClearRequested
- SendCommandRequested

These are interface actions, not runtime implementation details.

---

## Internal Responsibilities

- shape runtime console state for operator use
- derive display-ready values
- derive operator-facing text
- derive control state
- expose a stable interface contract to Web

---

## Non-Responsibilities (Hard Rules)

The module must NOT:

- own or mutate runtime console truth
- parse raw JSON
- route messages
- interpret transport protocol
- render HTML/Razor
- assign CSS classes tied to implementation details in Web
- store display-layer-only temporary rendering quirks as source of truth

---

## Contract Ownership

All presentation-facing contracts must live in:

RustMan.Presentation/Modules/ConsoleStream

---

## Interface State Rule

UI control state belongs here, not in Web.

Examples:
- whether a button is enabled
- operator-facing status text
- selected console item identity if behavior depends on it
- empty-state messaging

Web may render this state, but must not decide it.

---

## Display Identity Rule

This module may expose message identity suitable for display decisions.

Examples:
- Info
- Warning
- Error
- Chat
- System

This module should still avoid hardcoding rendering technology details.

---

## State Ownership Rule

- Presentation owns interface state
- Presentation does NOT own runtime state
- Presentation is the source of truth for what the operator can see and do

---

## Testing Expectations

This module should be testable by:

- providing console stream state snapshots
- verifying produced view model shape
- verifying control enabled/disabled state
- verifying operator-facing text/state

Tests should NOT require:
- Razor components
- WebSocket
- Router
- full system setup

---

## Summary

Console Stream Presentation is the real user interface for console stream behavior.

It knows:
- what the operator should see
- what the operator can do

It does NOT know:
- how the display is rendered
- how runtime state is internally managed