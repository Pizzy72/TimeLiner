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
share an offset and daylight-saving rules change over time. Time zone selection is
therefore treated as a display setting rather than inferred from imported timestamps.

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
