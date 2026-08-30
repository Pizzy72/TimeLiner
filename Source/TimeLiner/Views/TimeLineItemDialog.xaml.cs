// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2026 Christian Pistor

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using TimeLiner.UI;
using TimeLiner.ViewModels;

namespace TimeLiner.Views
{
    /// <summary>
    /// The timeline item dialog.
    /// </summary>
    public partial class TimeLineItemDialog : BaseRibbonWindow
    {
        /// <summary>
        /// The data context as FindDialogViewModel.
        /// </summary>
        private TimeLineItemDialogViewModel TimeLineItemDialogViewModel => (TimeLineItemDialogViewModel)DataContext;

        /// <summary>
        /// Constructor.
        /// </summary>
        public TimeLineItemDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Is called when a key is pressed.
        /// </summary>
        private async void Window_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                switch (e.Key)
                {
                    case Key.Escape:
                        await TimeLineItemDialogViewModel.CloseDialog(false);
                        break;
                }
            }
            catch
            {
                // ignore
            }
        }

        /// <summary>
        /// Is called when the OK button is clicked.
        /// </summary>
        private async void OkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Keyboard.FocusedElement is TextBox textBox)
                {
                    BindingExpression bindingExpression = textBox.GetBindingExpression(TextBox.TextProperty);
                    bindingExpression?.UpdateSource();
                }

                await TimeLineItemDialogViewModel.CloseDialog(true);
            }
            catch
            {
                // ignore
            }
        }

        /// <summary>
        /// Is called when the Cancel button is clicked.
        /// </summary>
        private async void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await TimeLineItemDialogViewModel.CloseDialog(false);
            }
            catch
            {
                // ignore
            }
        }

        /// <summary>
        /// Is called when the color button is clicked.
        /// </summary>
        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            ColorDialog colorDialog = new()
            {
                Owner = Application.Current.MainWindow,
                SelectedColor = TimeLineItemDialogViewModel.Color
            };

            bool? result = colorDialog.ShowDialog();

            if (result.HasValue && result.Value)
            {
                TimeLineItemDialogViewModel.Color = colorDialog.SelectedColor;
            }
        }
    }
}
