// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Christian Pistor

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using TimeLiner.Common;
using TimeLiner.Models;
using TimeLiner.ViewModels;
using TimeLiner.Views;
using TimeLinerTest.TestDoubles;

namespace TimeLinerTest
{
    [TestClass]
    public class TestTimeDisplayView
    {
        [STATestMethod]
        public void ModeSelection_UpdatesVisibleControls_AndExternalSettingsUpdateUi()
        {
            var settings = new SettingsViewModel(new SettingsRepositoryStub(new SettingsModel { IsUniversalTime = false }));
            var view = CreateView(settings);
            var mode = (Fluent.ComboBox)view.FindName("DisplayModeComboBox");
            var zone = (Fluent.ComboBox)view.FindName("TimeZoneComboBox");
            var offset = (Fluent.ComboBox)view.FindName("UtcOffsetComboBox");
            Flush(view);
            Assert.AreEqual(Visibility.Visible, zone.Visibility);
            Assert.AreEqual(Visibility.Collapsed, offset.Visibility);
            mode.SelectedValue = TimeDisplayMode.FixedOffset;
            Flush(view);
            Assert.IsTrue(settings.IsFixedOffset);
            Assert.AreEqual(Visibility.Collapsed, zone.Visibility);
            Assert.AreEqual(Visibility.Visible, offset.Visibility);

            settings.SelectFixedOffset(TimeSpan.FromMinutes(-345));
            Flush(view);
            Assert.AreEqual("-05:45", offset.Text, "Command-line and manual changes must agree with the UI.");
            settings.SetFirstEventOffset(TimeSpan.FromHours(2));
            settings.OffsetInput = settings.FirstEventOffsetLabel;
            settings.TryCommitOffsetInput();
            Flush(view);
            Assert.AreEqual("First event (+02:00)", offset.Text);
        }

        [STATestMethod]
        public void EditableOffset_EnterCommits_EscapeCancels_AndPopupSelectionCommits()
        {
            var settings = new SettingsViewModel(new SettingsRepositoryStub(new SettingsModel()));
            settings.SelectFixedOffset(TimeSpan.FromHours(2));
            var view = CreateView(settings);
            var combo = (Fluent.ComboBox)view.FindName("UtcOffsetComboBox");
            Window window = new()
            {
                Content = view, Width = 430, Height = 170,
                ShowActivated = false, ShowInTaskbar = false, Left = -10000, Top = -10000
            };
            try
            {
                window.Show();
                Flush(view);
                var editor = combo.Template.FindName("PART_EditableTextBox", combo) as TextBox;
                Assert.IsNotNull(editor);
                editor.Text = "+05:45";
                Flush(view);
                Assert.AreEqual("+05:45", settings.OffsetInput);
                Assert.AreEqual(TimeSpan.FromHours(2), settings.UtcOffset);
                Press(combo, Key.Enter);
                Flush(view);
                Assert.AreEqual(TimeSpan.FromMinutes(345), settings.UtcOffset);

                editor.Text = "+15:00";
                Press(combo, Key.Enter);
                Assert.IsTrue(settings.HasOffsetError);
                Assert.AreEqual(TimeSpan.FromMinutes(345), settings.UtcOffset);
                Press(combo, Key.Escape);
                Flush(view);
                Assert.IsFalse(settings.HasOffsetError);
                Assert.AreEqual("+05:45", editor.Text);

                settings.SetFirstEventOffset(TimeSpan.FromHours(1));
                Flush(view);
                combo.IsDropDownOpen = true;
                combo.SelectedItem = settings.FirstEventOffsetLabel;
                combo.IsDropDownOpen = false;
                Flush(view);
                Assert.IsTrue(settings.UsesFirstEventOffset);
                Assert.AreEqual(TimeSpan.FromHours(1), settings.UtcOffset);
                Assert.AreEqual("First event (+01:00)", combo.Text);

                combo.IsDropDownOpen = true;
                combo.SelectedItem = "UTC (+00:00)";
                combo.IsDropDownOpen = false;
                Flush(view);
                Assert.AreEqual(TimeSpan.Zero, settings.UtcOffset);
                Assert.IsFalse(settings.UsesFirstEventOffset);
                Assert.IsFalse(settings.HasOffsetError);
                Assert.AreEqual("+00:00", editor.Text);
            }
            finally { window.Close(); }
        }

