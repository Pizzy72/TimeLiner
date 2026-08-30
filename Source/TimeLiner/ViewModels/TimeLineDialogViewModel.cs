// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2025 Christian Pistor

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TimeLiner.Properties;
using TimeLiner.UI;

namespace TimeLiner.ViewModels
{
    /// <summary>
    /// The view model of the timeline dialog.
    /// </summary>
    internal class TimeLineDialogViewModel : ViewModelBase, IModalDialogViewModel, IDataErrorInfo
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
        /// The view model of the edited timeline.
        /// </summary>
        private TimeLineViewModel _timeLine;

        /// <see cref="TimeLineName"/>
        private string _timeLineName;

        /// <summary>
        /// Dictionary for validation errors.
        /// The key is the property name, the value is the error text.
        /// </summary>
        private Dictionary<string, string> _errors = new();

        /// <summary>
        /// Constructor.
        /// </summary>
        public TimeLineDialogViewModel(IDialogService dialogService, TimeLinesViewModel timeLinesViewModel)
        {
            _dialogService = dialogService;
            _timeLinesViewModel = timeLinesViewModel;
        }

        /// <summary>
        /// Show edit dialog for given timeline item.
        /// </summary>
        public bool ShowDialog(TimeLineViewModel timeLine, string title)
        {
            _timeLine = timeLine;
            _timeLineName = timeLine.Name;
            Title = title;

            bool? result = _dialogService.ShowDialog(_timeLinesViewModel, this);

            return result ?? false;
        }

        /// <summary>
        /// The tile of the dialog window.
        /// </summary>
        public string Title
        {
            get;
            private set;
        }

        /// <summary>
        /// The name of the timeline
        /// </summary>
        public string TimeLineName
        {
            get => _timeLineName;
            set
            {
                if (_timeLineName != value)
                {
                    _timeLineName = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <inheritdoc cref="IModalDialogViewModel.DialogResult"/>
        public bool? DialogResult { get; set; }

        /// <inheritdoc cref="IDataErrorInfo.Error"/>
        public string Error => null;

        /// <inheritdoc cref="IDataErrorInfo.this"/>
        public string this[string propertyName]
        {
            get
            {
                ResetValidationError(propertyName);

                switch (propertyName)
                {
                    case nameof(TimeLineName):
                        return ValidateTimeLineName();

                    default:
                        break;
                }

                return null;
            }
        }

        /// <summary>
        /// Validate the timeline name.
        /// </summary>
        private string ValidateTimeLineName()
        {
            if (string.IsNullOrWhiteSpace(_timeLineName))
            {
                return AddValidationError(nameof(TimeLineName), Resources.ErrorEmptyTimeLineName);
            }

            string timeLineName = _timeLineName.Trim();

            if (timeLineName != _timeLine.Name && _timeLinesViewModel.TimeLinesModel.TimeLines.Select(t => t.Name).Contains(_timeLineName))
            {
                return AddValidationError(nameof(TimeLineName), Resources.ErrorTimelineAlreadyExists);
            }

            return null;
        }

        /// <summary>
        /// Reset validation error for given property.
        /// </summary>
        private void ResetValidationError(string propertyName)
        {
            _errors.Remove(propertyName);
        }

        /// <summary>
        /// Add validation error for given property.
        /// </summary>
        private string AddValidationError(string propertyName, string error)
        {
            _errors[propertyName] = error;
            return error;
        }

        /// <summary>
        /// Check if there are validation errors.
        /// </summary>
        private bool HasValidationErrors()
        {
            return _errors.Count > 0;
        }

        /// <summary>
        /// Close the edit dialog.
        /// </summary>
        public async void CloseDialog(bool dialogResult)
        {
            DialogResult = dialogResult;

            if (dialogResult)
            {
                if (HasValidationErrors())
                {
                    return;
                }

                await _timeLinesViewModel.CaptureUndoAsync();

                _timeLine.Name = _timeLineName.Trim();

            }

            _dialogService.Close(this);
        }
    }
}
