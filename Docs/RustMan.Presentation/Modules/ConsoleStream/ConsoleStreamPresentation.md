# ConsoleStream Presentation Module (As-Built)

## Purpose

ConsoleStream Presentation is the interface-layer module responsible for shaping the runtime console feed into operator-facing state.

It:

- receives console stream state updates from ConsoleStream
- projects runtime entries into neutral UI-facing view models
- owns presentation-side console state
- exposes read-only console view data for the Web layer

Presentation does **not** own runtime truth and does **not** contain styling logic.

---

## Position in Architecture

Runtime flow:

Server → WebRcon → Router → ConsoleStream

Presentation flow:

ConsoleStream → ConsoleStream Presentation → Web

This keeps runtime state ownership separate from interface state ownership.

---

## Responsibilities

### 1. Consume ConsoleStream updates

- implements `IConsoleStreamConsumer`
- receives `ConsoleStreamStateChanged`
- responds to runtime console snapshot changes

---

### 2. Project runtime data into interface data

- converts `ConsoleEntry` values into `ConsoleLineViewModel`
- exposes a `ConsoleStreamViewModel`

Projection preserves:

- `Text`
- `Type`
- `TimestampUtc`

No styling or CSS concepts are introduced.

---

### 3. Own presentation-side console state

Presentation stores the current operator-facing console view state.

Current state includes:

- `Lines`
- `IsEmpty`
- `CanClear`

This state is read-only from outside the module.

---

## Inputs

- `ConsoleStreamStateChanged`

---

## Outputs

- `ConsoleStreamViewModel`

---

## Current View Models

### ConsoleStreamViewModel

Represents the console as a whole for interface consumption.

Fields:

- `IReadOnlyList<ConsoleLineViewModel> Lines`
- `bool IsEmpty`
- `bool CanClear`

---

### ConsoleLineViewModel

Represents one visible console line.

Fields:

- `string Text`
- `string Type`
- `DateTime TimestampUtc`

---

## What Presentation Does NOT Do

Presentation does NOT:

- store runtime console truth
- buffer runtime messages independently
- perform routing
- access WebRcon
- access Router internals
- contain CSS classes
- contain platform-specific styling concepts
- contain HTML-specific concepts

---

## Platform-Neutral Rule

Presentation exposes neutral state and identifying information only.

Examples of allowed concepts:

- `Type`
- `IsEmpty`
- `CanClear`

Examples of forbidden concepts:

- CSS class names
- color names
- Tailwind classes
- HTML-specific formatting hints

The Web layer is responsible for translating neutral presentation state into actual appearance.

---

## Design Principles

- interface state only
- no styling leakage
- no runtime ownership leakage
- explicit and readable
- replaceable boundary as long as contracts are preserved

---

## Summary

ConsoleStream Presentation is:

- the interface-layer projection of the runtime console feed
- the owner of operator-facing console state
- the source of neutral view data for the Web layer

It is NOT:

- a runtime buffer
- a router
- a styling layer
- a Web-specific component