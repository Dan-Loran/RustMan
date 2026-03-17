# RustMan Module-Forward Architecture Notes

## Purpose

This document exists to preserve architectural intent across sessions.

The goal is not elegance.  
The goal is **low context reload cost** and **safe, isolated development**.

If something in the system feels hard to reason about, assume a boundary has been violated.

---

## Core Philosophy

Design the system like electrical components:

- Modules are sealed units (ICs / relays)
- Contracts are the wiring harness
- Signals flow through defined connectors
- Internals are irrelevant outside the module

> Do not share wires. Share connectors.

---

## Primary Goal

Enable development where:

- each module can be understood in isolation
- a new session can resume from contracts, not history
- no part of the system requires full-system context to modify safely

---

## System Layers

### Core
Defines contracts only (the wiring harness)

- message contracts
- command contracts
- enums and identifiers
- shared value shapes

Rules:
- no runtime behavior
- no UI concerns
- no infrastructure details

---

### Infrastructure
Implements modules (the circuitry)

- WebRCON
- Router
- Console interpretation
- runtime state

Rules:
- owns live state
- owns behavior
- no UI knowledge
- no presentation formatting

---

### Presentation
Defines the actual user interface (control panel design)

- shapes runtime state into user-facing models
- defines what the operator sees and can do

Rules:
- may combine and shape data
- must not own or mutate runtime state
- must not contain transport or protocol logic

---

### Web
Rendering only (faceplate)

- Razor components
- layout
- CSS

Rules:
- depends only on Presentation
- no business logic
- no protocol knowledge
- no runtime state ownership

> UI is a display.  
> Presentation is the interface.

---

## Module Design Rules

### 1. Modules are sealed

A module:
- has defined inputs
- has defined outputs
- owns its internal behavior
- exposes no internals

---

### 2. Modules communicate only through contracts

- no direct access to another module’s internal classes
- no shared mutable objects across modules
- no “just call into it” shortcuts

If this happens:
- the contract is wrong, or
- the boundary is wrong

---

### 3. Contracts are the wiring harness

Contracts must be:
- small
- explicit
- stable
- boring

They define:
- what goes in
- what comes out

They must NOT:
- expose internal state
- include convenience data
- grow without clear reason

---

### 4. Share only what shares a reason to change

Do not share classes because they look similar.

Only share when:
- all consumers depend on it for the same reason
- changes will naturally affect all consumers

Otherwise:
- duplicate and keep local

> Group what changes together. Separate what changes independently.

---

### 5. Runtime owns truth

- connection state
- message flow
- buffers

No other layer owns or mutates runtime truth.

---

### 6. Presentation defines meaning for the user

- interprets runtime state for display
- exposes actions

Does NOT:
- control runtime behavior
- contain protocol logic

---

### 7. UI renders only

- no interpretation
- no decision-making
- no knowledge of modules

---

## WebRCON Module (Reference)

### Responsibility
Transport + protocol conversion.

### Inputs
- connect
- disconnect
- command

### Outputs
- connection state
- inbound message (converted)
- error

### Rules
- knows message structure
- does NOT know message purpose

> WebRCON knows *what the message is structurally*, not *what it means to the system*.

---

## Router Module (Reference)

### Responsibility
- classify messages
- optionally correlate command IDs
- route to downstream modules

### Rules
- no state ownership beyond minimal correlation
- no business logic
- no presentation logic

---

## Console Interpretation Module (Reference)

### Responsibility
- decide if message contributes to console
- convert message to ConsoleEntry
- assign message type (Info, Error, Chat, etc.)

### Rules
- defines identity, not appearance
- no CSS
- no UI concerns

> Define what it is, not how it looks.

---

## Console Stream Module (Reference)

### Responsibility
- store console entries
- expose current stream state

---

## Key Architectural Rules (Non-Negotiable)

- No module may access another module’s internals
- No UI code outside Web
- Web depends only on Presentation
- Presentation does not mutate runtime
- Contracts do not leak implementation details

---

## Testing Philosophy

Modules should be testable like components:

- input → output
- no system setup required
- no UI involved

Test:
- identity
- behavior

Not:
- appearance
- rendering

---

## Smell Detection

If you see:

- UI needing runtime knowledge → boundary broken
- shared “helper” classes across modules → leakage
- contracts growing large → overexposure
- modules needing system context → poor isolation

Stop and fix the boundary.

---

## Guiding Principle

> Make every module understandable without loading the whole system.

If a module requires explanation of other modules to understand:

- it is not isolated enough

---

## Final Summary

- Modules = components  
- Contracts = wiring harness  
- Presentation = interface  
- UI = display  

Keep it boring.  
Keep it explicit.  
Keep it isolated.

## Additional Rule: UI Control State Lives in Presentation

UI control state must be owned by the Presentation layer, not the Web/UI layer.

Examples:
- whether a button is enabled
- status text to display
- selected item identity
- current operator-facing mode
- validation messages
- visible section state if it reflects interface behavior

The Web layer may:
- bind to control state
- render control state
- forward user actions

The Web layer must NOT:
- decide control state
- store interface state as source of truth
- interpret runtime data to determine operator behavior

Reason:
If UI control state lives in the Web layer, the display stops being replaceable and starts owning interface behavior.

Rule:
Presentation defines the interface state.
Web only renders it.