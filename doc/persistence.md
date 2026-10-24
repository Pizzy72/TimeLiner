# Persistence

This document describes how TimeLiner persists application state, user settings,
and timeline data. The focus is on responsibilities, formats, and design decisions.

---

## Overview

Persistence in TimeLiner is deliberately simple and file-based.

Two different persistence concerns are handled separately:

- application and window settings (JSON)
- timeline data (CSV)

This separation keeps responsibilities clear and avoids coupling between user
preferences and domain data.

---

## Application Settings Persistence

### SettingsModel

Application-wide user settings are represented by `SettingsModel`.

Persisted settings include:

- time display mode (UTC or local time)
- grid layout preferences
- visibility options
- selected application theme
- main window settings

Settings are serialized and deserialized using JSON.

### Storage Format

JSON is used as the persistence format for settings because:

- it is human-readable
- it supports version-tolerant evolution of the data structure
- it integrates well with the existing .NET ecosystem

Serialization is implemented using a dedicated repository abstraction to decouple
storage mechanics from application logic.

### Storage Location

The settings file is stored in the user-specific application data directory.

On Windows, this typically resolves to:

%APPDATA%\TimeLiner\

The exact file name and location are determined by the settings repository
implementation.

This location is relevant for debugging and manual inspection of persisted
application state.

---

## Window State Persistence

Window geometry and state are persisted separately using `WindowSettingsModel`.

Design characteristics:

- window position and size always reflect the last normal (non-maximized) state
- the maximized state is persisted as a separate flag
- coordinates are stored in device-independent pixels (DIP)

This approach avoids issues when restoring maximized windows across different
screen configurations.

---

## Timeline Data Persistence

Timeline data is persisted using a CSV-based file format.

### CSV Format Characteristics

- one row represents one timeline item
- timeline names are repeated to group items logically
- start and end times are stored in ISO 8601 format
- empty end times represent point-in-time events
- optional color information is stored as a string

The CSV format is intentionally strict to ensure predictable parsing and early
error detection.

### Time Zones and Offsets

CSV timestamps preserve an instant in time, including its UTC offset where present.
An offset alone cannot identify the original time zone because multiple time zones can
share an offset and daylight-saving rules change over time. Regional time zone
selection is therefore a display setting, never inferred from imported timestamps.

Alternatively, all events can be displayed with a fixed UTC offset, without
daylight-saving adjustment. The offset can be supplied manually or using
`-offset ±HH:mm`. Choosing **First event** in the Ribbon, or `-offset auto`, uses
the original explicit start offset of the first event in file order. An empty
timeline does not count as an event. A missing start offset is not taken from the
machine's local zone or from a later event. Other events may have different
original offsets; the display still uses one common offset.

The original first-event offset is retained as in-memory import context, including
undo snapshots. It is refreshed on file load, not on editing or reordering events.
The chosen mode, manual/last resolved offset, and automatic-source preference are
stored in application settings. Existing settings preserve their UTC/regional
choice. No additional CSV metadata is written.

---

## Import and Export Behavior

### Import

When importing CSV data:

- input is validated line by line
- invalid column counts result in errors
- start and end times are normalized to UTC
- unknown color names are rejected

Parsing errors are reported with line number information to support troubleshooting.

### Export

When exporting data:

- times are written in UTC using a standardized format
- separators and special characters are sanitized
- empty timelines are preserved explicitly

The export format is compatible with common spreadsheet tools.
Because export writes UTC, the first-event offset on reopening an exported file
is zero. The original input offsets and the display zone are not persisted in CSV.

---

## Relationship to the Data Model

Persistence logic operates on model-level abstractions rather than ViewModels.

Consequences:

- the data model remains independent of storage concerns
- persistence can be tested independently
- changes to the UI do not require changes to persistence logic

Details of the data model itself are documented in [Data Model](data-model.md).

---

## Error Handling

Persistence errors are treated as user-visible errors.

Typical error cases include:

- invalid file formats
- malformed CSV data
- invalid time or color values

Errors are reported with descriptive messages to allow users to correct input data.

---

## Summary

Persistence in TimeLiner:

- uses simple, transparent file formats
- separates settings from domain data
- favors clarity and robustness over abstraction

This approach supports long-term maintainability and predictable behavior.
