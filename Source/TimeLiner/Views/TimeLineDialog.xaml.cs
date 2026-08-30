// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2025 Christian Pistor

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using TimeLiner.UI;
using TimeLiner.ViewModels;

namespace TimeLiner.Views
{
    /// <summary>
    /// The timeline dialog.
    /// </summary>
    public partial class TimeLineDialog : BaseRibbonWindow
    {
        /// <summary>
        /// The data context as TimeLineDialogViewModel.
        /// </summary>
        private TimeLineDialogViewModel TimeLineDialogViewModel => (TimeLineDialogViewModel)DataContext;

        /// <summary>
        /// Constructor.
        /// </summary>
        public TimeLineDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Is called when the OK button is clicked.
        /// </summary>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            TextBox textBox = Keyboard.FocusedElement as TextBox;

            if (textBox != null)
            {
                BindingExpression bindingExpression = textBox.GetBindingExpression(TextBox.TextProperty);
                bindingExpression?.UpdateSource();
            }

            TimeLineDialogViewModel.CloseDialog(true);
        }

        /// <summary>
        /// Is called when the Cancel button is clicked.
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            TimeLineDialogViewModel.CloseDialog(false);
        }

        /// <summary>
        /// Is called when a key is pressed.
        /// </summary>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    TimeLineDialogViewModel.CloseDialog(false);
                    break;
            }
        }

        /// <summary>
        /// Is called when the window is loaded.
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TextBoxTimeLineName.Focus();
        }
    }
}
