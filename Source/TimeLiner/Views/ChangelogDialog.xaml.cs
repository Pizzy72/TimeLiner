// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Christian Pistor

using System;
using System.Windows;
using System.Windows.Input;
using TimeLiner.UI;
using TimeLiner.ViewModels;

namespace TimeLiner.Views
{
    public partial class ChangelogDialog : BaseRibbonWindow
    {
        public ChangelogDialog() => InitializeComponent();

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Keep the caption left aligned while making the entire title bar draggable.
            TitleBar.HorizontalAlignment = HorizontalAlignment.Stretch;
            TitleBar.HeaderAlignment = HorizontalAlignment.Left;
            TitleBar.PreviewMouseLeftButtonDown += TitleBar_MouseLeftButtonDown;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1 && e.LeftButton == MouseButtonState.Pressed)
            {
                e.Handled = true;
                DragMove();
            }
        }

        private void Dialog_Closed(object sender, EventArgs e) =>
            ((ChangelogDialogViewModel)DataContext).DialogClosed();

        private void Dialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                ((ChangelogDialogViewModel)DataContext).CloseCommand.Execute(null);
                e.Handled = true;
            }
        }
    }
}