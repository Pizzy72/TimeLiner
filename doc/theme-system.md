# Theme System

This document describes the theme mechanism used in TimeLiner.
It focuses on responsibilities, runtime behavior, and design decisions rather than
on individual styles or color definitions.

---

## Overview

TimeLiner supports two visual themes:

- Light
- Dark

The theme system is designed to:

- provide a consistent appearance across the application
- support switching between light and dark mode
- persist the selected theme across application restarts

Theme switching is supported both at application startup and at runtime.

---

## Relation to Fluent Ribbon

The TimeLiner themes are based on the corresponding Fluent Ribbon themes:

- `Light.Blue.xaml`
- `Dark.Blue.xaml`

Rather than redefining complete styles, TimeLiner aligns with the Fluent Ribbon
themes and only overrides selected colors and brushes.

This approach minimizes the number of custom styles and reduces runtime overhead.

---

## Theme Resources

The theme system is composed of the following resource dictionaries:

- `LightTheme.xaml`
- `DarkTheme.xaml`
- `Styles.xaml`

### LightTheme.xaml and DarkTheme.xaml

These files define theme-specific resources:

- colors
- brushes

They intentionally do not contain control styles.

### Styles.xaml

`Styles.xaml` contains all TimeLiner-specific styles that are independent of
the active theme.

By separating styles from theme-dependent resources, theme switching can be
performed efficiently by replacing only the color and brush dictionaries.

---

## Theme Service

Theme switching is coordinated by the `ThemeService`, which implements
`IThemeService`.

Responsibilities of the ThemeService include:

- loading the appropriate theme resource dictionary
- replacing the active theme at runtime
- ensuring consistent application of the selected theme

The ThemeService does not persist settings and does not contain UI logic.

---

## Theme Selection and Persistence

The selected theme is part of the application settings and is stored in
`SettingsModel`.

### Application Startup

At application startup:

1. persisted settings are loaded
2. the selected theme is read from `SettingsModel`
3. the ThemeService applies the corresponding theme

This logic is executed during application initialization in `App.xaml.cs`,
ensuring that the UI starts in the correct theme.

---

### Runtime Theme Switching

At runtime, the user can switch the theme using a checkbox in the main window.

The checkbox is bound to `SettingsViewModel.SelectedTheme`.

When the value changes:

1. the ViewModel updates the setting
2. the ThemeService replaces the active theme resource dictionary
3. the UI updates automatically

No application restart is required.

---

## Design Decisions

Key design decisions of the theme system include:

- limiting themes to light and dark variants
- aligning with Fluent Ribbon themes
- replacing only colors and brushes instead of full styles
- centralizing theme switching in a dedicated service

These decisions keep the theme system predictable, performant, and easy to maintain.

---

## Summary

The TimeLiner theme system provides a lightweight and efficient mechanism for
supporting light and dark modes.

Theme application is centralized, settings-driven, and decoupled from both
UI logic and persistence mechanics.
