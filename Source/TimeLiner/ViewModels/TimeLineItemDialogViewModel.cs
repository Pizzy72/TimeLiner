// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2026 Christian Pistor

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using TimeLiner.Common;
using TimeLiner.Converter;
using TimeLiner.Properties;
using TimeLiner.UI;

namespace TimeLiner.ViewModels
{
    /// <summary>
    /// The view model of the timeline item dialog.
    /// </summary>
    internal class TimeLineItemDialogViewModel : ViewModelBase, IModalDialogViewModel, IDataErrorInfo
    {
        /// <summary>
        /// Reference to the dialog service.
        /// </summary>
        private readonly IDialogService _dialogService;

        /// <summary>
        /// Reference to the global settings view model.
        /// </summary>
        private readonly SettingsViewModel _settingsViewModel;

        /// <summary>
        /// Reference to the timelines view model.
        /// </summary>
        private readonly TimeLinesViewModel _timeLinesViewModel;

        /// <summary>
        /// Reference to timeline item view model.
        /// </summary>
        private TimeLineItemViewModel _timeLineItem;

        /// <see cref="TimeLineName"/>
        private string _timeLineName;

        /// <see cref="TimeLineItemName"/>
        private string _timeLineItemName;

        /// <see cref="StartTime"/>
        private string _startTimeText;

        /// <see cref="IsTimeSpan"/>
        private bool _isTimeSpan = false;

        /// <see cref="EndTime"/>
        private string _endTimeText;

        /// <see cref="Color"/>
        private Color _color;

        /// <summary>
        /// Dictionary for validation errors.
        /// The key is the property name, the value is the error text.
        /// </summary>
        private readonly Dictionary<string, string> _errors = [];

        /// <summary>
        /// Constructor.
        /// </summary>
        public TimeLineItemDialogViewModel(IDialogService dialogService, SettingsViewModel settingsViewModel, TimeLinesViewModel timeLinesViewModel)
        {
            _dialogService = dialogService;
            _settingsViewModel = settingsViewModel;
            _timeLinesViewModel = timeLinesViewModel;
        }

        /// <summary>
        /// Show edit dialog for given timeline item.
        /// </summary>
        public bool ShowDialog(TimeLineItemViewModel timeLineItem, string title)
        {
            _timeLineItem = timeLineItem;
            Title = title;

            _timeLineName = timeLineItem.TimeLineViewModel.Name;
            _timeLineItemName = timeLineItem.Name;
            _color = timeLineItem.Color;
            _startTimeText = TimeFormat.GetTimeString(timeLineItem.StartTime, _settingsViewModel);
            _endTimeText = TimeFormat.GetTimeString(timeLineItem.EndTime, _settingsViewModel);

            IsTimeSpan = timeLineItem.IsTimeSpan;

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
        /// The start time of the timeline item as text.
        /// </summary>
        public string StartTime
        {
            get => _startTimeText;
            set
            {
                if (_startTimeText != value)
                {
                    _startTimeText = value;

                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(EndTime));
                }
            }
        }

