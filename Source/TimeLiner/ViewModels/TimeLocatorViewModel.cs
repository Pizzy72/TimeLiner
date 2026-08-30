// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2026 Christian Pistor

using System;
using System.ComponentModel;
using System.Text;
using TimeLiner.Common;

namespace TimeLiner.ViewModels
{
    /// <summary>
    /// The time locator view model.
    /// </summary>
    internal class TimeLocatorViewModel : ViewModelBase
    {
        /// <summary>
        /// Reference to the global settings view model.
        /// </summary>
        private readonly SettingsViewModel _settingsViewModel;

        /// <summary>
        /// Reference to the timeline scaling view model.
        /// </summary>
        private readonly TimeLineScalingViewModel _timeLineScalingViewModel;

        /// <summary>
        /// Reference to the timelines view model.
        /// </summary>
        private readonly TimeLinesViewModel _timeLinesViewModel;

        /// <see cref="X"/>
        private double _x;

        /// <summary>
        /// Remembers the last horizontal scroll offset.
        /// </summary>
        private double _horizontalScrollOffset;

        /// <summary>
        /// Remembers the last scale.
        /// </summary>
        private ScaleIndex _scale;

        /// <summary>
        /// Remembers the last timeline grid width.
        /// </summary>
        private double _gridWidth;

        /// <summary>
        /// Constructor.
        /// </summary>
        public TimeLocatorViewModel(TimeLinesViewModel timeLinesViewModel, SettingsViewModel settingsViewModel, TimeLineScalingViewModel timeLineScaling)
        {
            _timeLinesViewModel = timeLinesViewModel;
            _settingsViewModel = settingsViewModel;
            _timeLineScalingViewModel = timeLineScaling;

            _gridWidth = _settingsViewModel.TimeGridWidth;

            PropertyChangedEventManager.AddHandler(_timeLinesViewModel, TimeLinesViewModel_PropertyChanged, "");
            PropertyChangedEventManager.AddHandler(_settingsViewModel, Settings_PropertyChanged, "");
            PropertyChangedEventManager.AddHandler(this, _timeLinesViewModel.TimeLocatorViewModel_PropertyChanged, "");

            _horizontalScrollOffset = _timeLinesViewModel.HorizontalScrollOffset;
            _scale = _timeLinesViewModel.Scale;
        }

        /// <summary>
        /// The X-coordinate of the time locator [pixel].
        /// </summary>
        public double X
        {
            get => _x;
            set
            {
                if (value != _x)
                {
                    _x = value;

                    NotifyPropertyChanged();
                    NotifyDerivedPropertiesChanged();
                }
            }
        }

        /// <summary>
        /// If true, show time locator; otherwise, hide it.
        /// </summary>
        public bool IsVisible => (_x > 0d) && (_x < _timeLinesViewModel.TimeLinesVisibleWidth);

        /// <summary>
        /// The locator time.
        /// </summary>
        public DateTime Time
        {
            get
            {
                double offsetPixels = _timeLinesViewModel.HorizontalScrollOffset + X - _settingsViewModel.TimeLineSpacerLeft;
                double offsetSeconds = _timeLineScalingViewModel.CalculateSeconds(offsetPixels, _timeLinesViewModel.Scale);

                try
                {
                    DateTime time = _timeLinesViewModel.TotalStartTime.AddSeconds(offsetSeconds);
                    return time;
                }
                catch (ArgumentOutOfRangeException)
                {
                    // ignore
                }

                return _timeLinesViewModel.TotalStartTime;
            }
        }

        /// <summary>
        /// The locator time as text.
        /// </summary>
        public string TimeText => TimeFormat.GetTimeString(Time, _settingsViewModel);

        /// <summary>
        /// The locator tool-tip.
        /// </summary>
        public string ToolTip
        {
            get
            {
                StringBuilder text = new();

                text.AppendLine(TimeFormat.GetTimeString(Time, _settingsViewModel));

                text.Append(System.Net.WebUtility.HtmlDecode("&#x394; "));
                text.Append(_timeLinesViewModel.LocatorDelta);

                return text.ToString();
            }
        }

