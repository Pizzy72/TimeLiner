# Asset Provenance Checklist

This checklist covers visual and binary documentation assets relevant to a
public release of TimeLiner. On 2026-08-30, the TimeLiner author expressly
confirmed the self-created provenance recorded below.

Allowed status values:

- `CONFIRMED_SELF_CREATED`
- `CONFIRMED_THIRD_PARTY`
- `NEEDS_CONFIRMATION`

## Application icon and ribbon images

The author confirmed that all application icons were self-created using
Affinity Designer.

| File | Use | Known origin | Status |
|---|---|---|---|
| `Source/TimeLiner/Images/Application.ico` | Executable and main-window icon | Self-created by the TimeLiner author using Affinity Designer | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Images/Find-16px.png` | Small Find ribbon icon | Self-created by the TimeLiner author using Affinity Designer | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Images/Find-32px.png` | Large Find ribbon icon | Self-created by the TimeLiner author using Affinity Designer | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Images/GotoBegin-16px.png` | Small go-to-begin ribbon icon | Self-created by the TimeLiner author using Affinity Designer | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Images/GotoBegin-32px.png` | Large go-to-begin ribbon icon | Self-created by the TimeLiner author using Affinity Designer | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Images/GotoEnd-16px.png` | Small go-to-end ribbon icon | Self-created by the TimeLiner author using Affinity Designer | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Images/GotoEnd-32px.png` | Large go-to-end ribbon icon | Self-created by the TimeLiner author using Affinity Designer | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Images/GotoNext-16px.png` | Small next-event ribbon icon | Self-created by the TimeLiner author using Affinity Designer | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Images/GotoNext-32px.png` | Large next-event ribbon icon | Self-created by the TimeLiner author using Affinity Designer | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Images/GotoPrevious-16px.png` | Small previous-event ribbon icon | Self-created by the TimeLiner author using Affinity Designer | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Images/GotoPrevious-32px.png` | Large previous-event ribbon icon | Self-created by the TimeLiner author using Affinity Designer | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Images/Help-16px.png` | Small Help ribbon icon | Self-created by the TimeLiner author using Affinity Designer | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Images/Help-32px.png` | Large Help ribbon icon | Self-created by the TimeLiner author using Affinity Designer | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Images/Info-16px.png` | Small About/Info ribbon icon | Self-created by the TimeLiner author using Affinity Designer | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Images/Info-32px.png` | Large About/Info ribbon icon | Self-created by the TimeLiner author using Affinity Designer | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Images/New-16px.png` | Small New ribbon icon | Self-created by the TimeLiner author using Affinity Designer | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Images/New-32px.png` | Large New ribbon icon | Self-created by the TimeLiner author using Affinity Designer | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Images/Open-16px.png` | Small Open ribbon icon | Self-created by the TimeLiner author using Affinity Designer | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Images/Open-32px.png` | Large Open ribbon icon | Self-created by the TimeLiner author using Affinity Designer | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Images/ResetEndLocator-16px.png` | Small reset-end-locator ribbon icon | Self-created by the TimeLiner author using Affinity Designer | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Images/ResetEndLocator-32px.png` | Large reset-end-locator ribbon icon | Self-created by the TimeLiner author using Affinity Designer | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Images/ResetStartLocator-16px.png` | Small reset-start-locator ribbon icon | Self-created by the TimeLiner author using Affinity Designer | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Images/ResetStartLocator-32px.png` | Large reset-start-locator ribbon icon | Self-created by the TimeLiner author using Affinity Designer | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Images/Save-16px.png` | Small Save/Save As ribbon icon | Self-created by the TimeLiner author using Affinity Designer | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Images/Save-32px.png` | Large Save/Save As ribbon icon | Self-created by the TimeLiner author using Affinity Designer | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Images/ZoomIn-16px.png` | Small zoom-in ribbon icon | Self-created by the TimeLiner author using Affinity Designer | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Images/ZoomIn-32px.png` | Large zoom-in ribbon icon | Self-created by the TimeLiner author using Affinity Designer | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Images/ZoomOut-16px.png` | Small zoom-out ribbon icon | Self-created by the TimeLiner author using Affinity Designer | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Images/ZoomOut-32px.png` | Large zoom-out ribbon icon | Self-created by the TimeLiner author using Affinity Designer | CONFIRMED_SELF_CREATED |

The editable sources for these icons are retained separately from the build
resources. The following entry represents 16 individual Affinity Designer files.

| File | Use | Known origin | Status |
|---|---|---|---|
| `Design/Icons/Sources/*.afdesign` (16 files) | Editable sources for the application and ribbon icons | Self-created by the TimeLiner author using Affinity Designer | CONFIRMED_SELF_CREATED |

## Repository documentation images and diagrams

