// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2025 Christian Pistor

using System;
using System.Windows.Input;
using TimeLiner.UI;
using TimeLiner.ViewModels;

namespace TimeLiner.Views
{
    /// <summary>
    /// The find dialog window.
    /// </summary>
    public partial class FindDialog : BaseRibbonWindow
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public FindDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The data context as FindDialogViewModel.
        /// </summary>
        FindDialogViewModel FindDialogViewModel => ((FindDialogViewModel)DataContext);

        /// <summary>
        /// Is called when the find dialog is closing
        /// </summary>
        private void FindDialog_Closed(object sender, EventArgs e)
        {
            FindDialogViewModel.DialogClosed();
        }

        /// <summary>
        /// Is called when a key is pressed.
        /// </summary>
        private void FindDialog_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    CloseDialog();
                    break;

                case Key.Enter:
                    FindNextTimeLineItem();
                    break;
            }
        }

        /// <summary>
        /// Find next matching timeline item.
        /// </summary>
        private void FindNextTimeLineItem()
        {
            FindDialogViewModel.FindNextCommand.Execute(null);
        }

        /// <summary>
        /// Close dialog through view model.
        /// </summary>
        private void CloseDialog()
        {
            FindDialogViewModel.CloseCommand.Execute(null);
        }

    }
}