        private static KeyEventArgs Press(Fluent.ComboBox combo, Key key)
        {
            KeyEventArgs args = new(Keyboard.PrimaryDevice, PresentationSource.FromVisual(combo), 0, key)
            { RoutedEvent = Keyboard.PreviewKeyDownEvent };
            combo.RaiseEvent(args);
            return args;
        }

        [STATestMethod]
        [DataRow("DisplayModeComboBox")]
        [DataRow("TimeZoneComboBox")]
        public void T_AfterFocusingClosedSelection_TogglesTimeDisplay(string name)
        {
            var settings = new SettingsViewModel(new SettingsRepositoryStub(new SettingsModel()));
            settings.SelectFixedOffset(TimeSpan.FromHours(8));
            settings.DisplayMode = TimeDisplayMode.Regional;
            var view = CreateView(settings);
            var combo = (Fluent.ComboBox)view.FindName(name);
            Window window = new()
            {
                Content = view, Width = 430, Height = 170,
                ShowActivated = false, ShowInTaskbar = false, Left = -10000, Top = -10000
            };
            try
            {
                window.Show();
                Flush(view);
                Keyboard.Focus(combo);
                Assert.IsTrue(combo.IsKeyboardFocusWithin);
                Assert.IsTrue(Press(combo, Key.T).Handled);
                Flush(view);
                Assert.AreEqual(TimeDisplayMode.FixedOffset, settings.DisplayMode);
                Assert.AreEqual(TimeSpan.FromHours(8), settings.UtcOffset);
                Assert.IsFalse(combo.IsKeyboardFocusWithin, "After toggling, timeline shortcuts should receive focus again.");

                settings.DisplayMode = TimeDisplayMode.Regional;
                Flush(view);
                Keyboard.Focus(combo);
                combo.IsDropDownOpen = true;
                Press(combo, Key.T);
                Flush(view);
                Assert.AreEqual(TimeDisplayMode.Regional, settings.DisplayMode,
                    "An open list must retain native keyboard navigation.");
            }
            finally { window.Close(); }
        }

        [STATestMethod]
        public void T_InEditableOffset_DoesNotToggleTimeDisplay()
        {
            var settings = new SettingsViewModel(new SettingsRepositoryStub(new SettingsModel()));
            settings.SelectFixedOffset(TimeSpan.FromHours(8));
            var view = CreateView(settings);
            var combo = (Fluent.ComboBox)view.FindName("UtcOffsetComboBox");
            Window window = new()
            {
                Content = view, Width = 430, Height = 170,
                ShowActivated = false, ShowInTaskbar = false, Left = -10000, Top = -10000
            };
            try
            {
                window.Show();
                Flush(view);
                var editor = (TextBox)combo.Template.FindName("PART_EditableTextBox", combo);
                Keyboard.Focus(editor);
                Assert.IsTrue(combo.IsKeyboardFocusWithin);
                Assert.IsFalse(Press(combo, Key.T).Handled);
                Assert.IsTrue(combo.IsKeyboardFocusWithin);
                Assert.AreEqual(TimeDisplayMode.FixedOffset, settings.DisplayMode);
                Assert.AreEqual(TimeSpan.FromHours(8), settings.UtcOffset);
            }
            finally { window.Close(); }
        }

        [STATestMethod]
        [DataRow("+03:00", true)]
        [DataRow("+15:00", false)]
        public void Enter_EndsEditingOnlyForValidOffset(string entered, bool valid)
        {
            var settings = new SettingsViewModel(new SettingsRepositoryStub(new SettingsModel()));
            settings.SelectFixedOffset(TimeSpan.FromHours(2));
            var view = CreateView(settings);
            var combo = (Fluent.ComboBox)view.FindName("UtcOffsetComboBox");
            Window window = new()
            {
                Content = view, Width = 430, Height = 170,
                ShowActivated = false, ShowInTaskbar = false, Left = -10000, Top = -10000
            };
            try
            {
                window.Show();
                Flush(view);
                var editor = (TextBox)combo.Template.FindName("PART_EditableTextBox", combo);
                Keyboard.Focus(editor);
                Flush(view);
                Assert.IsTrue(combo.IsKeyboardFocusWithin, "The test must begin with keyboard focus in the editor.");
                editor.Text = entered;
                Press(combo, Key.Enter);
                Flush(view);
                Assert.AreEqual(!valid, combo.IsKeyboardFocusWithin,
                    "Enter must leave a valid offset, but keep invalid input focused for correction.");
                Assert.AreEqual(!valid, settings.HasOffsetError);
                Assert.AreEqual(entered, editor.Text);
                if (valid)
                {
                    Assert.AreEqual(TimeSpan.FromHours(3), settings.UtcOffset);
                    Keyboard.Focus(editor);
                    Assert.IsTrue(combo.IsKeyboardFocusWithin, "The offset must remain editable when focused again.");
                }
                else
                    Assert.AreEqual(TimeSpan.FromHours(2), settings.UtcOffset);
            }
            finally { window.Close(); }
        }

