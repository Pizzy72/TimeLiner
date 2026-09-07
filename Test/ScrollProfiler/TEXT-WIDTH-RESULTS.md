# Textbreiten-Cache nach 04a63d2

## Entscheidung

Der begrenzte Cache bleibt erhalten: Er reduziert die wiederholte Textmessung,
CPU-Zeit und Allokationen deutlich. Die dichte Scrollstrecke wird im Standardlauf
um 15,3 % kürzer. Eine Verbesserung der p95-Frameabstände ist dagegen **nicht
durchgehend nachgewiesen**. Weitere Umbauten an Kollisionssuche oder Layout wurden
in diesem Schritt nicht vorgenommen. Die Änderungen sind zunächst uncommittet.

## Änderung und Absicherung

Jeder TextBlock hält höchstens seine letzte natürliche Textbreite. Vor einer
Wiederverwendung werden Text, Schriftfamilie, Schriftstil, Gewicht, Streckung,
Schriftgröße, Schreibrichtung, DPI und die aktuelle UI-Kultur verglichen.
Bei veränderlichen Kulturinformationen und anonymen zusammengesetzten
Schriftfamilien wird die Messung nicht wiederverwendet. Es gibt keine globale
Sammlung von Labeltexten oder TextBlock-Referenzen.

180 Regressionstests bestehen. Neue Tests vergleichen ein bereits gemessenes
Textfeld nach Änderungen seiner Eingaben mit einem frischen Textfeld, einschließlich
Kultur- und DPI-Wechseln. Ein GC-Test prüft die Freigabe eines Textfeldes mit Cache.
15 reale WPF-Layoutzustände mit 4627 Elementgeometrien stimmen mit der Baseline
überein, einschließlich Rückwärtsscrollen. Dies ist kein Pixelvergleich.

## Messverfahren

Grundlage ist `04a63d2`, ergänzt ausschließlich um Profiling-Zähler. Beide Builds
enthalten dieselben Zähler für Breitenabfragen, echte FormattedText-Messungen und
deren Dauer. Sie sind durch `SCROLL_PROFILE` begrenzt und fehlen im normalen Build.
Der Messläufer liest sie außerhalb des gemessenen Intervalls; ältere Quellstände
ohne Zähler bleiben unterstützt und melden dafür `null`.

Unveränderter [WPF-Messaufbau](README.md): vollständiges Hauptfenster, 16 Zeilen,
361 Items, drei Zoomstufen, je ein Aufwärmdurchlauf und drei Wiederholungen mit
30 Schritten zu 10 DIPs. .NET 10.0.11, 125 % DPI, Rendering-Tier 2.
Alle Tabellenwerte sind Mediane der drei Wiederholungen.

## Standardlauf

| Zoom | Gesamtzeit vorher → nachher | Prozess-CPU vorher → nachher | UI-Allokationen vorher → nachher |
|---|---:|---:|---:|
| 5 Minuten | 2,304 → 1,950 s | 3,719 → 3,000 s | 158,45 → 135,78 MB |
| 1 Minute | 0,895 → 0,841 s | 1,281 → 1,172 s | 55,76 → 48,45 MB |
| 1 Sekunde | 0,816 → 0,872 s | 0,234 → 0,359 s | 15,74 → 15,05 MB |

| Zoom | Echte Textmessungen vorher → nachher | Textmesszeit vorher → nachher | Kollisions-Callback-Zeit vorher → nachher |
|---|---:|---:|---:|
| 5 Minuten | 6108 → 29 | 429,75 → 5,29 ms | 648,51 → 208,25 ms |
| 1 Minute | 1903 → 2 | 110,00 → 0,58 ms | 160,66 → 32,35 ms |
| 1 Sekunde | 168 → 0 | 11,58 → 0,00 ms | 17,39 → 8,67 ms |

Die Zahl der Breitenabfragen bleibt gleich. Der Cache vermeidet den teuren Teil
der Abfrage. Die Textmesszeit umfasst FormattedText-Konstruktion und Ermittlung der
natürlichen Breite, nicht die Prüfung des Cache-Schlüssels.

| Zoom | WPF-Frameabstand p95 vorher → nachher | LayoutUpdated-Zyklen vorher → nachher |
|---|---:|---:|
| 5 Minuten | 63,18 → 75,36 ms | 87 → 87 |
| 1 Minute | 39,46 → 43,12 ms | 46 → 46 |
| 1 Sekunde | 53,88 → 58,62 ms | 31 → 31 |

Die p95-Werte fallen im Standardlauf höher aus. Weniger Arbeit bedeutet hier
also nicht automatisch bessere Frame-Spitzen. Bei 5 Minuten sinkt zugleich die
Anzahl der Frameabstände über 25 ms von 36 auf 31 pro Strecke. Anzahl und Verteilung
der Rendering-Callbacks ändern sich mit der Laufzeit; die Kennzahlen beschreiben
unterschiedliche Aspekte und dürfen nicht gegeneinander ausgetauscht werden.

## Kontrolllauf ohne gestufte JIT-Kompilierung

Wegen der gemischten Framewerte wurden beide Varianten zusätzlich mit
`DOTNET_TieredCompilation=0` ausgeführt. Diese Einstellung galt ausschließlich für
die jeweiligen Messprozesse und ändert die normalen Anwendungseinstellungen nicht.
Sie ist ein diagnostischer Vergleich, kein Ersatz für den Standardlauf.

| Zoom | Gesamtzeit vorher → nachher | WPF-Frameabstand p95 vorher → nachher |
|---|---:|---:|
| 5 Minuten | 2,913 → 2,270 s | 91,57 → 82,84 ms |
| 1 Minute | 1,150 → 0,959 s | 47,87 → 48,17 ms |
| 1 Sekunde | 0,933 → 0,875 s | 55,02 → 49,68 ms |

Bei 5 Minuten sinkt die Prozess-CPU hier von 3,203 auf 2,672 s und die
Kollisionszeit von 704,18 auf 171,10 ms. Die Einsparung an Rechenarbeit ist damit
reproduzierbar. Die Unterschiede zwischen Standard- und Kontrolllauf erlauben
keine eindeutige Zuschreibung der Frame-Schwankungen an die JIT-Kompilierung.
Für die schwach belastete Sekundenansicht wird kein verlässlicher Zeitgewinn behauptet.

Frameabstände stammen von `CompositionTarget.Rendering`, nicht von GPU-Präsentationen.
p95 ist jeweils der Median der drei einzelnen p95-Werte. MB sind dezimale,
kumulierte Allokationen des UI-Threads, kein gleichzeitig belegter Speicher.

## Rohdaten

Im ignorierten Verzeichnis `artifacts/text-width` liegen `baseline.json`,
`final.json`, `baseline-controlled.json`, `final-controlled.json`, der erste
Cache-Probelauf `candidate.json`, `geometry-baseline.json`, `geometry-final.json`
und `regression.log`. Die JSON-Metadaten enthalten die Hashes der Profiling-Builds.