        /// <summary>
        /// Is true if timeline item is a time span; otherwise it is an event.
        /// </summary>
        public bool IsTimeSpan
        {
            get => _isTimeSpan;
            set
            {
                if (_isTimeSpan != value)
                {
                    _isTimeSpan = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Is true if the timeline has a name.
        /// </summary>
        public bool HasTimeLineName => !string.IsNullOrEmpty(_timeLineName);

        /// <summary>
        /// The end time of the timeline item as text.
        /// </summary>
        public string EndTime
        {
            get => _endTimeText;
            set
            {
                if (_endTimeText != value)
                {
                    _endTimeText = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(StartTime));
                }
            }
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

        /// <summary>
        /// The name of the timeline item.
        /// </summary>
        public string TimeLineItemName
        {
            get => _timeLineItemName;
            set
            {
                if (_timeLineItemName != value)
                {
                    _timeLineItemName = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The color of the timeline item.
        /// </summary>
        public Color Color
        {
            get => _color;
            set
            {
                if (_color != value)
                {
                    _color = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The name of the color of the timeline item.
        /// </summary>
        public string ColorName => ColorToNameConverter.Convert(Color);

        /// <inheritdoc cref="IModalDialogViewModel.DialogResult"/>
        public bool? DialogResult
        {
            get; set;
        }

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

                    case nameof(TimeLineItemName):
                        return ValidateTimeLineItemName();

                    case nameof(StartTime):
                        return ValidateStartTime();

                    case nameof(EndTime):
                        return ValidateEndTime();

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

            if (timeLineName != _timeLineItem.TimeLineViewModel.Name && _timeLinesViewModel.TimeLinesModel.TimeLines.Select(t => t.Name).Contains(timeLineName))
            {
                return AddValidationError(nameof(TimeLineName), Resources.ErrorTimelineAlreadyExists);
            }

            return null;
        }

        /// <summary>
        /// Validate the timeline item name.
        /// </summary>
        private string ValidateTimeLineItemName()
        {
            if (string.IsNullOrWhiteSpace(_timeLineItemName))
            {
                return AddValidationError(nameof(TimeLineItemName), "Empty name");
            }

            return null;
        }

        /// <summary>
        /// Validate the start time.
        /// </summary>
        private string ValidateStartTime()
        {
            if (string.IsNullOrWhiteSpace(_startTimeText))
            {
                return AddValidationError(nameof(StartTime), "Empty start time");
            }

            if (TryParseUtcTime(_startTimeText, out DateTime startTimeUtc))
            {
                if (_timeLineItem.StartTime != startTimeUtc && _timeLineItem.TimeLineViewModel.TimeLineItems.Any(tli => tli.StartTime == startTimeUtc))
                {
                    return AddValidationError(nameof(StartTime), "An item with this start time already exists on this timeline");
                }

                if (IsTimeSpan && TryParseUtcTime(_endTimeText, out DateTime endTimeUtc))
                {
                    if (startTimeUtc > endTimeUtc)
                    {
                        return AddValidationError(nameof(StartTime), "Start time > End time");
                    }
                }
            }
            else
            {
                return AddValidationError(nameof(StartTime), "Invalid time");
            }

            return null;
        }

        /// <summary>
        /// Validate the end time.
        /// </summary>
        private string ValidateEndTime()
        {
            if (string.IsNullOrWhiteSpace(_endTimeText))
            {
                return AddValidationError(nameof(EndTime), "Empty end time");
            }

            if (TryParseUtcTime(_endTimeText, out DateTime endTimeUtc))
            {
                if (IsTimeSpan && TryParseUtcTime(_startTimeText, out DateTime startTimeUtc))
                {
                    if (endTimeUtc < startTimeUtc)
                    {
                        return AddValidationError(nameof(EndTime), "End time < Start time");
                    }
                }
            }
            else
            {
                return AddValidationError(nameof(EndTime), "Invalid time");
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
        public async Task CloseDialog(bool dialogResult)
        {
            DialogResult = dialogResult;

            if (dialogResult)
            {
                if (HasValidationErrors())
                {
                    return;
                }

                await _timeLinesViewModel.CaptureUndoAsync();

                _timeLineItem.TimeLineViewModel.Name = _timeLineName.Trim();
                _timeLineItem.Name = _timeLineItemName.Trim();
                _timeLineItem.StartTime = DateTime.Parse(_startTimeText).ToUniversalTime();
                _timeLineItem.EndTime = IsTimeSpan ? DateTime.Parse(_endTimeText).ToUniversalTime() : _timeLineItem.StartTime;
                _timeLineItem.Color = _color;
            }

            _dialogService.Close(this);
        }

        /// <summary>
        /// Try to parse the time from the given string, and convert it to UTC.
        /// </summary>
        private static bool TryParseUtcTime(string s, out DateTime timeUtc)
        {
            timeUtc = default;

            if (DateTime.TryParse(s, out DateTime time))
            {
                timeUtc = time.ToUniversalTime();
                return true;
            }

            return false;
        }
    }
}
