// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Christian Pistor

using System;
using System.Windows.Controls;
using System.Windows.Input;
using TimeLiner.ViewModels;

namespace TimeLiner.Views
{
    public partial class TimeDisplayView : UserControl
    {
        private bool _handlingOffset;

        public TimeDisplayView() => InitializeComponent();

        private void TimeSelection_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.T || Keyboard.Modifiers != ModifierKeys.None
                || sender is not Fluent.ComboBox { IsEditable: false, IsDropDownOpen: false }
                || DataContext is not SettingsViewModel settings)
                return;

            e.Handled = true;
            settings.ToggleTimeFormat();
            Focus();
        }

        private bool CommitOffset()
        {
            if (_handlingOffset || DataContext is not SettingsViewModel settings)
                return false;
            try
            {
                _handlingOffset = true;
                return settings.TryCommitOffsetInput();
            }
            finally { _handlingOffset = false; }
        }

        private void UtcOffsetComboBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                bool committed = CommitOffset();
                _handlingOffset = true;
                try
                {
                    UtcOffsetComboBox.IsDropDownOpen = false;
                    // Leave the editor while keeping keyboard shortcuts routed
                    // through the window. Invalid input stays focused for correction.
                    if (committed)
                        Focus();
                }
                finally { _handlingOffset = false; }
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                // Closing the popup can otherwise commit its current selection.
                _handlingOffset = true;
                try
                {
                    UtcOffsetComboBox.IsDropDownOpen = false;
                    (DataContext as SettingsViewModel)?.CancelOffsetInput();
                }
                finally { _handlingOffset = false; }
                e.Handled = true;
            }
        }

        private void UtcOffsetComboBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (!UtcOffsetComboBox.IsKeyboardFocusWithin)
                CommitOffset();
        }

        private void UtcOffsetComboBox_DropDownClosed(object sender, EventArgs e) => CommitOffset();
    }
}
