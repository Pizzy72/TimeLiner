// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Christian Pistor

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using TimeLiner.ViewModels;
using TimeLiner.Views;

namespace TimeLiner.UI
{
    /// <summary>
    /// Creates and controls the WPF windows associated with dialog view models.
    /// </summary>
    internal sealed class DialogService : IDialogService
    {
        private readonly Dictionary<object, Window> _windows = [];

        public void Show(object ownerViewModel, object viewModel)
        {
            Window window = CreateWindow(ownerViewModel, viewModel);
            window.Show();
        }

        public bool? ShowDialog(object ownerViewModel, object viewModel)
        {
            Window window = CreateWindow(ownerViewModel, viewModel);
            return window.ShowDialog();
        }

        public void Activate(object viewModel)
        {
            if (_windows.TryGetValue(viewModel, out Window window))
            {
                window.Activate();
            }
        }

        public void Close(object viewModel)
        {
            if (!_windows.TryGetValue(viewModel, out Window window))
            {
                return;
            }

            if (viewModel is IModalDialogViewModel modalDialog && modalDialog.DialogResult.HasValue)
            {
                window.DialogResult = modalDialog.DialogResult;
            }
            else
            {
                window.Close();
            }
        }

        private Window CreateWindow(object ownerViewModel, object viewModel)
        {
            if (_windows.ContainsKey(viewModel))
            {
                throw new InvalidOperationException("A dialog is already open for this view model.");
            }

            Window window = viewModel switch
            {
                FindDialogViewModel => new FindDialog(),
                InfoDialogViewModel => new InfoDialog(),
                TimeLineDialogViewModel => new TimeLineDialog(),
                TimeLineItemDialogViewModel => new TimeLineItemDialog(),
                _ => throw new ArgumentException("No dialog is registered for this view model.", nameof(viewModel))
            };

            window.DataContext = viewModel;
            window.Owner = FindOwner(ownerViewModel);
            window.Closed += (_, _) => _windows.Remove(viewModel);
            _windows.Add(viewModel, window);

            return window;
        }

        private static Window FindOwner(object ownerViewModel)
        {
            return Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(window => ReferenceEquals(window.DataContext, ownerViewModel))
                ?? Application.Current.MainWindow;
        }
    }
}
