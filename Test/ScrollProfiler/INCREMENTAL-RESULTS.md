# Inkrementelle Sichtbarkeit nach c147575

## Ergebnis

Die inkrementelle Aktualisierung verbessert besonders die dichte Ansicht. Dort
sinkt die Gesamtzeit einer Scrollstrecke um 59,4 %, der p95-WPF-Frameabstand um
80,8 %. Beim Scrollen entstehen keine Collection-Resets mehr. Die Sekundenansicht
bleibt zeitlich innerhalb der Messstreuung praktisch unverändert.

Vergleichsbasis ist der saubere Commit `c147575`. Die Änderungen bleiben zur
Prüfung uncommittet; der vorherige Schritt ist in [RESULTS.md](RESULTS.md) dokumentiert.

## Änderungen

- Eine `ObservableCollection` enthält die sichtbaren Items. Beim Verlassen des
  Viewports werden einzelne Items entfernt, beim Eintritt an ihrer Position in
  der Modellreihenfolge eingefügt. Überlebende WPF-Container bleiben erhalten.
  Diese Reihenfolge ist auch für deckungsgleiche Marker relevant.
- Die stabile, dispatchergebundene CollectionView entsteht erst beim ersten
  Zugriff der Ansicht, nicht schon beim Erzeugen des ViewModels.
- Ein Textanker merkt sich seinen Host während seiner geladenen Lebensdauer.
  Beim Entladen wird dessen Textlayout erneut eingeplant und die Host-Referenz
  entfernt. So bekommt ein unverändert stehendes Label nach dem Entfernen seines
  Nachbarn wieder die verfügbare Breite. Die Listener werden weiterhin abgemeldet.

Die Sichtbarkeitsberechnung und die eigentliche Kollisionssuche bleiben unverändert.

## Messung

Vollständige WPF-Anwendung mit echtem Hauptfenster, Ribbon, Zeitskala und Item-Templates.
Unveränderter [Messläufer](README.md), .NET 10.0.11, Rendering-Tier 2, 125 % DPI,
16 Zeilen und 361 Items. Timeline-Viewport: 1263,6 × 593,75 DIPs.

Pro Zoomstufe ein Aufwärmdurchlauf und drei Wiederholungen mit jeweils 30 horizontalen
10-DIP-Schritten. Startpositionen und Scrollstrecken sind zwischen beiden Builds
identisch. Der Input-Timer fordert Schritte alle 16,67 ms an; ein ausgelasteter
Dispatcher führt sie später aus. Die Tabellen zeigen die Mediane von
`baseline.json` und dem abschließenden `final.json`.

| Zoomstufe | Gesamtzeit vorher → nachher | WPF-Frameabstand p95 vorher → nachher | UI-Allokationen vorher → nachher |
|---|---:|---:|---:|
| 5 Minuten | 5,878 → 2,388 s | 337,96 → 64,89 ms | 541,31 → 159,27 MB |
| 1 Minute | 1,317 → 0,889 s | 79,75 → 46,50 ms | 113,95 → 55,73 MB |
| 1 Sekunde | 0,814 → 0,822 s | 54,15 → 54,76 ms | 17,84 → 15,75 MB |

WPF-Frameabstände sind Abstände unterschiedlicher `CompositionTarget.Rendering`-
Callbacks, keine GPU-Präsentationszeiten. Die p95-Spalte enthält den Median der
drei einzeln berechneten p95-Werte. Allokationen sind kumulierte UI-Thread-Bytes
in dezimalen MB, kein Speicherhöchststand.

| Zoomstufe | Collection-Resets vorher → nachher | Item-Loaded vorher → nachher | LayoutUpdated-Zyklen vorher → nachher |
|---|---:|---:|---:|
| 5 Minuten | 67 → 0 | 1158 → 45 | 112 → 87 |
| 1 Minute | 25 → 0 | 172 → 7 | 59 → 46 |
| 1 Sekunde | 2 → 0 | 8 → 2 | 31 → 31 |

| Zoomstufe | Prozess-CPU vorher → nachher | Textkollisionen: Callback-Zeit vorher → nachher |
|---|---:|---:|
| 5 Minuten | 6,641 → 3,797 s | 471,77 → 671,93 ms |
| 1 Minute | 1,438 → 1,234 s | 168,08 → 146,69 ms |
| 1 Sekunde | 0,266 → 0,250 s | 22,88 → 20,45 ms |

Die dichte Ansicht spart vor allem den Neuaufbau von Templates. Ihre gemessene
Kollisionszeit steigt trotz gleichbleibender Callback-Anzahl; dieser Schritt
beschleunigt die Kollisionssuche selbst also nicht. Die Ursache des höheren
Einzelaufwands wurde hier nicht isoliert.

Die Gesamtzeit streut bei 5 Minuten von 5,50–6,00 s vorher und 2,23–2,43 s nachher.
Bei 1 Minute liegen die Bereiche bei 1,24–1,35 s und 0,856–0,917 s. In der
Sekundenansicht überlappen sie: 0,773–0,874 s und 0,810–0,861 s.

Ein zusätzlicher Baseline-Prozess und weitere Prozesse des Zwischenstands dienten
der Kontrolle zunächst leicht höherer CPU-/Framewerte in der Sekundenansicht.
Sie bestätigen dort keinen klaren Laufzeitgewinn. Für diese Ansicht wird weder
eine Beschleunigung noch eine relevante Verschlechterung aus den kleinen
Zeitunterschieden abgeleitet.

## Regressionen

- 168 Tests erfolgreich. Der bisherige Grenzübertrittstest prüft nun gezielte
  Add-/Remove-Ereignisse und den Erhalt der übrigen WPF-Container in beiden
  Scrollrichtungen.
- Zusätzliche Prüfungen für Modellreihenfolge, deckungsgleiche Startzeiten,
  Zoom, Größenänderung, kompaktes Raster, Bearbeitung, Löschen und Undo/Redo.
- Der neue Test für das Entfernen eines Textnachbarn scheiterte vor der
  Host-Aktualisierung und besteht mit ihr. Bestehende GC- und Wiederanmeldetests
  für entladene Anker bestehen ebenfalls.
- 15 reale WPF-Zustände mit insgesamt 4627 Anker-, Hindernis- und Textfeld-Geometrien
  stimmen mit der Basis überein: Sichtbarkeit, Position und Breite auf 0,001 DIP,
  einschließlich Rückwärtsscrollen. Dies ist kein Pixelvergleich.

## Verbleibendes Potenzial

Die dichteste Ansicht erreicht weiterhin keine gleichmäßigen 60 Hz. Nachdem die
Zeilen-Resets entfallen, hat die Textkollisionserkennung relativ mehr Gewicht.
Ein weiterer Schritt sollte deshalb ihren Aufwand einschließlich der wiederholten
natürlichen Textbreitenmessung isoliert untersuchen. Eine andere Suchstruktur
oder ein Textbreiten-Cache wurde in diesem Schritt nicht eingeführt.

## Rohdaten

Lokale, ignorierte Artefakte unter `artifacts/incremental-scroll`:

- `baseline.json`, `final.json`: Hauptvergleich, Einzelmessungen und Frameabstände.
- `baseline-repeat.json`: zusätzlicher Baseline-Prozess.
- `candidate.json`, `candidate-repeat.json`: Messungen vor der verzögerten
  CollectionView-Erzeugung.
- `geometry-baseline.json`, `geometry-final.json`: vollständiger Geometrievergleich.
- `regression.log`: abschließende Regressionssuite.

Die JSON-Metadaten enthalten die SHA-256-Hashes der gemessenen Profiling-Builds.
