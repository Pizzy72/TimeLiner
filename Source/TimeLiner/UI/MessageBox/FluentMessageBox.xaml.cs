// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2025 Christian Pistor

using System;
using System.Windows;
using System.Windows.Input;

namespace TimeLiner.UI.MessageBox
{
    /// <summary>
    /// Represents the result of a FluentMessageBox dialog.
    /// </summary>
    public enum FluentMessageBoxResult
    {
        None,
        OK,
        Yes,
        No,
        Cancel
    }

    /// <summary>
    /// Specifies which buttons are shown in the FluentMessageBox dialog.
    /// </summary>
    public enum FluentMessageBoxButtons
    {
        OK,
        YesNo,
        YesNoCancel
    }

    /// <summary>
    /// A custom message box window with Fluent Ribbon styling.
    /// </summary>
    public partial class FluentMessageBox : BaseRibbonWindow
    {
        /// <summary>
        /// Gets the dialog title.
        /// </summary>
        public string DialogTitle { get; }
        /// <summary>
        /// Gets the message text.
        /// </summary>
        public string MessageText { get; }

        /// <summary>
        /// Gets the visibility of the OK button.
        /// </summary>
        public Visibility OkVisibility { get; } = Visibility.Collapsed;

        /// <summary>
        /// Gets the visibility of the Yes button.
        /// </summary>
        public Visibility YesVisibility { get; } = Visibility.Collapsed;

        /// <summary>
        /// Gets the visibility of the No button.
        /// </summary>
        public Visibility NoVisibility { get; } = Visibility.Collapsed;

        /// <summary>
        /// Gets the visibility of the Cancel button.
        /// </summary>
        public Visibility CancelVisibility { get; } = Visibility.Collapsed;

        /// <summary>
        /// Gets the alignment of the message text.
        /// </summary>
        public TextAlignment MessageTextAlignment { get; }

        /// <summary>
        /// Gets the default result for the dialog.
        /// </summary>
        public FluentMessageBoxResult DefaultResult { get; }

        /// <summary>
        /// Returns true if OK is the default result.
        /// </summary>
        public bool IsDefaultOk => DefaultResult == FluentMessageBoxResult.OK;

        /// <summary>
        /// Returns true if Yes is the default result.
        /// </summary>
        public bool IsDefaultYes => DefaultResult == FluentMessageBoxResult.Yes;

        /// <summary>
        /// Returns true if No is the default result.
        /// </summary>
        public bool IsDefaultNo => DefaultResult == FluentMessageBoxResult.No;

        /// <summary>
        /// Returns true if Cancel is the default result.
        /// </summary>
        public bool IsDefaultCancel => DefaultResult == FluentMessageBoxResult.Cancel;

        /// <summary>
        /// Gets the result of the dialog after closing.
        /// </summary>
        public FluentMessageBoxResult Result { get; private set; } = FluentMessageBoxResult.None;

        /// <summary>
        /// Shows an error message box with the specified message and title.
        /// </summary>
        /// <param name="owner">The owner window.</param>
        /// <param name="message">The error message.</param>
        /// <param name="title">The dialog title (optional).</param>
        public static void ShowError(
            Window owner,
            string message,
            string title = "Error"
            )
        {
            Show(
                owner,
                message,
                title,
                FluentMessageBoxButtons.OK,
                TextAlignment.Left
                );
        }

        /// <summary>
        /// Shows the message box dialog with the specified parameters.
        /// </summary>
        /// <param name="owner">The owner window.</param>
        /// <param name="message">The message text.</param>
        /// <param name="title">The dialog title.</param>
        /// <param name="buttons">The buttons to display.</param>
        /// <param name="textAlignment">The alignment of the message text.</param>
        /// <returns>The result selected by the user.</returns>
        public static FluentMessageBoxResult Show(
            Window owner,
            string message,
            string title,
            FluentMessageBoxButtons buttons,
            TextAlignment textAlignment = TextAlignment.Left
            )
        {
            FluentMessageBox dialog = new(message, title, buttons, textAlignment)
            {
                Owner = owner
            };

            dialog.ShowDialog();

            return dialog.Result;
        }

        /// <summary>
        /// Initializes a new instance of the FluentMessageBox class.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <param name="title">The dialog title.</param>
        /// <param name="buttons">The buttons to display.</param>
        /// <param name="textAlignment">The alignment of the message text.</param>
        private FluentMessageBox(
            string message,
            string title,
            FluentMessageBoxButtons buttons,
            TextAlignment textAlignment
            )
        {
            InitializeComponent();

            DataContext = this;

            MessageText = message;
            DialogTitle = title;
            MessageTextAlignment = textAlignment;

            switch (buttons)
            {
                case FluentMessageBoxButtons.OK:
                    OkVisibility = Visibility.Visible;
                    DefaultResult = FluentMessageBoxResult.OK;
                    break;

                case FluentMessageBoxButtons.YesNo:
                    YesVisibility = Visibility.Visible;
                    NoVisibility = Visibility.Visible;
                    DefaultResult = FluentMessageBoxResult.No;
                    break;

                case FluentMessageBoxButtons.YesNoCancel:
                    YesVisibility = Visibility.Visible;
                    NoVisibility = Visibility.Visible;
                    CancelVisibility = Visibility.Visible;
                    DefaultResult = FluentMessageBoxResult.Cancel;
                    break;
            }
        }

        /// <summary>
        /// Handles the OK button click event.
        /// </summary>
        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            CloseWithResult(FluentMessageBoxResult.OK);
        }

        /// <summary>
        /// Handles the Yes button click event.
        /// </summary>
        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            CloseWithResult(FluentMessageBoxResult.Yes);
        }

        /// <summary>
        /// Handles the No button click event.
        /// </summary>
        private void No_Click(object sender, RoutedEventArgs e)
        {
            CloseWithResult(FluentMessageBoxResult.No);
        }

        /// <summary>
        /// Handles the Cancel button click event.
        /// </summary>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            CloseWithResult(FluentMessageBoxResult.Cancel);
        }

        /// <summary>
        /// Closes the dialog and sets the result.
        /// </summary>
        /// <param name="result">The result to set.</param>
        private void CloseWithResult(FluentMessageBoxResult result)
        {
            Result = result;
            Close();
        }

        /// <summary>
        /// Handles the KeyDown event for the window (e.g., Escape key).
        /// </summary>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Result = DefaultResult;
                Close();
            }
        }

        /// <summary>
        /// Ensures a result is set when the dialog is closed.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnClosed(EventArgs e)
        {
            if (Result == FluentMessageBoxResult.None)
            {
                Result = DefaultResult;
            }

            base.OnClosed(e);
        }
    }
}