        [STATestMethod]
        [DataRow("+02:00", "+03:00", false)]
        [DataRow("+02:00", "+05:45", false)]
        [DataRow("auto", "-05:45", false)]
        [DataRow("+02:00", "+03:00", true)]
        [DataRow("+02:00", "+05:45", true)]
        [DataRow("auto", "-05:45", true)]
        public void TypedOffset_AfterSelectingPreset_RemainsDisplayedAfterCommit(string previous, string entered, bool loseFocus)
        {
            var settings = new SettingsViewModel(new SettingsRepositoryStub(new SettingsModel()));
            settings.SetFirstEventOffset(TimeSpan.FromHours(1));
            settings.SelectFixedOffset(TimeSpan.Zero);
            var view = CreateView(settings);
            var combo = (Fluent.ComboBox)view.FindName("UtcOffsetComboBox");
            var ribbon = new Fluent.Ribbon();
            var tab = new Fluent.RibbonTabItem { Header = "Home" };
            var group = new Fluent.RibbonGroupBox { Header = "Time" };
            group.Items.Add(view);
            tab.Groups.Add(group);
            ribbon.Tabs.Add(tab);
            ribbon.SelectedTabIndex = 0;
            Window window = new()
            {
                Content = ribbon, Width = 550, Height = 210,
                ShowActivated = false, ShowInTaskbar = false, Left = -10000, Top = -10000
            };
            window.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/Fluent;component/Themes/Generic.xaml")
            });
            try
            {
                window.Show();
                Flush(view);
                combo.IsDropDownOpen = true;
                combo.SelectedItem = previous == "auto" ? settings.FirstEventOffsetLabel : previous;
                combo.IsDropDownOpen = false;
                Flush(view);
                var editor = (TextBox)combo.Template.FindName("PART_EditableTextBox", combo);
                editor.Text = entered;
                Flush(view);
                if (loseFocus)
                    combo.RaiseEvent(new KeyboardFocusChangedEventArgs(Keyboard.PrimaryDevice, 0, editor, null)
                    { RoutedEvent = Keyboard.LostKeyboardFocusEvent });
                else
                    Press(combo, Key.Enter);
                Flush(view);
                Assert.IsTrue(UtcOffset.TryParse(entered, out TimeSpan expected));
                Assert.AreEqual(expected, settings.UtcOffset);
                Assert.AreEqual(entered, settings.OffsetInput, "The source text must keep the committed offset.");
                Assert.AreEqual(entered, combo.Text, "The menu must keep the committed offset.");
                Assert.AreEqual(entered, editor.Text, "The visible editor must keep the committed offset.");
                combo.IsDropDownOpen = true;
                combo.IsDropDownOpen = false;
                Flush(view);
                Assert.AreEqual(entered, editor.Text, "Reopening the list must not restore its previous selection.");
                Assert.AreEqual(expected, settings.UtcOffset);

                settings.SetFirstEventOffset(TimeSpan.FromHours(3));
                Flush(view);
                Assert.AreEqual(entered, editor.Text, "A new file must preserve a manually chosen offset.");
                combo.IsDropDownOpen = true;
                combo.SelectedItem = settings.FirstEventOffsetLabel;
                combo.IsDropDownOpen = false;
                Flush(view);
                settings.SetFirstEventOffset(TimeSpan.FromHours(1));
                Flush(view);
                Assert.IsTrue(settings.UsesFirstEventOffset);
                Assert.AreEqual(TimeSpan.FromHours(1), settings.UtcOffset);
                Assert.AreEqual("First event (+01:00)", editor.Text, "Automatic selection must update its resolved label on a new file.");
            }
            finally { window.Close(); }
        }

        private static TimeDisplayView CreateView(SettingsViewModel settings)
        {
            var view = new TimeDisplayView { DataContext = settings };
            view.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/Fluent;component/Themes/Generic.xaml")
            });
            return view;
        }

        private static void Flush(FrameworkElement view)
        {
            view.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
            view.Measure(new Size(400, 150));
            view.Arrange(new Rect(0, 0, 400, 150));
            view.UpdateLayout();
        }
    }
}
