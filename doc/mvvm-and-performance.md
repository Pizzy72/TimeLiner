# MVVM Usage and Performance Considerations

This document describes how the MVVM pattern is used in TimeLiner and explains the
performance characteristics and limitations that result from this architectural choice.

The intent is to make these trade-offs explicit for future maintenance and development.

---

## MVVM in TimeLiner

TimeLiner follows the Model-View-ViewModel (MVVM) pattern consistently across the user interface.

The main goals of using MVVM are:

- clear separation between UI and application logic
- improved testability of non-visual logic
- predictable data flow between models and views

MVVM is implemented with lightweight application-owned base classes, commands, and services.
No external MVVM framework is required.

---

## Responsibilities of Each Layer

### Models

Models represent the domain data of the application, including:

- timelines
- timeline items
- application and window settings

They are free of UI concerns and can be tested independently.

### ViewModels

ViewModels act as the presentation layer:

- they expose properties for data binding
- they provide commands for user interaction
- they translate domain data into UI-friendly representations

ViewModels do not reference concrete views and do not depend on WPF control types.

### Views

Views are responsible for:

- visual layout
- styling and theming
- binding to ViewModel properties and commands

Code-behind is limited to UI-specific behavior that cannot reasonably be expressed via bindings.

---

## ViewModel Granularity

TimeLiner uses relatively fine-grained ViewModels.

Each major UI element or dialog typically has its own ViewModel. This approach:

- keeps responsibilities small and explicit
- simplifies reasoning about individual UI components
- improves testability

The trade-off is an increased number of bindings and notification events at runtime.

---

## Performance Characteristics

The performance of TimeLiner is largely determined by WPF data binding behavior.

In particular:

- property change notifications are propagated through bindings
- complex visual trees amplify binding overhead
- frequent updates can trigger layout and rendering work

For typical data sizes, this approach performs well and provides a responsive user experience.

---

## Known Performance Limitations

When working with very large data models, performance may degrade noticeably.

Typical scenarios include:

- a large number of timelines
- many timeline items visible at once
- frequent updates to bound properties

These limitations are a direct consequence of:

- the number of active bindings
- the cost of change notification propagation
- WPF layout recalculation

This behavior is expected and not considered a defect.

---

## Design Trade-offs

The use of MVVM in TimeLiner represents a conscious trade-off:

**Advantages**
- clear structure
- maintainable code base
- good test coverage for logic
- low coupling between UI and logic

**Disadvantages**
- reduced scalability for very large data sets
- higher runtime overhead compared to more imperative UI approaches

Given the intended use cases of TimeLiner, this trade-off is considered acceptable.

---

## Possible Alternatives

Alternative approaches could improve performance for large data sets, for example:

- reducing ViewModel granularity
- limiting the number of active bindings
- moving parts of the UI logic closer to the view layer

These approaches would increase complexity and reduce the clarity provided by MVVM.

They have therefore not been pursued.

---

## Summary

MVVM provides a solid and maintainable foundation for TimeLiner.

Performance limitations for large data models are a known and documented consequence of this
choice. The architecture favors clarity and correctness over maximum scalability.
