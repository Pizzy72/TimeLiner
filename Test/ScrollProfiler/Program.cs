// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Christian Pistor
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using TimeLiner;
using TimeLiner.Models;
using TimeLiner.ViewModels;
using TimeLiner.Views;

internal static class Program
{
    private const int Steps = 30;
    private static int ItemLoads;
    private static readonly List<object> Results = new();
    private static readonly FieldInfo CallbackField = typeof(DispatcherOperation)
        .GetField("_method", BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new NotSupportedException("This WPF runtime does not expose the expected dispatcher callback field.");

    [STAThread]
    private static int Main()
    {
        return StartApplication();
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static int StartApplication()
    {
        string file = Environment.GetEnvironmentVariable("TIMELINER_BENCHMARK_FILE");
        string output = Environment.GetEnvironmentVariable("TIMELINER_PROFILE_OUTPUT");
        if (!File.Exists(file) || string.IsNullOrWhiteSpace(output))
        {
            Console.Error.WriteLine("Set TIMELINER_BENCHMARK_FILE and TIMELINER_PROFILE_OUTPUT.");
            return 2;
        }

        // Isolate settings in memory before the real App startup. Do not save over user preferences.
        SettingsViewModel settings = AppServices.Settings;
        typeof(SettingsViewModel).GetField("_repository", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(settings, new MemorySettings());
        typeof(SettingsViewModel).GetField("_settings", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(settings, new SettingsModel());

        App app = new();
        app.InitializeComponent();
        EventManager.RegisterClassHandler(typeof(TimeLineItemView), FrameworkElement.LoadedEvent,
            new RoutedEventHandler((_, _) => ItemLoads++));
        int exitCode = 0;
        app.Startup += (_, _) => app.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(async () =>
        {
            try
            {
                await Profile(app, file);
                File.WriteAllText(output, JsonSerializer.Serialize(Results, new JsonSerializerOptions { WriteIndented = true }));
                Console.WriteLine("Profile saved: " + output);
            }
            catch (Exception ex)
            {
                // File names and model contents are deliberately absent from the report.
                Console.Error.WriteLine(ex.GetType().Name + ": " + ex.StackTrace);
                exitCode = 1;
            }
            finally { app.Shutdown(); }
        }));
        app.Run();
        return exitCode;
    }

    private static async Task Profile(App app, string file)
    {
        MainWindow window = (MainWindow)app.MainWindow;
        window.WindowState = WindowState.Normal;
        window.Left = 20;
        window.Top = 20;
        window.Width = 1280;
        window.Height = 900;
        await Settle();
        TimeLinesViewModel model = AppServices.TimeLines;
        await model.LoadAsync(file, model.TimeLinesVisibleWidth);
        await Settle();
        Console.WriteLine("Main window and data loaded.");
        Results.Add(new
        {
            AssemblySha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(typeof(App).Assembly.Location))),
            AssemblyVersion = typeof(App).Assembly.GetName().Version.ToString(),
            Runtime = Environment.Version.ToString(),
            TieredCompilation = Environment.GetEnvironmentVariable("DOTNET_TieredCompilation") ?? "default",
            Rows = model.TimeLines.Count,
            Items = model.TimeLineItems.Count,
            ViewportWidth = model.TimeLinesVisibleWidth,
            ViewportHeight = model.TimeLinesVisibleHeight,
            Dpi = VisualTreeHelper.GetDpi(window).PixelsPerDip,
            RenderingTier = RenderCapability.Tier >> 16,
            CollisionProbe = CallbackField.Name,
            FrameDefinition = "Unique CompositionTarget.Rendering callbacks; not GPU presentation times"
        });
        if (Environment.GetEnvironmentVariable("TIMELINER_PROFILE_VERIFY") == "1")
        {
            await VerifyGeometry(window, model);
            return;
        }
        foreach (ScaleIndex scale in new[] { ScaleIndex.FiveMinutes, ScaleIndex.OneMinute, ScaleIndex.Second })
        {
            model.Scale = scale;
            await Settle();
            Console.WriteLine($"Preparing {scale}");
            double travel = Math.Min(Steps * 10, model.HorizontalScrollMaximum);
            double start = FindDenseStart(model, travel);
            Console.WriteLine($"Route: start={start:F2}, travel={travel:F2}");
            await Run(window, model, scale, start, travel, -1, Steps);
            for (int run = 0; run < 3; run++)
            {
                object result = await Run(window, model, scale, start, travel, run, Steps);
                Results.Add(result);
                Console.WriteLine($"Completed {scale}, run {run}");
            }
        }
    }

    private static double FindDenseStart(TimeLinesViewModel model, double travel)
    {
        double best = 0;
        int bestCount = -1;
        // Select the same dense route from model geometry for either binary.
        for (int i = 0; i <= 100; i++)
        {
            double offset = Math.Max(0, model.HorizontalScrollMaximum - travel) * i / 100;
            double factor = AppServices.TimeLineScaling.GetScaleValue(model.Scale);
            int count = model.TimeLineItems.Count(item =>
                (item.EndTime - model.TotalStartTime).TotalSeconds * factor >= offset
                && (item.StartTime - model.TotalStartTime).TotalSeconds * factor <= offset + model.TimeLinesVisibleWidth);
            if (count > bestCount) { best = offset; bestCount = count; }
        }
        return best;
    }

    private static async Task<object> Run(MainWindow window, TimeLinesViewModel model,
        ScaleIndex scale, double start, double travel, int run, int steps)
    {
        model.HorizontalScrollOffset = start;
        await Settle();
        Console.WriteLine($"Starting {scale}, run {run}");
        int layouts = 0, resets = 0, collisionCalls = 0, step = 0;
        int loadsBefore = ItemLoads;
        long collisionTicks = 0, lastFrame = 0;
        double setterMs = 0;
        List<double> frames = new(steps);
        Dictionary<DispatcherOperation, long> collisions = new();
        EventHandler layoutHandler = (_, _) => layouts++;
        NotifyCollectionChangedEventHandler collectionHandler = (_, e) => { if (e.Action == NotifyCollectionChangedAction.Reset) resets++; };
        DispatcherHookEventHandler started = (_, e) =>
        {
            if (CallbackField.GetValue(e.Operation) is Delegate callback
                && callback.Method.DeclaringType?.FullName?.Contains(nameof(TimelineItemTextBehavior)) == true)
                collisions[e.Operation] = Stopwatch.GetTimestamp();
        };
        DispatcherHookEventHandler completed = (_, e) =>
        {
            if (collisions.Remove(e.Operation, out long timestamp))
            {
                collisionTicks += Stopwatch.GetTimestamp() - timestamp;
                collisionCalls++;
            }
        };
        window.LayoutUpdated += layoutHandler;
        foreach (TimeLineViewModel row in model.TimeLines) row.TimeLineItemCollectionView.CollectionChanged += collectionHandler;
        DispatcherHooks hooks = window.Dispatcher.Hooks;
        hooks.OperationStarted += started;
        hooks.OperationCompleted += completed;
        TaskCompletionSource done = new();
        TimeSpan lastRendering = TimeSpan.MinValue;
        EventHandler render = (_, e) =>
        {
            TimeSpan rendering = ((RenderingEventArgs)e).RenderingTime;
            if (rendering == lastRendering) return;
            lastRendering = rendering;
            long now = Stopwatch.GetTimestamp();
            if (lastFrame != 0) frames.Add(Stopwatch.GetElapsedTime(lastFrame, now).TotalMilliseconds);
            lastFrame = now;
            if (step == steps) { done.TrySetResult(); return; }
        };
        // Match input-driven scrolling. Changing the item collection inside Rendering
        // causes reentrant layout/automation work and is not a valid input simulation.
        DispatcherTimer input = new(DispatcherPriority.Input) { Interval = TimeSpan.FromMilliseconds(1000d / 60) };
        input.Tick += (_, _) =>
        {
            long before = Stopwatch.GetTimestamp();
            model.HorizontalScrollOffset = start + travel * ++step / steps;
            setterMs += Stopwatch.GetElapsedTime(before).TotalMilliseconds;
            if (step == steps) input.Stop();
        };
        Process process = Process.GetCurrentProcess();
        TimeSpan cpuBefore = process.TotalProcessorTime;
        long allocated = GC.GetAllocatedBytesForCurrentThread();
        Stopwatch duration = Stopwatch.StartNew();
        using System.Threading.Timer watchdog = new(_ =>
        {
            Console.WriteLine($"Progress: steps={step}, frames={frames.Count}, layouts={layouts}, resets={resets}, loads={ItemLoads - loadsBefore}, collisions={collisionCalls}");
            if (duration.Elapsed.TotalSeconds > 60)
            {
                Console.Error.WriteLine("UI watchdog timeout; this run is invalid.");
                Environment.Exit(3);
            }
        }, null, 10000, 10000);
        CompositionTarget.Rendering += render;
        input.Start();
        try { await done.Task.WaitAsync(TimeSpan.FromSeconds(45)); }
        finally
        {
            input.Stop();
            CompositionTarget.Rendering -= render;
            hooks.OperationStarted -= started;
            hooks.OperationCompleted -= completed;
            window.LayoutUpdated -= layoutHandler;
            foreach (TimeLineViewModel row in model.TimeLines) row.TimeLineItemCollectionView.CollectionChanged -= collectionHandler;
        }
        long bytes = GC.GetAllocatedBytesForCurrentThread() - allocated;
        double cpuMs = (process.TotalProcessorTime - cpuBefore).TotalMilliseconds;
        duration.Stop();
        double[] sorted = frames.Order().ToArray();
        return new
        {
            Scale = scale.ToString(), Run = run, Steps = steps, Start = start, Travel = travel,
            DurationMs = duration.Elapsed.TotalMilliseconds, CpuMs = cpuMs, UiAllocatedBytes = bytes,
            FrameP50Ms = Percentile(sorted, .5), FrameP95Ms = Percentile(sorted, .95),
            FrameP99Ms = Percentile(sorted, .99), FrameMaxMs = sorted.Last(),
            FramesOver25Ms = frames.Count(x => x > 25), LayoutCycles = layouts, CollectionResets = resets,
            ItemLoads = ItemLoads - loadsBefore, ScrollSetterMs = setterMs, CollisionCalls = collisionCalls,
            CollisionMs = collisionTicks * 1000d / Stopwatch.Frequency,
            FrameIntervalsMs = frames
        };
    }

    private static double Percentile(double[] values, double percentile) => values[(int)Math.Ceiling(percentile * values.Length) - 1];

    private static async Task VerifyGeometry(MainWindow window, TimeLinesViewModel model)
    {
        Dictionary<TimeLineItemViewModel, int> ids = model.TimeLineItems
            .Select((item, index) => (item, index)).ToDictionary(x => x.item, x => x.index);
        foreach (ScaleIndex scale in new[] { ScaleIndex.FiveMinutes, ScaleIndex.OneMinute, ScaleIndex.Second })
        {
            model.Scale = scale;
            double start = FindDenseStart(model, 300);
            foreach (double delta in new double[] { 0, 10, 20, 300, 10 })
            {
                model.HorizontalScrollOffset = start + delta;
                await Settle();
                await window.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ApplicationIdle);
                List<object> geometry = new();
                foreach (FrameworkElement element in Descendants(window).OfType<FrameworkElement>())
                {
                    if (element.DataContext is not TimeLineItemViewModel item || !ids.TryGetValue(item, out int id)) continue;
                    if (!TimelineItemTextBehavior.GetIsTextAnchor(element)
                        && !TimelineItemTextBehavior.GetIsTextObstacle(element)
                        && !TimelineItemTextBehavior.GetEnableAutoWidth(element)) continue;
                    Point origin = element.TranslatePoint(new Point(), window);
                    geometry.Add(new
                    {
                        Item = id, Kind = element.GetType().Name, Visible = element.IsVisible,
                        X = Math.Round(origin.X, 3), Y = Math.Round(origin.Y, 3),
                        Width = Math.Round(element.ActualWidth, 3), Height = Math.Round(element.ActualHeight, 3),
                        WidthLimit = double.IsNaN(element.Width) ? "Auto" : Math.Round(element.Width, 3).ToString(System.Globalization.CultureInfo.InvariantCulture)
                    });
                }
                Results.Add(new { Scale = scale.ToString(), Offset = model.HorizontalScrollOffset, Geometry = geometry });
            }
        }
    }

    private static IEnumerable<DependencyObject> Descendants(DependencyObject parent)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, i);
            yield return child;
            foreach (DependencyObject descendant in Descendants(child)) yield return descendant;
        }
    }

    private static async Task Settle()
    {
        await Task.Delay(250);
        // Loaded-priority collision work can keep ApplicationIdle from running.
        // Drain through the same priority without requiring the dispatcher to become idle.
        await Dispatcher.CurrentDispatcher.InvokeAsync(() => { }, DispatcherPriority.Loaded);
    }

    private sealed class MemorySettings : ISettingsRepository
    {
        public SettingsModel Load() => new();
        public void Save(SettingsModel settings) { }
    }
}
