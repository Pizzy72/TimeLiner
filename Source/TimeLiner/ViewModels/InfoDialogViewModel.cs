// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2025 Christian Pistor

using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;
using TimeLiner.Common;
using TimeLiner.UI;

namespace TimeLiner.ViewModels
{
    /// <summary>
    /// The view model of the info dialog.
    /// </summary>
    internal class InfoDialogViewModel : ViewModelBase
    {
        /// <summary>
        /// Reference to the dialog service.
        /// </summary>
        private readonly IDialogService _dialogService;

        /// <summary>
        /// Reference to the timelines view model.
        /// </summary>
        private readonly TimeLinesViewModel _timeLinesViewModel;

        /// <summary>
        /// The version information.
        /// </summary>
        private readonly FileVersionInfo _versionInfo;

        /// <summary>
        /// Is true if the info dialog is visible.
        /// </summary>
        private bool _isVisible = false;

        /// <summary>
        /// The product name.
        /// </summary>
        public string ProductName => _versionInfo.ProductName;

        /// <summary>
        /// The version.
        /// </summary>
        public string Version => _versionInfo.ProductVersion;

        /// <summary>
        /// The copyright string.
        /// </summary>
        public string Copyright => _versionInfo.LegalCopyright;

        /// <summary>
        /// Constructor.
        /// </summary>
        public InfoDialogViewModel(IDialogService dialogService, TimeLinesViewModel timeLinesViewModel)
        {
            _dialogService = dialogService;
            _timeLinesViewModel = timeLinesViewModel;
            _versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetCallingAssembly().Location);
        }

        /// <summary>
        /// Show the info dialog.
        /// </summary>
        public void ShowDialog()
        {
            if (_isVisible)
            {
                _dialogService.Activate(this);
            }
            else
            {
                _dialogService.Show(_timeLinesViewModel, this);
                _isVisible = true;
            }
        }

        /// <summary>
        /// Close the info dialog.
        /// </summary>
        public void CloseDialog()
        {
            _dialogService.Close(this);
        }

        public void DialogClosed()
        {
            _isVisible = false;
        }

        /// <summary>
        /// Is called when the info dialog is closing
        /// </summary>
        private void InfoDialog_Closing(object sender, CancelEventArgs e)
        {
            CloseDialog();
        }

        /// <summary>
        /// Command to close the info dialog.
        /// </summary>
        public ICommand CloseCommand
        {
            get
            {
                return _closeCommand ?? (_closeCommand = new MyActionCommand(_ =>
                {
                    CloseDialog();
                }));
            }
        }

        private ICommand _closeCommand;
    }
}
