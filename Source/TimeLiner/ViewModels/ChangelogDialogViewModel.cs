// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Christian Pistor

using System;
using System.IO;
using System.Windows.Input;
using TimeLiner.Common;
using TimeLiner.UI;

namespace TimeLiner.ViewModels
{
    internal sealed class ChangelogDialogViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        private bool _isVisible;

        public string Text { get; }
        public ICommand CloseCommand { get; }

        public ChangelogDialogViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            try
            {
                Text = ChangelogText.Load();
            }
            catch (Exception exception) when (exception is IOException || exception is InvalidOperationException)
            {
                Text = "The changelog could not be loaded.";
            }
            CloseCommand = new MyActionCommand(_ => _dialogService.Close(this));
        }

        public void ShowDialog(object owner)
        {
            if (_isVisible)
                _dialogService.Activate(this);
            else
            {
                _dialogService.Show(owner, this);
                _isVisible = true;
            }
        }

        public void DialogClosed() => _isVisible = false;
    }
}