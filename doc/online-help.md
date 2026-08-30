# Online Help

This document describes how the end-user online help for TimeLiner is created,
maintained, and integrated into the application.

It focuses on the documentation workflow rather than on the content of the help
itself.

---

## Purpose

The online help is intended for end users of TimeLiner.

It provides:

- usage instructions
- explanations of UI elements
- examples and screenshots

The help is accessed from within the application via the F1 key and is distributed
as a PDF document.

Developer-oriented documentation is maintained separately in the `doc` folder.

---

## Source Document

The primary source of the online help is a LibreOffice document:

- `TimeLinerHelp.odt`

This file is considered the single source of truth for the help content.
All textual changes to the online help are made in this document.

The PDF file is a generated artifact and should not be edited directly.

---

## PDF Generation

The end-user help is distributed as:

- `TimeLinerHelp.pdf`

The PDF file is created by exporting `TimeLinerHelp.odt` using LibreOffice.

PDF generation is a manual step and is typically performed when:

- help content has changed
- screenshots or diagrams have been updated
- a new application version is prepared

---

## Graphics and Assets

Graphics used in the online help are created outside of the ODT document.

The following tools are used:

- **draw.io** for diagrams  
  (source file: `Sketch.drawio`)
- **Affinity** for screenshots  
  (the referenced source file `Screenshots.af` is not part of the current repository)

The generated images are embedded into `TimeLinerHelp.odt`. Their authorship and
redistribution status are tracked in `ASSET-PROVENANCE.md` at the repository root.

Source files for graphics are maintained separately to allow easy updates and
reuse.

---

## Integration into the Application

The generated PDF file is copied into the application build output.

At runtime:

- the PDF file is located relative to the application directory
- pressing F1 opens the PDF in the system's default PDF viewer

The help system is intentionally file-based and does not rely on online resources
or embedded HTML viewers.

---

## Design Decisions

Key design decisions for the online help include:

- using a PDF-based help format
- authoring content with LibreOffice
- creating diagrams and screenshots with dedicated tools
- keeping the help system independent of the application runtime

This approach provides a simple, robust, and maintainable help system with minimal
technical complexity.

---

## Summary

The TimeLiner online help is maintained as a separate documentation artifact.

Content is authored in a single LibreOffice document, enriched with externally
created graphics, and copied as a PDF file into application build output.
