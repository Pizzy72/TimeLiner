# Changelog

All notable changes to TimeLiner are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [2.15.2] - 2026-09-05

### Changed

- Improved scrolling performance

## [2.15.1] - 2026-09-03

### Changed
- Reduced timeline grid line thickness for a finer appearance

### Fixed
- Fixed timeline item label collisions at extreme zoom-out levels and coincident item positions

## [2.15.0] - 2026-08-30

### Added
- Modern SDK-style project structure for .NET 10 and WPF
- Automated build, test, run, and installer workflows through Invoke-Build

### Changed
- Updated Fluent.Ribbon to version 11.0.2
- Improved light and dark theme styling for the Ribbon, expanders, and scrollbars
- Improved consistency and usability of the timeline and information panel controls
- Improved color dialog
- Replaced external MVVM and dialog infrastructure with application-owned implementations
- Updated the project and architecture documentation

### Removed
- Removed dependencies on MVVM Light, MvvmDialogs, Newtonsoft.Json, and Moq

### Fixed
- Fixed Fluent.Ribbon window-dragging compatibility issues
- Fixed styling and layout issues in light and dark mode

## [2.14.1] - 2026-08-04

### Fixed
- Fixed exception "The file does not contain TimeLiner data" when creating a new timeline with an item in an empty model. The validation in `LoadAsync` was too restrictive and prevented cloning of empty models, which is required for undo/redo functionality.

## [2.14.0] - 2026-06-17

### Added
- Undo/Redo functionality with keyboard shortcuts
- Compact timelines mode
- Dark mode with theme switching
- Command line parameter "-timezone"
- Keyboard shortcuts for navigation
- Tool to scroll timelines up/down by one page

### Changed
- Fixed warnings regarding async/await
- Improved text behavior and async/await handling
- Improved timeline item rendering
- Faster vertical and mouse scrolling
- Restore scale and scroll position after undo/redo
- Updated online help
- Truncate long timeline item names
- Compact info panel

### Fixed
- Fixed ranged zoom-in
- Fix exception in zoom tool
- Fix empty space after switching to compact timelines
- Fix time events in compact timeline mode

## [2.11.3] - 2025-05-12

### Changed
- Updated help
- Improved tooltip duration

## [2.11.2] - 2024-02-13

### Changed
- Updated help

## [2.11.1] - 2024-02-10

### Fixed
- Fixed scrolling bugs

## [2.11.0] - 2024-02-10

### Added
- Initial version with previous features

## [2.10.0.1] - 2023-04-19

### Changed
- Version bump

## [2.10.0] - 2022-09-25

### Added
- New features and improvements

## [2.9.0] - 2022-07-24

### Changed
- General improvements

## [2.8.1] - 2022-07-15

### Fixed
- Strip commas from names in output file

## [2.8.0] - 2022-07-15

### Added
- New features

## [2.7.2] - 2022-07-12

### Changed
- Show timeline item name in tooltip

## [2.7.1] - 2022-07-12

### Changed
- General improvements

## [2.7.0] - 2022-07-10

### Added
- New features

## [2.5.4] - 2021-03-13

### Changed
- General improvements

## [2.5.3] - 2021-03-13

### Changed
- General improvements

## [2.5.2] - 2021-03-11

### Changed
- General improvements

## [2.5.1] - 2021-03-09

### Changed
- General improvements

## [2.5.0] - 2021-03-09

### Changed
- General improvements

## [2.4.2] - 2021-03-06

### Changed
- General improvements

## [2.4.1.2] - 2021-03-06

### Changed
- General improvements

## [2.4.1.1] - 2021-03-05

### Changed
- General improvements

## [2.4.1] - 2021-03-04

### Changed
- General improvements

## [2.4.0] - 2021-03-04

### Changed
- General improvements

## [2.3.0] - 2021-02-23

### Changed
- General improvements

## [2.2.1] - 2021-02-14

### Changed
- General improvements

## [2.2.0] - 2021-02-13

### Changed
- General improvements

## [2.1.0] - 2021-02-12

### Changed
- General improvements

## [2.0.0.1] - 2021-02-07

### Changed
- General improvements

## [2.0.0] - 2021-01-16

### Changed
- Major version release

## [1.12.2] - 2020-11-10

### Changed
- General improvements

## [1.12.0] - 2020-11-09

### Changed
- General improvements

## [1.11.0] - 2020-10-15

### Added
- Added time scales

## [1.10.3] - 2020-10-15

### Fixed
- Fix rendering issue with time locators

## [1.10.2] - 2020-10-15

### Fixed
- Update tooltip of timeline item when changing time zone

## [1.10.1] - 2020-10-15

### Changed
- Improved mouse-over cursor

## [1.10.0] - 2020-10-14

### Added
- Move start/end locator with drag & drop

## [1.9.2] - 2020-10-12

### Changed
- Improved navigation

## [1.9.1] - 2020-10-12

### Changed
- Improved vertical navigation

## [1.9.0] - 2020-10-11

### Changed
- General improvements

## [1.8.0] - 2020-10-04

### Added
- Added menu for selecting time zone

## [1.7.0] - 2020-08-10

### Changed
- General improvements

## [1.6.1] - 2020-08-09

### Added
- Press "C" key to toggle compact/normal grid

## [1.6.0] - 2020-08-09

### Added
- Toggle compact/normal grid

## [1.5.3] - 2020-07-13

### Changed
- Cleanup

## [1.5.2] - 2020-07-12

### Added
- Select timeline item color with double click

## [1.5.1] - 2020-07-12

### Changed
- Ask to discard changes before opening file

## [1.5.0] - 2020-07-12

### Changed
- General improvements

## [1.4.0] - 2020-07-07

### Changed
- General improvements

## [1.3.0] - 2020-07-06

### Added
- Added OK button to close About box

## [1.2.0] - 2020-07-06

### Fixed
- Fixed bug in locator tooltip

## [1.1.0] - 2020-07-05

### Changed
- General improvements

## [1.0.0] - 2020-06-08

### Added
- Initial version of TimeLiner
