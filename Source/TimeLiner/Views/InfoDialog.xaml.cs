// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2025 Christian Pistor

using System;
using System.Windows;
using System.Windows.Input;
using TimeLiner.UI;
using TimeLiner.ViewModels;

namespace TimeLiner.Views
{
    /// <summary>
    /// The info dialog window.
    /// </summary>
    public partial class InfoDialog : BaseRibbonWindow
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public InfoDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Is called when a key is pressed.
        /// </summary>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CloseDialog();
            }
        }

        /// <summary>
        /// Called when the OK button is clicked.
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CloseDialog();
        }

        /// <summary>
        /// Is called when the info dialog is closing.
        /// </summary>
        private void InfoDialog_Closed(object sender, EventArgs e)
        {
            ((InfoDialogViewModel)DataContext).DialogClosed();
        }

        /// <summary>
        /// Close dialog through view model.
        /// </summary>
        private void CloseDialog()
        {
            ((InfoDialogViewModel)DataContext).CloseCommand.Execute(null);
        }
    }
}
