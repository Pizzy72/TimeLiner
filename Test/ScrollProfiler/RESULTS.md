# Scrolloptimierung nach 32e4d56

## Ergebnis

Die vollständige WPF-Anwendung benötigt für die gemessenen Scrollstrecken zwischen
64 % und 92 % weniger Zeit. Die dichteste Ansicht ist deutlich schneller, erreicht
aber weiterhin kein gleichmäßig flüssiges Scrollen.

Verglichen wurden die Quellen von Commit `32e4d56` und der uncommittete Arbeitsstand
mit zwei Änderungen:

1. Eine Zeile aktualisiert beim Scrollen weiterhin die Geometrie ihrer betroffenen
   Items. Ihre Collection wird nur zurückgesetzt, wenn sich die sichtbare Menge
   geändert hat. Dadurch bleiben vorhandene Item-Templates meist erhalten.
2. Textanker melden ihren `DependencyPropertyDescriptor`-Listener beim Entladen ab
   und beim erneuten Laden wieder an. Zuvor hielt dieser Listener entfernte Anker
   einschließlich ihrer Views und Bindungen im Speicher.

Die vorhandene Versionsanhebung auf 2.15.4.0 ist eine bereits zuvor bestehende
Änderung im Arbeitsverzeichnis.

## Messverfahren

Messung am 7. September 2026 mit .NET 10.0.11, WPF-Rendering-Tier 2, 125 % DPI,
heller Standarddarstellung und sichtbaren Item-Namen. Alle 16 Zeilen mit insgesamt
361 Items passen vertikal in das Fenster. Der Timeline-Viewport misst
1263,6 × 593,75 DIPs.

Je Zoomstufe: ein Aufwärmdurchlauf, danach drei Wiederholungen mit 30 kontinuierlichen
10-DIP-Schritten. Angefordert werden Schritte alle 16,67 ms auf Input-Priorität.
Die tatsächliche Ausführung verzögert sich unter Last. Beide Stände verwenden
dieselben dichten Startpositionen und jeweils 300 DIPs Scrollstrecke.
Die Werte in den Tabellen sind Mediane der drei Wiederholungen.
Messaufbau und Reproduktion: [README](README.md).

Frühe Diagnoseversuche mit Scrolländerungen direkt im Rendercallback und sehr langen
Durchläufen wurden verworfen. Die folgenden Ergebnisse stammen ausschließlich aus
vollständig abgeschlossenen, eingabegesteuerten Durchläufen.

| Zoomstufe | Gesamtzeit vorher → nachher | Reduktion | WPF-Frameabstand p95 vorher → nachher |
|---|---:|---:|---:|
| 5 Minuten | 33,61 → 5,79 s | 82,8 % | 870,99 → 352,42 ms |
| 1 Minute | 17,49 → 1,31 s | 92,5 % | 455,18 → 79,38 ms |
| 1 Sekunde | 2,37 → 0,86 s | 63,7 % | 88,67 → 52,81 ms |

Frameabstände stammen aus unterschiedlichen `CompositionTarget.Rendering`-Callbacks,
nicht aus GPU-Präsentationsmessungen. Die p95-Spalte zeigt den Median der drei
einzeln berechneten p95-Werte. Gesamtzeiten sind keine FPS-Angabe. Die Basis streut
stark: 22,51–33,64 s bei 5 Minuten und 13,77–46,74 s bei 1 Minute. Nachher liegen
diese Bereiche bei 5,66–6,02 s beziehungsweise 1,28–1,36 s.

| Zoomstufe | Prozess-CPU vorher → nachher | UI-Allokationen vorher → nachher | LayoutUpdated-Zyklen vorher → nachher |
|---|---:|---:|---:|
| 5 Minuten | 34,55 → 6,50 s | 2278,25 → 543,37 MB | 164 → 122 |
| 1 Minute | 17,03 → 1,34 s | 903,36 → 114,13 MB | 364 → 59 |
| 1 Sekunde | 2,41 → 0,33 s | 194,30 → 17,86 MB | 60 → 31 |

MB sind dezimale Megabytes. Die Allokationen summieren alle während des Durchlaufs
auf dem UI-Thread angeforderten Bytes; sie sind kein Maß für den gleichzeitig
belegten Speicher. LayoutUpdated-Ereignisse zählen globale Layoutzyklen, nicht
einzelne Measure-/Arrange-Aufrufe.

| Zoomstufe | Collection-Resets vorher → nachher | Item-Loaded vorher → nachher | Textkollisionen: Callback-Zeit vorher → nachher |
|---|---:|---:|---:|
| 5 Minuten | 480 → 67 | 6588 → 1158 | 631,11 → 443,44 ms |
| 1 Minute | 480 → 25 | 2383 → 172 | 272,86 → 154,34 ms |
| 1 Sekunde | 480 → 2 | 457 → 8 | 46,87 → 20,30 ms |

Ein Zwischenstand mit ausschließlich der Reset-Vermeidung erreichte bereits
6,43 / 1,48 / 0,85 s Gesamtzeit. Der Hauptgewinn stammt damit von erhaltenen Views.
Der zusätzliche Nutzen der Listener-Korrektur ist durch den GC-Lebensdauertest
belegt; ihr isolierter Zeitgewinn lässt sich aus diesen schwankenden Läufen nicht
zuverlässig quantifizieren.

## Regressionen

- 166 Regressionstests erfolgreich, einschließlich Zoom, Sichtbarkeit, bestehender
  Scrolltests und vier neuer Tests für Containererhalt, beide Scrollrichtungen,
  Freigabe entladener Textanker und Wiederanmeldung ihrer Listener.
- Der neue GC-Test scheiterte vor der Listener-Korrektur auch nach drei
  vollständigen GC-Durchläufen und besteht nach der Korrektur.
- 15 reale WPF-Layoutzustände über drei Zoomstufen einschließlich Rückwärtsscrollen
  stimmen mit der Baseline überein: 4627 verglichene Anker-, Hindernis- und
  Textfeld-Geometrien. Verglichen wurden Sichtbarkeit, Position und Breiten auf
  0,001 DIP; es handelt sich nicht um einen Pixelvergleich.

## Verbleibendes Potenzial

Beim Ein- oder Austritt eines Items setzt die Anwendung noch die gesamte betroffene
Zeile zurück. Die 1158 Item-Ladevorgänge und hohen Frame-Spitzen in der dichtesten
Ansicht sprechen dafür, als nächsten Schritt einzelne Collection-Änderungen statt
vollständiger Zeilen-Resets zu untersuchen. Die Textkollisionserkennung ist weiterhin
quadratisch pro Zeile, beansprucht hier aber wesentlich weniger Zeit als der übrige
UI-Aufbau. Eine aufwendigere Suchstruktur wäre deshalb derzeit nicht der erste Ansatz.

Die Messungen betreffen diese drei horizontalen Strecken. Sie belegen weder eine
allgemeine FPS-Garantie noch Verbesserungen für beliebige Dateien oder vertikales
Scrollen. Die Änderungen sind zur Prüfung uncommittet.

## Rohdaten

Lokale, ignorierte Artefakte unter `artifacts/scroll-profile`:

- `baseline-1.json`: vollständige Basismessung, einschließlich einzelner Frameabstände.
- `candidate-1.json`: Zwischenstand mit Reset-Vermeidung.
- `final-1.json`: Abschlussmessung mit beiden Änderungen.
- `geometry-baseline.json`, `geometry-final.json`: Layoutvergleich.
- `regression.log`: Regressionssuite.

Die JSON-Metadaten enthalten jeweils den SHA-256-Hash des Profiling-Builds.
