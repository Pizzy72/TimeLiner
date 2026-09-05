// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Christian Pistor

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Threading;
using TimeLiner.Models;
using TimeLiner.ViewModels;
using TimeLinerTest.TestDoubles;

namespace TimeLinerTest
{
    [TestClass]
    public class TestScrollPerformance
    {
        [STATestMethod]
        public void HiddenRow_ReenteringViewport_RefreshesExistingBindings()
        {
            SettingsViewModel settings = new(new SettingsRepositoryStub(new SettingsModel()));
            TimeLinesViewModel model = new(new DialogServiceStub(), settings, new(settings));
            model.TimeLinesVisibleHeight = 30;
            RunOnDispatcher(() => model.LoadAsync(@"TestData\Minutes.csv", 1000));
            model.Scale = ScaleIndex.Second;
            TimeLineItemViewModel item = model.TimeLines[1].TimeLineItems[0];
            Border existingVisual = new()
            {
                DataContext = item
            };
            existingVisual.SetBinding(FrameworkElement.WidthProperty, new Binding("Width"));
            existingVisual.SetBinding(Canvas.LeftProperty, new Binding("Left"));
            double oldLeft = Canvas.GetLeft(existingVisual);

            model.HorizontalScrollOffset = 5500;
            Assert.AreEqual(oldLeft, Canvas.GetLeft(existingVisual));
            Assert.AreNotEqual(item.Left, oldLeft);

            model.VerticalScrollOffset = 30;
            Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
            Assert.AreEqual(item.Left, Canvas.GetLeft(existingVisual));
            Assert.AreEqual(item.Width, existingVisual.Width);
            Assert.IsGreaterThan(0, existingVisual.Width);

            model.VerticalScrollOffset = 0;
            model.HorizontalScrollOffset = 6200;
            model.TimeLinesVisibleHeight = 60;
            Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
            Assert.AreEqual(item.Left, Canvas.GetLeft(existingVisual));
            Assert.AreEqual(item.Width, existingVisual.Width);
        }

        [STATestMethod]
        [TestCategory("Performance")]
        public void ExternalFile_ScrollBenchmark()
        {
            string path = Environment.GetEnvironmentVariable("TIMELINER_BENCHMARK_FILE");
            if (string.IsNullOrEmpty(path))
                Assert.Inconclusive("Set TIMELINER_BENCHMARK_FILE to a local .tli file to run this benchmark.");

            string assemblyPath = Environment.GetEnvironmentVariable("TIMELINER_BENCHMARK_ASSEMBLY");
            (object model, double rowHeight) = CreateBenchmarkModel(assemblyPath);
            Type modelType = model.GetType();
            modelType.GetProperty("TimeLinesVisibleHeight").SetValue(model, 600d);
            Func<string, double, Task> load = modelType.GetMethod("LoadAsync", new[] { typeof(string), typeof(double) })
                .CreateDelegate<Func<string, double, Task>>(model);
            RunOnDispatcher(() => load(path, 1200));
            PropertyInfo scale = modelType.GetProperty("Scale");
            scale.SetValue(model, Enum.Parse(scale.PropertyType, "Second"));
            // Bind setters once so reflection is excluded from the measured scroll loop.
            Action<double> setHorizontal = modelType.GetProperty("HorizontalScrollOffset").SetMethod.CreateDelegate<Action<double>>(model);
            Action<double> setVertical = modelType.GetProperty("VerticalScrollOffset").SetMethod.CreateDelegate<Action<double>>(model);
            object[] timelines = ((IEnumerable)modelType.GetProperty("TimeLines").GetValue(model)).Cast<object>().ToArray();
            string binary = modelType.Assembly.Location;
            Console.WriteLine($"BINARY version={modelType.Assembly.GetName().Version} sha256={Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(binary)))} runtime={Environment.Version}");
            Assert.AreEqual(30d, rowHeight, "Both builds must use normal row height.");

            // A fixed WPF binding/layout harness, not the complete application window.
            ItemsControl rows = new()
            {
                Width = 1200,
                Height = 600,
                DataContext = model
            };
            rows.SetBinding(ItemsControl.ItemsSourceProperty, new Binding("TimeLineCollectionView"));
            rows.ItemTemplate = (DataTemplate)XamlReader.Parse("""
            <DataTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
              <ItemsControl Height="{Binding Height}" ItemsSource="{Binding TimeLineItemCollectionView}">
                <ItemsControl.ItemsPanel><ItemsPanelTemplate><Canvas/></ItemsPanelTemplate></ItemsControl.ItemsPanel>
                <ItemsControl.ItemTemplate><DataTemplate>
                  <Canvas>
                    <Rectangle Canvas.Left="{Binding Left}" Width="{Binding Width}" Height="20" Fill="SteelBlue"/>
                    <TextBlock Canvas.Left="{Binding Left}" Text="{Binding Name}"/>
                  </Canvas>
                </DataTemplate></ItemsControl.ItemTemplate>
              </ItemsControl>
            </DataTemplate>
            """);
            Window window = new()
            {
                Content = rows,
                Width = 1220,
                Height = 640,
                ShowActivated = false,
                ShowInTaskbar = false,
                Left = -10000,
                Top = -10000
            };
            long notifications = 0;
            PropertyChangedEventHandler count = (_, _) => notifications++;
            INotifyPropertyChanged[] items = timelines.SelectMany(x => ((IEnumerable)x.GetType().GetProperty("TimeLineItems").GetValue(x))
                .Cast<INotifyPropertyChanged>()).ToArray();
            foreach (INotifyPropertyChanged item in items)
                item.PropertyChanged += count;
            try
            {
                window.Show();
                Flush(window);
                foreach (bool vertical in new[] { false, true })
                {
                    for (int run = -3; run < 5; run++)
                    {
                        setHorizontal(0);
                        setVertical(0);
                        Flush(window);
                        notifications = 0;
                        long allocated = GC.GetAllocatedBytesForCurrentThread();
                        Stopwatch timer = Stopwatch.StartNew();
                        for (int step = 1; step <= 120; step++)
                        {
                            if (vertical)
                                setVertical((step % 60) * rowHeight);
                            else
                                setHorizontal((step % 60) * 10);
                            Flush(window);
                        }
                        timer.Stop();
                        allocated = GC.GetAllocatedBytesForCurrentThread() - allocated;
                        if (run >= 0)
                            Console.WriteLine($"BENCH {(vertical ? "vertical" : "horizontal")} run={run} ms={timer.Elapsed.TotalMilliseconds:F2} bytes={allocated} notifications={notifications} rows={timelines.Length}");
                    }
                }
            }
            finally
            {
                window.Close();
                foreach (INotifyPropertyChanged item in items)
                    item.PropertyChanged -= count;
            }
        }

