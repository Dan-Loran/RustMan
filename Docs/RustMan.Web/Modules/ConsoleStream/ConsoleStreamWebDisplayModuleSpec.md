# Console Stream Web Display Module Spec

## Purpose

Provide a display-only rendering surface for the console stream interface.

This module owns:
- rendering presentation models
- forwarding operator actions to Presentation-defined inputs
- display-layer-only concerns

This module does NOT define interface behavior.

---

## Responsibility

- render ConsoleStreamViewModel
- bind controls to presentation state
- forward user actions through presentation-defined action contracts
- apply display styling

---

## Inputs (Pins In)

### ConsoleStreamViewModel
A presentation-defined model for rendering console stream state.

### ConsoleStreamInterfaceActions
Presentation-defined action surface for console interactions.

---

## Outputs (Pins Out)

### UserActionForwarded
A Web-layer forwarding action to the Presentation layer.

Examples:
- clear requested
- command submit requested
- selection changed if the interface contract allows it

---

## Internal Responsibilities

- render lines
- render control state
- map identity/type to CSS/styling
- handle local rendering concerns

Examples of allowed local concerns:
- scroll behavior
- focus behavior
- animation flags
- transient input field text before submission

---

## Non-Responsibilities (Hard Rules)

The module must NOT:

- decide whether a control is enabled
- interpret runtime console state
- classify messages
- own interface behavior
- own runtime truth
- parse JSON
- route messages
- directly depend on Infrastructure modules

---

## Dependency Rule

Web depends only on Presentation.

Web must not depend directly on:
- Infrastructure module implementations
- runtime state modules
- protocol conversion logic

---

## CSS Rule

CSS and rendering choices belong here.

Allowed:
- map message identity to CSS classes
- choose visual formatting

Not allowed:
- define message identity in Web
- move behavior decisions into rendering

---

## State Rule

Web may own display-only state.

Allowed examples:
- transient text box contents
- focus flags
- local scroll position
- component lifecycle flags

Not allowed examples:
- source-of-truth button enablement
- operator-facing status meaning
- interface availability rules
- selected identity when it affects system behavior

Those belong in Presentation.

---

## Testing Expectations

This module should be testable by:

- supplying presentation view models
- verifying rendering behavior
- verifying event forwarding

Tests should NOT require:
- Infrastructure modules
- WebSocket
- Router internals
- protocol parsing

---

## Summary

Console Stream Web Display is a display-only module.

It knows:
- how to render the interface

It does NOT know:
- what the interface means
- how the system works