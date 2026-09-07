# WPF-Scrollprofil

Dieser optionale Messläufer startet die vollständige Anwendung über `App.Run()`:
Hauptfenster, Ribbon, echte Item-Templates, Zeitskala und Textkollisionserkennung.
Er gehört nicht zum normalen Build oder zur automatischen Testsuite.

## Aufbau

`ScrollProfiler.targets` ergänzt ausschließlich einen Messstartpunkt. Der Anwendungscode
und sämtliche WPF-Ressourcen werden aus dem jeweils untersuchten Stand kompiliert.
Benutzereinstellungen werden beim Start durch Standardwerte und eine Speicherung im
Arbeitsspeicher ersetzt. Die Eingabedatei wird ausschließlich gelesen; ihr Name und
ihre Textinhalte stehen nicht im JSON-Bericht.

Die Messung verwendet ein sichtbares Fenster mit 1280 × 900 DIPs und drei Zoomstufen:
5 Minuten, 1 Minute und 1 Sekunde. Pro Zoomstufe wird rechnerisch eine dichte Stelle
ausgewählt. Auf einen Aufwärmdurchlauf folgen drei Messdurchläufe mit jeweils
30 horizontalen Schritten zu 10 DIPs. Ein DispatcherTimer auf `Input`-Priorität fordert
Schritte im Abstand von 16,67 ms an. Bei ausgelastetem UI-Thread werden sie später
ausgeführt; die tatsächlich benötigte Gesamtdauer wird erfasst.

Erfasst werden Prozess-CPU-Zeit, Allokationen des UI-Threads, Collection-Resets,
Item-Loaded-Ereignisse, globale WPF-LayoutUpdated-Ereignisse und die Laufzeit der
Dispatcher-Callbacks von `TimelineItemTextBehavior`. Die Zuordnung der Callbacks
verwendet das private WPF-Feld `DispatcherOperation._method` und prüft dessen
Vorhandensein beim Start. Ein Runtime-Wechsel kann eine Anpassung erfordern.

Framezeiten sind Abstände verschiedener `CompositionTarget.Rendering`-Callbacks.
Sie messen den WPF-UI-Takt, **keine GPU-Präsentationszeiten**. LayoutUpdated-Zähler
sind keine Zähler einzelner Measure-/Arrange-Aufrufe. Allokationen sind kumulierte
Bytes, kein Speicherhöchststand. Messläufe nacheinander ausführen; währenddessen
keine Builds, Tests oder UI-Automation parallel starten.

## Ausführen

In PowerShell im Repository-Verzeichnis. `TIMELINER_BENCHMARK_FILE` muss auf eine
lokale repräsentative Datei gesetzt sein. Ausgaben gehören in das ignorierte
Verzeichnis `artifacts/scroll-profile`.

```powershell
$root = (Get-Location).Path
$targets = Join-Path $root 'Test/ScrollProfiler/ScrollProfiler.targets'
dotnet build Source/TimeLiner/TimeLiner.csproj -c Release -p:Platform=x64 `
  "-p:CustomAfterMicrosoftCommonTargets=$targets" -p:StartupObject=Program `
  -p:OutputType=Exe -o artifacts/scroll-profile/app-current
$env:TIMELINER_PROFILE_OUTPUT = Join-Path $root 'artifacts/scroll-profile/current.json'
dotnet artifacts/scroll-profile/app-current/TimeLiner.dll
```

Für die Baseline den gewünschten Commit mit `git archive` in ein eigenes Verzeichnis
unter `artifacts` extrahieren. Anschließend dessen `Source/TimeLiner/TimeLiner.csproj`
mit demselben Targets-Pfad und einem separaten Ausgabeverzeichnis bauen.
Den aktiven Checkout dabei nicht zurücksetzen.

Mit `TIMELINER_PROFILE_VERIFY=1` werden statt Laufzeiten geometrische Zustände bei
drei Zoomstufen und je fünf Scrollpositionen einschließlich Rückwärtsscrollen
aufgezeichnet. Der Vergleich umfasst Anker, Hindernisse und automatisch begrenzte
Textfelder mit Position, Sichtbarkeit und Breite, gerundet auf 0,001 DIP.
Für den Vergleich der JSON-Dateien den ersten Metadatensatz auslassen.
Diese Prüfung vergleicht Layoutgeometrie, keine gerasterten Pixel.

```powershell
$env:TIMELINER_PROFILE_VERIFY = '1'
$env:TIMELINER_PROFILE_OUTPUT = Join-Path $root 'artifacts/scroll-profile/geometry-current.json'
dotnet artifacts/scroll-profile/app-current/TimeLiner.dll
Remove-Item Env:TIMELINER_PROFILE_VERIFY
```

Nach Profiling-Builds den normalen Release-Build beziehungsweise die Tests ohne
die zusätzlichen MSBuild-Eigenschaften ausführen. Für normale Anwendungspakete
keine Dateien aus den Profiling-Ausgabeverzeichnissen verwenden.
