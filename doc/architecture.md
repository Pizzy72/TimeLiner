# Architecture Overview

## General Structure

TimeLiner is implemented as a classic layered WPF application following the MVVM pattern.
The code base is organized by responsibility rather than by feature, which keeps dependencies
explicit and limits coupling between UI, presentation logic, and data models.

At a high level, the application consists of:

- UI layer (Views and reusable UI components)
- Presentation layer (ViewModels)
- Domain layer (Models)
- Infrastructure and cross-cutting utilities
- Supporting services (themes, persistence)

There is no strict enforcement of layer boundaries by tooling, but the structure is consistently
applied throughout the project.

---

## Entry Point and Application Lifetime

The application starts in `App.xaml` and `App.xaml.cs`.

Responsibilities at startup include:

- initialization of application-wide resources
- loading of themes and styles
- initialization of global services
- restoration of persisted user settings

The main window (`MainWindow`) is created after the application resources are available, ensuring
that theming and styling are applied consistently from the start.

---

## UI Layer (Views and UI Components)

The UI layer consists of:

- XAML-based views located in the `Views` folder
- reusable UI components located in the `UI` folder

Each view is typically paired with a corresponding ViewModel and contains no business logic.
Code-behind is limited to:

- UI-specific behavior
- interaction with WPF infrastructure
- dialog handling where MVVM-only solutions would be impractical

Reusable UI elements such as the custom ribbon window base class and the custom message box
are implemented centrally to avoid duplication and to encapsulate WPF-specific logic.

Timeline collections are presented with WPF `ItemsControl` instances. This keeps rendering
flexible while allowing the ViewModels to expose filtered and sorted `ICollectionView`
instances. Drag-and-drop and cursor changes are handled in view code-behind because they
depend directly on WPF mouse and capture APIs.

---

## Presentation Layer (ViewModels)

The `ViewModels` folder contains the presentation logic of the application.

Characteristics:

- ViewModels expose state and commands for data binding
- no direct dependency on concrete views
- minimal awareness of UI layout or control types

A locator-based approach is used to associate ViewModels with views.
ViewModels typically coordinate:

- user interaction
- transformation between models and UI-friendly representations
- command execution and validation

This layer is intentionally fine-grained. Each dialog and functional UI element has its own
ViewModel to keep responsibilities small and explicit.

---

## Domain Layer (Models)

The `Models` folder contains data-centric classes that represent:

- timelines
- timeline items
- application and window settings

Models are free of UI concepts and can be tested independently.
Persistence-related abstractions (for example repositories) are defined here to decouple
storage format from application logic.

The structure and responsibilities of the core data model are described in more detail
in [Data Model](data-model.md).

---

## Infrastructure and Utilities

Cross-cutting functionality is located in the `Common` and `Converter` folders.

### Common

This includes:

- shared helper classes
- extensions
- base abstractions
- domain-specific constants and exceptions

These components are intentionally lightweight and do not depend on UI elements.

### Converter

Value converters are used to bridge the gap between ViewModel data and WPF bindings.
They are treated as UI infrastructure and are not part of the domain model.

Converters are kept simple and stateless.

---

## Theme System

The theme system is implemented as part of the UI infrastructure and is responsible
for applying light and dark themes at runtime.

Details about the theme mechanism and runtime behavior are described in
[Theme System](theme-system.md).

---

## Persistence

Persistence concerns are isolated from UI logic.

Responsibilities include:

- loading and saving user settings
- restoring window state
- persisting application preferences

JSON is used as the primary storage format for user-specific data.
Persistence logic is designed to be replaceable and testable.

Details about persistence formats and responsibilities are documented in [Persistence](persistence.md).

---

## Third-Party Libraries

TimeLiner keeps external dependencies limited to UI composition. MVVM infrastructure,
dialog handling, and settings serialization are implemented in the application itself
using .NET platform APIs.

These dependencies are used selectively to reduce custom infrastructure code and to keep the
focus on application-specific logic.

Details about external libraries and their role within the application are documented in
[Third-Party Libraries](third-party-libraries.md).

---

## Testing Strategy

Automated tests are located in a separate test project.

The focus of testing is on:

- domain models
- ViewModels
- parsing and validation logic

UI elements are not tested directly. This is a conscious decision to keep tests stable
and focused on logic rather than visual representation.

---

## Architectural Boundaries and Trade-offs

The architecture prioritizes clarity and maintainability over maximum performance.

In particular:

- MVVM simplifies structure and testability
- large data models may cause performance degradation due to binding overhead

These trade-offs are intentional and are documented in more detail in a separate section.