| File | Use | Known origin | Status |
|---|---|---|---|
| `doc/images/TimeLinerIcon.png` | Application icon displayed in the README | Self-created by the TimeLiner author using Affinity Designer | CONFIRMED_SELF_CREATED |
| `doc/images/TimeLinerScreenshot.png` | Main README application screenshot | Self-created TimeLiner screenshot | CONFIRMED_SELF_CREATED |
| `doc/images/data-model.png` | Rendered data-model diagram | Self-created diagram | CONFIRMED_SELF_CREATED |
| `doc/TimeLiner.drawio` | Editable source for the data-model diagram | Self-created diagram source | CONFIRMED_SELF_CREATED |

## End-user help assets

The author confirmed that the manual and all screenshots, example graphics, and
diagrams contained in it were self-created. The Affinity source `Screenshots.af`
is not present in the current public source tree and is recorded separately below.

| File | Use | Known origin | Status |
|---|---|---|---|
| `Source/TimeLiner/Help/TimeLinerHelp.odt` | Editable source of the end-user manual; contains 16 embedded images | Self-created documentation containing self-created screenshots and diagrams | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Help/TimeLinerHelp.pdf` | Exported end-user manual | Self-created documentation containing self-created screenshots and diagrams | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Help/Sketch.drawio` | Editable source for a help diagram | Self-created diagram source | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Help/TimeLinerHelp.odt!/Pictures/100000000000015C00000078A218BFF6.png` | Image embedded in the end-user manual | Self-created screenshot, example graphic, or diagram | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Help/TimeLinerHelp.odt!/Pictures/100004C80000017100000171641E0545.svg` | Vector image embedded in the end-user manual | Self-created screenshot, example graphic, or diagram | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Help/TimeLinerHelp.odt!/Pictures/10000001000004E20000015DAC6E1A18.png` | Image embedded in the end-user manual | Self-created screenshot, example graphic, or diagram | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Help/TimeLinerHelp.odt!/Pictures/10000000000005B80000009448B784F4.png` | Image embedded in the end-user manual | Self-created screenshot, example graphic, or diagram | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Help/TimeLinerHelp.odt!/Pictures/10000000000005D7000002E0F6E33FFA.png` | Image embedded in the end-user manual | Self-created screenshot, example graphic, or diagram | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Help/TimeLinerHelp.odt!/Pictures/10000000000004B5000001796E0205E1.png` | Image embedded in the end-user manual | Self-created screenshot, example graphic, or diagram | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Help/TimeLinerHelp.odt!/Pictures/100000000000014B0000009D8E97D9E9.png` | Image embedded in the end-user manual | Self-created screenshot, example graphic, or diagram | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Help/TimeLinerHelp.odt!/Pictures/100000000000012B0000008794D4AE9F.png` | Image embedded in the end-user manual | Self-created screenshot, example graphic, or diagram | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Help/TimeLinerHelp.odt!/Pictures/1000000000000188000000AA7AFE8AEA.png` | Image embedded in the end-user manual | Self-created screenshot, example graphic, or diagram | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Help/TimeLinerHelp.odt!/Pictures/10000000000003B6000001A90BCE539B.png` | Image embedded in the end-user manual | Self-created screenshot, example graphic, or diagram | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Help/TimeLinerHelp.odt!/Pictures/10000000000004070000007B52B05E37.png` | Image embedded in the end-user manual | Self-created screenshot, example graphic, or diagram | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Help/TimeLinerHelp.odt!/Pictures/10000000000002DE00000146E1D3435A.png` | Image embedded in the end-user manual | Self-created screenshot, example graphic, or diagram | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Help/TimeLinerHelp.odt!/Pictures/100000000000026A000001094DE48A96.png` | Image embedded in the end-user manual | Self-created screenshot, example graphic, or diagram | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Help/TimeLinerHelp.odt!/Pictures/1000000000000236000000AF66D1E673.png` | Image embedded in the end-user manual | Self-created screenshot, example graphic, or diagram | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Help/TimeLinerHelp.odt!/Pictures/1000000000000411000001113916AF44.png` | Image embedded in the end-user manual | Self-created screenshot, example graphic, or diagram | CONFIRMED_SELF_CREATED |
| `Source/TimeLiner/Help/TimeLinerHelp.odt!/Pictures/10000000000002E0000000B08CAC13E7.png` | Image embedded in the end-user manual | Self-created screenshot, example graphic, or diagram | CONFIRMED_SELF_CREATED |

## Referenced but absent source asset

| File | Use | Known origin | Status |
|---|---|---|---|
| `Source/TimeLiner/Help/Screenshots.af` (not present in the current public source tree) | Working/source file used to create or edit help screenshots | Self-created working/source file; not required for publication | CONFIRMED_SELF_CREATED |

## Status summary

- `CONFIRMED_SELF_CREATED`: 69
- `CONFIRMED_THIRD_PARTY`: 0
- `NEEDS_CONFIRMATION`: 0