        private static (object Model, double RowHeight) CreateBenchmarkModel(string assemblyPath)
        {
            if (string.IsNullOrEmpty(assemblyPath))
            {
                SettingsViewModel settings = new(new SettingsRepositoryStub(new SettingsModel()));
                return (new TimeLinesViewModel(new DialogServiceStub(), settings, new(settings)), settings.TimeLineHeight);
            }

            assemblyPath = Path.GetFullPath(assemblyPath);
            AssemblyLoadContext context = new("InstalledTimeLiner");
            context.Resolving += (_, name) =>
            {
                string dependency = Path.Combine(Path.GetDirectoryName(assemblyPath), name.Name + ".dll");
                return File.Exists(dependency) ? context.LoadFromAssemblyPath(dependency) : null;
            };
            Assembly assembly = context.LoadFromAssemblyPath(assemblyPath);
            Type GetType(string name) => assembly.GetType("TimeLiner." + name, true);
            object repository = DispatchProxy.Create(GetType("Models.ISettingsRepository"), typeof(BenchmarkServiceProxy));
            ((BenchmarkServiceProxy)repository).Settings = Activator.CreateInstance(GetType("Models.SettingsModel"));
            object settingsModel = Activator.CreateInstance(GetType("ViewModels.SettingsViewModel"), repository);
            object scaling = Activator.CreateInstance(GetType("ViewModels.TimeLineScalingViewModel"), settingsModel);
            object dialogs = DispatchProxy.Create(GetType("UI.IDialogService"), typeof(BenchmarkServiceProxy));
            object model = Activator.CreateInstance(GetType("ViewModels.TimeLinesViewModel"), dialogs, settingsModel, scaling);
            return (model, (double)settingsModel.GetType().GetProperty("TimeLineHeight").GetValue(settingsModel));
        }

        // Keep installed-build settings in memory and reject any unexpected dialog calls.
        public class BenchmarkServiceProxy : DispatchProxy
        {
            public object Settings
            {
                get; set;
            }
            protected override object Invoke(MethodInfo targetMethod, object[] args)
            {
                if (targetMethod.Name == "Load" && Settings != null)
                    return Settings;
                if (targetMethod.Name == "Save" && Settings != null)
                {
                    Settings = args[0];
                    return null;
                }
                throw new NotSupportedException($"Unexpected benchmark service call: {targetMethod.Name}");
            }
        }

        private static void Flush(Window window)
        {
            window.UpdateLayout();
            window.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
        }

        private static void RunOnDispatcher(Func<Task> action)
        {
            SynchronizationContext previous = SynchronizationContext.Current;
            DispatcherFrame frame = new();
            Exception failure = null;
            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext());
            async void Run()
            {
                try
                {
                    await action();
                }
                catch (Exception error) { failure = error; }
                finally { frame.Continue = false; }
            }
            try
            {
                Run();
                Dispatcher.PushFrame(frame);
                if (failure != null)
                    throw failure;
            }
            finally { SynchronizationContext.SetSynchronizationContext(previous); }
        }
    }
}
