# Decisions and Limitations

This document summarizes key architectural decisions made in TimeLiner and the
resulting limitations. The intent is to capture rationale and context for future
maintenance and development.

---

## Technology Stack

### WPF and .NET 10

**Decision**  
TimeLiner is implemented as a Windows desktop application using WPF on
.NET 10 for Windows.

**Rationale**  
WPF provides a mature and flexible UI framework with strong data binding support
and integrates well with the Windows desktop environment.

**Consequences**  
- the application is Windows-only
- long-term evolution follows the supported .NET release cycle
- the application remains Windows-only because it uses WPF

---

### Fluent Ribbon

**Decision**  
A ribbon-based UI is used via Fluent Ribbon.

**Rationale**  
The ribbon metaphor allows clear grouping of commands and scales well with the
feature set of the application.

**Consequences**  
- UI layout is structured around command groups
- customization outside the ribbon paradigm is limited

---

## Architectural Pattern

### Application-owned MVVM infrastructure

**Decision**  
The MVVM pattern is used consistently, supported by small application-owned
base classes, commands, and services.

**Rationale**  
MVVM provides clear separation of concerns, improves testability, and keeps
presentation logic independent of concrete views.

**Consequences**  
- a locator-based approach is used instead of dependency injection
- UI logic is expressed primarily through bindings and commands
- binding overhead affects performance for large data sets

---

## Data Model and Persistence

### CSV for Timeline Data

**Decision**  
Timeline data is persisted using a CSV-based file format.

**Rationale**  
CSV is simple, transparent, and compatible with common external tools such as
spreadsheet applications.

**Consequences**  
- limited expressiveness compared to structured formats
- strict parsing required to avoid ambiguous data
- manual edits are possible but error-prone

---

### JSON for Application Settings

**Decision**  
Application and window settings are stored using JSON.

**Rationale**  
JSON is human-readable, flexible, and well-supported by existing libraries.

**Consequences**  
- settings files are easy to inspect and modify
- schema evolution must remain backward-compatible

---

## Performance Characteristics

### Scalability Limits

**Decision**  
The architecture prioritizes clarity and maintainability over maximum scalability.

**Rationale**  
Typical use cases involve moderate data sizes where responsiveness and correctness
are more important than handling extreme volumes.

**Consequences**  
- performance may degrade with very large numbers of timelines or items
- rendering and binding overhead dominates runtime behavior
- optimization efforts focus on usability rather than raw throughput

---

## Scope and Non-Goals

### Explicit Non-Goals

The following aspects are intentionally not addressed by the current architecture:

- real-time collaboration
- background data processing
- plug-in or extension systems
- server-side components

These features would significantly increase complexity and are outside the intended
scope of the application.

---

## Summary

The design of TimeLiner is based on pragmatic decisions tailored to its intended use.

Architectural choices favor:

- clarity over abstraction
- explicit behavior over automation
- maintainability over extreme scalability

Documenting these decisions ensures that trade-offs remain visible and intentional
over the lifetime of the project.
