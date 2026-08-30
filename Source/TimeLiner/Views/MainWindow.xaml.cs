// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2026 Christian Pistor

using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using TimeLiner.UI;
using TimeLiner.UI.MessageBox;
using TimeLiner.ViewModels;


namespace TimeLiner.Views
{
    /// <summary>
    /// The main window.
    /// </summary>
    public partial class MainWindow : Fluent.RibbonWindow
    {
        /// <summary>
        /// Reference to the global settings view model.
        /// </summary>
        private readonly SettingsViewModel _settingsViewModel;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainWindow()
        {
            // Suppress non-critical binding errors in the debug output window.
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;

            InitializeComponent();

            // Force tool-tip to stay.
            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(int.MaxValue));

            _settingsViewModel = AppServices.Settings;
        }

        /// <summary>
        /// The data context as TimeLinesViewModel.
        /// </summary>
        private TimeLinesViewModel TimeLinesViewModel => (TimeLinesViewModel)DataContext;

        /// <summary>
        /// Waits for Fluent to create the display-options button before replacing its behavior.
        /// </summary>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(
                DispatcherPriority.ContextIdle,
                new Action(ConfigureDisplayOptionsButton));
        }

        /// <summary>
        /// Replaces Fluent's display-options menu with direct one-click toggling.
        /// </summary>
        private void ConfigureDisplayOptionsButton()
        {
            Fluent.RibbonTabControl ribbonTabControl = RibbonControl.Template.FindName(
                "PART_RibbonTabControl",
                RibbonControl) as Fluent.RibbonTabControl;

            Control displayOptionsButton = ribbonTabControl?.Template.FindName(
                "PART_DisplayOptionsButton",
                ribbonTabControl) as Control;

            if (displayOptionsButton is not null)
            {
                displayOptionsButton.PreviewMouseLeftButtonDown += DisplayOptionsButton_PreviewMouseLeftButtonDown;
            }
        }

        /// <summary>
        /// Toggles the Ribbon without opening the display-options menu.
        /// </summary>
        private void DisplayOptionsButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            RibbonControl.IsMinimized = !RibbonControl.IsMinimized;
            e.Handled = true;
        }

        /// <summary>
        /// Is called on executing the command "File / New".
        /// </summary>
        private async void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!await ContinueAfterSavingChanges())
            {
                return;
            }

            CreateNewViewModel();
        }

        /// <summary>
        /// Is called on executing the command "File / Open".
        /// </summary>
        private async void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!await ContinueAfterSavingChanges())
            {
                return;
            }

            if (PromptOpenFilePath(out string filePath))
            {
                await LoadAsync(filePath);
            }
        }

        /// <summary>
        /// Prompt user to enter file path to open.
        /// </summary>
        private bool PromptOpenFilePath(out string filePath)
        {
            filePath = "";

            string extensions = string.Join(";", SettingsViewModel.FileExtensions.Select(ext => $"*{ext}"));

            OpenFileDialog dialog = new()
            {
                Filter = $"TimeLiner files|{extensions}"
            };

            if (dialog.ShowDialog() != true)
            {
                return false;
            }

            filePath = dialog.FileName;

            return true;
        }

        /// <summary>
        /// Is called on executing the command "File / Save".
        /// </summary>
        private async void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            await SaveAsync();
        }

        /// <summary>
        /// Is called on executing the command "File / Save As".
        /// </summary>
        private async void SaveAsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            await SaveAsAsync();
        }

        /// <summary>
        /// Save view model to loaded or initial file.
        /// </summary>
        private async Task SaveAsync()
        {
            if (TimeLinesViewModel.HasFilePath)
            {
                await TimeLinesViewModel.SaveAsync(TimeLinesViewModel.FilePath);
            }
            else
            {
                await SaveAsAsync();
            }
        }

        /// <summary>
        /// Save view model to other file.
        /// </summary>
        private async Task SaveAsAsync()
        {
            if (PromptSaveFilePath(out string filePath))
            {
                await TimeLinesViewModel.SaveAsync(filePath);
            }
        }

        /// <summary>
        /// Prompt user to enter file path for saving.
        /// </summary>
        private bool PromptSaveFilePath(out string filePath)
        {
            filePath = "";

            string extensions = string.Join(";", SettingsViewModel.FileExtensions.Select(ext => $"*{ext}"));

            SaveFileDialog dialog = new()
            {
                Filter = $"TimeLiner files|{extensions}"
            };

            if (dialog.ShowDialog() != true)
            {
                return false;
            }

            filePath = dialog.FileName;

            return true;
        }

        /// <summary>
        /// Is called to check if the command "File / Save" can execute.
        /// </summary>
        private void SaveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = TimeLinesViewModel.IsModified;
        }

        /// <summary>
        /// Is called to check if the command "File / Save As" can execute.
        /// </summary>
        private void SaveAsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = TimeLinesViewModel.HasTimeLineItems;
        }

        /// <summary>
        /// Create new view model.
        /// </summary>
        private void CreateNewViewModel()
        {
            TimeLinesViewModel.Create(TimeLinesView.TimeLines.ActualWidth);
        }

        /// <summary>
        /// Reload view model from opened file.
        /// </summary>
        private async Task ReloadViewModelAsync()
        {
            if (string.IsNullOrEmpty(TimeLinesViewModel.FilePath))
            {
                return;
            }

            if (!await ContinueAfterSavingChanges())
            {
                return;
            }

            await LoadAsync(TimeLinesViewModel.FilePath);
        }

        /// <summary>
        /// Load view model from given file.
        /// </summary>
        private async Task LoadAsync(string filePath)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                // Ensure we don't attach the handler multiple times
                TimeLinesViewModel.Loaded -= TimeLinesViewModel_Loaded;
                TimeLinesViewModel.Loaded += TimeLinesViewModel_Loaded;

                await TimeLinesViewModel.LoadAsync(filePath, TimeLinesView.TimeLines.ActualWidth);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
            finally
            {
                // If Loaded event does not fire for any reason, ensure cursor is reset.
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }

        /// <summary>
        /// Is called when all timelines have been loaded.
        /// </summary>
        private void TimeLinesViewModel_Loaded(object sender, EventArgs e)
        {
            // Detach handler to avoid multiple subscriptions
            TimeLinesViewModel.Loaded -= TimeLinesViewModel_Loaded;

            Mouse.OverrideCursor = Cursors.Arrow;

            CommandManager.InvalidateRequerySuggested();
        }

        /// <summary>
        /// Is called when a key is pressed on the ribbon control
        /// </summary>
        private void Ribbon_KeyDown(object sender, KeyEventArgs e)
        {
            // Suppress keys on the ribbon which are needed on the timeline grid.
            switch (e.Key)
            {
                case Key.Left:
                case Key.Right:
                    e.Handled = true;
                    break;
            }
        }

        /// <summary>
        /// Is called when the text box is double clicked.
        /// </summary>
        private void TextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ((TextBox)sender)?.SelectAll();
        }

        /// <summary>
        /// Is called when the window is closing.
        /// </summary>
        private async void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WindowSettingsHelper.Capture(this, _settingsViewModel.Settings.MainWindowSettings);

            if (!await ContinueAfterSavingChanges())
            {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Is called when the window has closed.
        /// </summary>
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            // Save global settings
            _settingsViewModel.Save();
        }

        /// <summary>
        /// Is called when a file is dragged over the window.
        /// </summary>
        private void MainWindow_DragOver(object sender, DragEventArgs e)
        {
            string[] fileDropData = e.Data.GetData(DataFormats.FileDrop) as string[];

            if (fileDropData == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            string filePath = fileDropData.First();

            string extension = Path.GetExtension(filePath);
            e.Effects = SettingsViewModel.FileExtensions.Any(ext => string.Equals(ext, extension, StringComparison.OrdinalIgnoreCase))
                ? DragDropEffects.Move
                : DragDropEffects.None;

            e.Handled = true;
        }

        /// <summary>
        /// Is called when a file is dropped onto the window.
        /// </summary>
        private async void MainWindow_Drop(object sender, DragEventArgs e)
        {
            string[] fileDropData = e.Data.GetData(DataFormats.FileDrop) as string[];

            if (fileDropData == null)
            {
                return;
            }

            string filePath = fileDropData.First();

            if (!await ContinueAfterSavingChanges())
            {
                return;
            }

            await LoadAsync(filePath);
        }

        /// <summary>
        /// Is called when a key is pressed in the window.
        /// Implements the keyboard shortcuts.
        /// </summary>
        /// <remarks>
        /// In contrast to WPF input bindings, this approach works always!
        /// </remarks>
        private async void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Source is Fluent.ComboBox)
            {
                // Let ComboBox handle its key events.
                return;
            }

            switch (e.Key)
            {
                case Key.C:
                    if (Keyboard.IsKeyDown(Key.LeftShift))
                        _settingsViewModel.ToggleTimeLineHeight();
                    else
                        _settingsViewModel.ToggleTimeGridWidth();
                    break;
                case Key.F:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        TimeLinesViewModel.FindCommand.Execute(null);
                    break;
                case Key.D:
                    _settingsViewModel.ToggleTheme();
                    break;
                case Key.N:
                    _settingsViewModel.ToogleNameVisibility();
                    break;
                case Key.F5:
                    await ReloadViewModelAsync();
                    break;
                case Key.Home:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        TimeLinesViewModel.ScrollTimeLinesToTopCommand.Execute(null);
                    else
                        TimeLinesViewModel.ScrollTimeLinesToStartCommand.Execute(null);
                    break;
                case Key.End:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        TimeLinesViewModel.ScrollTimeLinesToBottomCommand.Execute(null);
                    else
                        TimeLinesViewModel.ScrollTimeLinesToEndCommand.Execute(null);
                    break;
                case Key.Down:
                    if (Keyboard.IsKeyDown(Key.LeftShift))
                        TimeLinesViewModel.ScrollMultipleTimeLinesUpCommand.Execute(null);
                    else
                        TimeLinesViewModel.ScrollOneTimeLineUpCommand.Execute(null);
                    break;
                case Key.Up:
                    if (Keyboard.IsKeyDown(Key.LeftShift))
                        TimeLinesViewModel.ScrollMultipleTimeLinesDownCommand.Execute(null);
                    else
                        TimeLinesViewModel.ScrollOneTimeLineDownCommand.Execute(null);
                    break;
                case Key.PageDown:
                    TimeLinesViewModel.ScrollTimeLinePageUpCommand.Execute(null);
                    break;
                case Key.PageUp:
                    TimeLinesViewModel.ScrollTimeLinePageDownCommand.Execute(null);
                    break;
                case Key.Left:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        TimeLinesViewModel.GotoPreviousCommand.Execute(null);
                    else
                        TimeLinesViewModel.ScrollTimeLinesLeftCommand.Execute(null);
                    break;
                case Key.Right:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        TimeLinesViewModel.GotoNextCommand.Execute(null);
                    else
                        TimeLinesViewModel.ScrollTimeLinesRightCommand.Execute(null);
                    break;
                case Key.OemPlus:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        TimeLinesViewModel.ZoomInCommand.Execute(null);
                    break;
                case Key.OemMinus:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        TimeLinesViewModel.ZoomOutCommand.Execute(null);
                    break;
                case Key.T:
                    _settingsViewModel.ToggleTimeFormat();
                    break;
                case Key.L:
                    _settingsViewModel.ToggleTimeLocatorLocking();
                    break;
                case Key.Escape:
                    TimeLinesViewModel.DeselectTimeLineItem();
                    break;
                case Key.Z:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        TimeLinesViewModel.UndoCommand.Execute(null);
                    break;
                case Key.Y:
                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
                        TimeLinesViewModel.RedoCommand.Execute(null);
                    break;
            }
        }

        /// <summary>
        /// Handles additional initialization after the window's source is created.
        /// Applies window settings, processes command-line arguments,
        /// and loads a file or creates a new view model as needed.
        /// </summary>
        protected async override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            try
            {
                string[] args = Environment.GetCommandLineArgs();

                _settingsViewModel.IsMinimalUi = HasOptionMinimal(args);

                WindowSettingsHelper.Apply(this, _settingsViewModel.Settings.MainWindowSettings);

                if (HasOptionMaximize(args))
                {
                    WindowState = WindowState.Maximized;
                }

                if (TryGetTimeZoneIdFromArgs(args, out string timeZoneId))
                {
                    _settingsViewModel.TimeZone = timeZoneId;
                    _settingsViewModel.IsUniversalTime = timeZoneId.ToLower() == "utc";
                }

                if (TryGetFilePathFromArgs(args, out string filepath))
                {
                    await LoadAsync(filepath);
                }
                else
                {
                    CreateNewViewModel();
                }
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        /// <summary>
        /// Check if the given command-line arguments contain the option "-minimal".
        /// </summary>
        private static bool HasOptionMinimal(string[] args)
        {
            return args.Contains("-minimal", StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Check if the given command-line arguments contain the option "-maximize".
        /// </summary>
        private static bool HasOptionMaximize(string[] args)
        {
            return args.Contains("-maximize", StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Try to get the file path from the given command-line arguments.
        /// </summary>
        private static bool TryGetFilePathFromArgs(string[] args, out string filePath)
        {
            string value = args.FirstOrDefault(a =>
                SettingsViewModel.FileExtensions.Any(ext => a.EndsWith(ext, StringComparison.OrdinalIgnoreCase)));

            if (string.IsNullOrEmpty(value))
            {
                filePath = "";
                return false;
            }

            filePath = value;
            return true;
        }



        /// <summary>
        /// Gets the timezone id from command-line arguments if present after '-timezone'.
        /// Throws if -timezone is present but no value is given. Returns false if not specified.
        /// </summary>
        private static bool TryGetTimeZoneIdFromArgs(string[] args, out string timeZoneId)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], "-timezone", StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 < args.Length)
                    {
                        string value = args[i + 1];
                        if (string.Equals(value, "local", StringComparison.OrdinalIgnoreCase))
                        {
                            timeZoneId = TimeZoneInfo.Local.Id;
                        }
                        else
                        {
                            try
                            {
                                TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(value);
                                timeZoneId = timeZoneInfo.Id;
                            }
                            catch (TimeZoneNotFoundException)
                            {
                                throw new ArgumentException($"Invalid timezone id: '{value}'");
                            }
                        }
                        return true;
                    }
                    else
                    {
                        throw new ArgumentException("Missing value for -timezone option.");
                    }
                }
            }

            timeZoneId = "";
            return false;
        }


        /// <summary>
        /// Ask user to save unsaved changes.
        /// </summary>
        private async Task<bool> ContinueAfterSavingChanges()
        {
            if (!TimeLinesViewModel.IsModified)
            {
                return true;
            }

            FluentMessageBoxResult result = FluentMessageBox.Show(
                Application.Current.MainWindow,
                "Save changes?",
                "Unsaved Changes",
                FluentMessageBoxButtons.YesNoCancel
                );

            if (result == FluentMessageBoxResult.Yes)
            {
                await SaveAsync();
                return true;
            }

            if (result == FluentMessageBoxResult.No)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Shows the given exception in a message box.
        /// </summary>
        private static void ShowError(Exception ex)
        {
            StringBuilder messageBoxText = new();

            messageBoxText.AppendLine(ex.Message);

            if (ex.InnerException != null)
            {
                messageBoxText.AppendLine(ex.InnerException.Message);
            }

            FluentMessageBox.ShowError(
                Application.Current.MainWindow,
                messageBoxText.ToString()
                );
        }
    }
}