        /// <summary>
        /// Is called when a global setting has changed.
        /// </summary>
        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SettingsViewModel.IsUniversalTime):
                case nameof(SettingsViewModel.TimeZone):
                case nameof(SettingsViewModel.IsTimeLocatorLocked):
                    NotifyDerivedPropertiesChanged();
                    break;

                case nameof(SettingsViewModel.IsCompactTimeGrid):
                    OnChangedGridWidth();
                    break;
            }
        }

        /// <summary>
        /// Is called when a property of the timelines view model has changed.
        /// </summary>
        private void TimeLinesViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(TimeLinesViewModel.LocatorDelta):
                    NotifyPropertyChanged(nameof(ToolTip));
                    break;

                case nameof(TimeLinesViewModel.HorizontalScrollOffset):
                    OnChangedHorizontalScrollOffset();
                    break;

                case nameof(TimeLinesViewModel.Scale):
                    OnChangedScale();
                    break;

                case nameof(TimeLinesViewModel.TimeLinesVisibleWidth):
                    NotifyPropertyChanged(nameof(IsVisible));
                    break;
            }
        }

        /// <summary>
        /// Is called when the grid width has changed.
        /// </summary>
        private void OnChangedGridWidth()
        {
            if (_settingsViewModel.IsTimeLocatorLocked)
            {
                NotifyDerivedPropertiesChanged();
            }
            else
            {
                double gridRatio = _settingsViewModel.TimeGridWidth / _gridWidth;
                _gridWidth = _settingsViewModel.TimeGridWidth;
                X = (X + _timeLinesViewModel.HorizontalScrollOffset) * gridRatio - _timeLinesViewModel.HorizontalScrollOffset;
            }
        }

        /// <summary>
        /// Is called when the horizontal scroll offset has changed.
        /// </summary>
        private void OnChangedHorizontalScrollOffset()
        {
            if (_horizontalScrollOffset != _timeLinesViewModel.HorizontalScrollOffset)
            {
                if (_settingsViewModel.IsTimeLocatorLocked)
                {
                    _horizontalScrollOffset = _timeLinesViewModel.HorizontalScrollOffset;
                    NotifyDerivedPropertiesChanged();
                }
                else
                {
                    double scrollDelta = _horizontalScrollOffset - _timeLinesViewModel.HorizontalScrollOffset;
                    _horizontalScrollOffset = _timeLinesViewModel.HorizontalScrollOffset;
                    X += scrollDelta;
                }
            }
        }

        /// <summary>
        /// Is called when the scale has changed.
        /// </summary>
        private void OnChangedScale()
        {
            if (_scale != _timeLinesViewModel.Scale)
            {
                if (_settingsViewModel.IsTimeLocatorLocked)
                {
                    _scale = _timeLinesViewModel.Scale;
                    NotifyDerivedPropertiesChanged();
                }
                else
                {
                    double x = X - _settingsViewModel.TimeLineSpacerLeft + _timeLinesViewModel.HorizontalScrollOffset;
                    double scaleFactor = _timeLineScalingViewModel.GetScaleValue(_timeLinesViewModel.Scale) / _timeLineScalingViewModel.GetScaleValue(_scale);
                    _scale = _timeLinesViewModel.Scale;
                    X = x * scaleFactor + _settingsViewModel.TimeLineSpacerLeft - _timeLinesViewModel.HorizontalScrollOffset;
                }
            }
        }

        /// <summary>
        /// Notify about derived properties which have changed.
        /// </summary>
        private void NotifyDerivedPropertiesChanged()
        {
            NotifyPropertyChanged(nameof(Time));
            NotifyPropertyChanged(nameof(TimeText));
            NotifyPropertyChanged(nameof(ToolTip));
            NotifyPropertyChanged(nameof(IsVisible));
        }
    }
}
