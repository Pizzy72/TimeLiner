// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2026 Christian Pistor

using System;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using TimeLiner.Common;
using TimeLiner.Converter;
using TimeLiner.Models;

namespace TimeLiner.ViewModels
{
    /// <summary>
    /// The view model of a timeline item.
    /// </summary>
    internal class TimeLineItemViewModel : ViewModelBase, IDisposable
    {
        /// <summary>
        /// Reference to the timeline scaling view model.
        /// </summary>
        private readonly TimeLineScalingViewModel _timeLineScalingViewModel;

        /// <summary>
        /// Reference to the global settings view model.
        /// </summary>
        private readonly SettingsViewModel _settingsViewModel;

        /// <summary>
        /// The model of the timeline item.
        /// </summary>
        public TimeLineItemModel TimeLineItemModel { get; internal set; }

        /// <summary>
        /// The view model of the timeline to which this timeline item belongs.
        /// </summary>
        public TimeLineViewModel TimeLineViewModel { get; set; }

        /// <see cref="IsSelected"/>
        private bool _isSelected;

        /// <summary>
        /// The font size of the timeline item text.
        /// </summary>
        public double FontSize => 11d;

        /// <summary>
        /// The vertical offset of the timeline item text [pixel].
        /// </summary>
        public double TextOffsetY => (_settingsViewModel.TimeLineHeight - FontSize) / 2d - 3d;

        /// <summary>
        /// The horizontal offset of a time event [pixel].
        /// </summary>
        public double TimeEventOffsetX => _settingsViewModel.TimeLineHeight / -2d;

        /// <summary>
        /// The width of a time event [pixel].
        /// </summary>
        public double TimeEventWidth => _settingsViewModel.TimeLineHeight / Math.Sqrt(2);

        /// <summary>
        /// The height of a time event [pixel].
        /// </summary>
        public double TimeEventHeight => _settingsViewModel.TimeLineHeight / Math.Sqrt(2);

        /// <summary>
        /// The horizontal offset of a time event text [pixel].
        /// </summary>
        public double TimeEventTextOffsetX => _settingsViewModel.TimeLineHeight / 2d;

        /// <summary>
        /// The height of a time span [pixel].
        /// </summary>
        public double TimeSpanHeight => _settingsViewModel.TimeLineHeight;

        /// <see cref="IsModified"/>
        private bool _isModified;

        /// <see cref="Color"/>
        private Color _color;

        /// <see cref="DeleteTimeLineItemCommand"/>
        private ICommand _deleteTimeLineItemCommand;

        /// <see cref="EditTimeLineItemCommand"/>
        private ICommand _editTimeLineItemCommand;

        /// <summary>
        /// Is true if instance is disposed.
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// Constructor.
        /// </summary>
        public TimeLineItemViewModel(TimeLineItemModel model, TimeLineViewModel timeLineViewModel, SettingsViewModel settingsViewModel, TimeLineScalingViewModel timeLineScaling)
        {
            TimeLineItemModel = model;
            TimeLineViewModel = timeLineViewModel;
            _timeLineScalingViewModel = timeLineScaling;
            _settingsViewModel = settingsViewModel;

            // Listen for property changes to update UI as needed
            PropertyChangedEventManager.AddHandler(_settingsViewModel, Settings_PropertyChanged, "");
            PropertyChangedEventManager.AddHandler(TimeLineViewModel.TimeLinesViewModel, TimeLinesViewModel_PropertyChanged, "");
            PropertyChangedEventManager.AddHandler(this, TimeLineViewModel.TimeLinesViewModel.TimeLineItemViewModel_PropertyChanged, "");

            SetColorFromName(TimeLineItemModel.Color);
        }

        /// <summary>
        /// Is true if the timeline item is modified.
        /// </summary>
        public bool IsModified
        {
            get => _isModified;
            set
            {
                if (value != _isModified)
                {
                    _isModified = value;
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
                if (value != _color)
                {
                    _color = value;

                    TimeLineItemModel.Color = ColorToNameConverter.Convert(value);

                    IsModified = true;

                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Is true if the timeline item is selected.
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;

            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The start time of the timeline item.
        /// </summary>
        public DateTime StartTime
        {
            get => TimeLineItemModel.StartTime;
            set
            {
                if (TimeLineItemModel.StartTime != value)
                {
                    TimeLineItemModel.StartTime = value;

                    IsModified = true;

                    NotifyPropertyChanged();
                    NotifyAllPropertiesChanged();
                }
            }
        }

        /// <summary>
        /// The end time of the timeline item.
        /// </summary>
        public DateTime EndTime
        {
            get => TimeLineItemModel.EndTime;
            set
            {
                if (TimeLineItemModel.EndTime != value)
                {
                    TimeLineItemModel.EndTime = value;

                    IsModified = true;

                    NotifyPropertyChanged();
                    NotifyAllPropertiesChanged();
                }
            }
        }

        /// <summary>
        /// The duration of the timeline item.
        /// </summary>
        public TimeSpan Duration => TimeLineItemModel.EndTime - TimeLineItemModel.StartTime;

        /// <summary>
        /// Is true if timeline item is a "time span" (has a duration); otherwise it is a "time event" (has no duration).
        /// </summary>
        public bool IsTimeSpan => TimeLineItemModel.StartTime != TimeLineItemModel.EndTime;

        /// <summary>
        /// The name of the timeline item.
        /// </summary>
        public string Name
        {
            get => TimeLineItemModel.Name;
            set
            {
                if (TimeLineItemModel.Name != value)
                {
                    TimeLineItemModel.Name = value;
                    IsModified = true;

                    NotifyPropertyChanged();
                    NotifyAllPropertiesChanged();
                }
            }
        }

        /// <summary>
        /// If true, show name of timeline item; otherwise, hide it.
        /// </summary>
        public bool IsNameVisible => _settingsViewModel.IsNameVisible;

        /// <summary>
        /// If true, show "time event" timeline item; otherwise, hide it.
        /// </summary>
        public bool IsTimeEventVisible
        {
            get
            {
                double minLeft = -_settingsViewModel.TimeLineSpacerLeft;
                double actualLeft = GetActualLeft();
                double maxRight = TimeLineViewModel.TimeLinesViewModel.TimeLinesVisibleWidth;

                // Only visible if within visible bounds
                bool isVisible = actualLeft >= minLeft && actualLeft <= maxRight;

                return isVisible;
            }
        }

        /// <summary>
        /// If true, show "time span" timeline item; otherwise, hide it.
        /// </summary>
        public bool IsTimeSpanVisible => Width > 0d;

        /// <summary>
        /// The tool-tip of the timeline item.
        /// </summary>
        public string ToolTip
        {
            get
            {
                StringBuilder text = new();

                text.AppendLine(Name);

                if (IsTimeSpan)
                {
                    text.Append(System.Net.WebUtility.HtmlDecode("&#913; "));
                    text.AppendLine(TimeFormat.GetTimeString(StartTime, _settingsViewModel));

                    text.Append(System.Net.WebUtility.HtmlDecode("&#937; "));
                    text.AppendLine(TimeFormat.GetTimeString(EndTime, _settingsViewModel));

                    text.Append(System.Net.WebUtility.HtmlDecode("&#x394; "));
                    text.Append(TimeFormat.GetDurationString(EndTime - StartTime));
                }
                else
                {
                    text.Append(TimeFormat.GetTimeString(StartTime, _settingsViewModel));
                }

                return text.ToString();
            }
        }

        /// <summary>
        /// The left coordinate of the timeline item [pixel].
        /// </summary>
        public double Left
        {
            get
            {
                double minLeft = -_settingsViewModel.TimeLineSpacerLeft;
                double actualLeft = GetActualLeft();
                double left = Math.Max(minLeft, actualLeft);

                return left;
            }
        }
        /// <summary>
        /// The visible width of the "time span" timeline item [pixel].
        /// </summary>
        /// <remarks>
        /// The returned width is clipped to the currently visible timeline area.
        /// If the item is completely outside the visible area, the returned width is 0.
        /// If the item is at least partly visible, a minimum width is applied so that
        /// very small visible time spans remain selectable and recognizable.
        /// </remarks>
        public double Width
        {
            get
            {
                // Left and right edge of the visible timeline area.
                const double MinLeft = 0d;
                double maxRight = TimeLineViewModel.TimeLinesViewModel.TimeLinesVisibleWidth;

                // Actual item position in visible timeline coordinates.
                // The left spacer is added because the visible clipping area starts
                // after the left spacer.
                double left = GetActualLeft() + _settingsViewModel.TimeLineSpacerLeft;

                // Full, unclipped item width based on its duration and current scale.
                double actualWidth = _timeLineScalingViewModel.CalculatePixels(
                    Duration,
                    TimeLineViewModel.TimeLinesViewModel.Scale
                    );

                double right = left + actualWidth;

                double visibleWidth;

                // Determine visible width based on the item's position relative to
                // the visible timeline area.
                if (right <= MinLeft)
                {
                    // Item is completely left of the visible area.
                    //
                    //      +------------------+
                    // #### |                  |
                    //      +------------------+
                    visibleWidth = 0d;
                }
                else if (left >= maxRight)
                {
                    // Item is completely right of the visible area.
                    //
                    // +------------------+
                    // |                  | ####
                    // +------------------+
                    visibleWidth = 0d;
                }
                else if (left < MinLeft && right > MinLeft && right <= maxRight)
                {
                    // Item starts left of the visible area and ends inside it.
                    // Only the right part is visible.
                    //
                    //   +------------------+
                    // ##|##                |
                    //   +------------------+
                    visibleWidth = actualWidth + left;
                }
                else if (left >= MinLeft && right <= maxRight)
                {
                    // Item is fully inside the visible area.
                    //
                    // +------------------+
                    // |     #####        |
                    // +------------------+
                    visibleWidth = actualWidth;
                }
                else if (left < maxRight && left >= MinLeft && right > maxRight)
                {
                    // Item starts inside the visible area and ends right of it.
                    // Only the left part is visible.
                    //
                    // +------------------+
                    // |                ##|##
                    // +------------------+
                    visibleWidth = maxRight - left;
                }
                else
                {
                    // Item starts left of the visible area and ends right of it.
                    // The complete visible area is covered.
                    //
                    //   +------------------+
                    // ##|##################|##
                    //   +------------------+
                    visibleWidth = maxRight - MinLeft;
                }

                // If the item is completely outside the visible area, do not apply
                // the minimum width. Otherwise, a small part would remain visible even
                // though the item should be hidden.
                if (visibleWidth <= 0d)
                    return 0d;

                // Apply a minimum width only for items that are at least partly visible.
                // This keeps very small visible time spans recognizable and selectable.
                return Math.Max(visibleWidth, SettingsViewModel.TimeLineItemMinWidth);
            }
        }

        /// <summary>
        /// Command to delete the timeline item.
        /// </summary>
        public ICommand DeleteTimeLineItemCommand =>
            _deleteTimeLineItemCommand ??= new MyActionCommand(
                async _ => { await TimeLineViewModel.TimeLinesViewModel.DeleteTimeLineItem(this); },
                _ => true
            );

        /// <summary>
        /// Command to edit the timeline item.
        /// </summary>
        public ICommand EditTimeLineItemCommand =>
            _editTimeLineItemCommand ??= new MyActionCommand(
                _ => { TimeLineViewModel.TimeLinesViewModel.EditTimeLineItem(this); });

        /// <summary>
        /// Shift this timeline item to the given start time.
        /// </summary>
        public void ShiftStartTime(DateTime startTime)
        {
            if (startTime < TimeLineViewModel.TimeLinesViewModel.TotalStartTime)
            {
                startTime = TimeLineViewModel.TimeLinesViewModel.TotalStartTime;
            }

            IsModified = true;

            TimeSpan duration = TimeLineItemModel.EndTime - TimeLineItemModel.StartTime;
            TimeLineItemModel.StartTime = startTime;
            TimeLineItemModel.EndTime = startTime + duration;

            NotifyPropertyChanged(nameof(StartTime));
            NotifyPropertyChanged(nameof(EndTime));
            NotifyPropertyChanged(nameof(Left));
            NotifyPropertyChanged(nameof(Width));
            NotifyPropertyChanged(nameof(IsTimeSpanVisible));
            NotifyPropertyChanged(nameof(IsTimeEventVisible));
            NotifyPropertyChanged(nameof(ToolTip));
        }

        /// <summary>
        /// Calculates the actual left position of the timeline item [pixel].
        /// </summary>
        private double GetActualLeft()
        {
            TimeSpan offsetSpan = StartTime - TimeLineViewModel.TimeLinesViewModel.TotalStartTime;
            double offsetPixels = _timeLineScalingViewModel.CalculatePixels(offsetSpan, TimeLineViewModel.TimeLinesViewModel.Scale);
            double left = offsetPixels - TimeLineViewModel.TimeLinesViewModel.HorizontalScrollOffset;
            return left;
        }

        /// <summary>
        /// Is called when a property of the timelines view model has changed.
        /// </summary>
        private void TimeLinesViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(TimeLinesViewModel.IsModified):
                    _isModified = TimeLineViewModel.TimeLinesViewModel.IsModified;
                    break;

                case nameof(TimeLinesViewModel.Scale):
                    NotifyPropertyChanged(nameof(Left));
                    NotifyPropertyChanged(nameof(Width));
                    NotifyPropertyChanged(nameof(IsTimeSpanVisible));
                    NotifyPropertyChanged(nameof(IsTimeEventVisible));
                    break;

                case nameof(TimeLinesViewModel.HorizontalScrollOffset):
                    NotifyPropertyChanged(nameof(Left));
                    NotifyPropertyChanged(nameof(Width));
                    NotifyPropertyChanged(nameof(IsTimeSpanVisible));
                    NotifyPropertyChanged(nameof(IsTimeEventVisible));
                    break;
            }
        }

        /// <summary>
        /// Is called when a global setting has changed.
        /// </summary>
        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SettingsViewModel.IsNameVisible):
                    NotifyPropertyChanged(nameof(IsNameVisible));
                    break;

                case nameof(SettingsViewModel.IsCompactTimeLines):
                    NotifyPropertyChanged(nameof(TextOffsetY));
                    NotifyPropertyChanged(nameof(TimeSpanHeight));
                    NotifyPropertyChanged(nameof(TimeEventHeight));
                    NotifyPropertyChanged(nameof(TimeEventWidth));
                    NotifyPropertyChanged(nameof(TimeEventOffsetX));
                    NotifyPropertyChanged(nameof(TimeEventTextOffsetX));
                    break;

                case nameof(SettingsViewModel.IsCompactTimeGrid):
                    NotifyPropertyChanged(nameof(Left));
                    NotifyPropertyChanged(nameof(Width));
                    NotifyPropertyChanged(nameof(IsTimeSpanVisible));
                    NotifyPropertyChanged(nameof(IsTimeEventVisible));
                    break;

                case nameof(SettingsViewModel.IsUniversalTime):
                case nameof(SettingsViewModel.TimeZone):
                    NotifyPropertyChanged(nameof(ToolTip));
                    break;
            }
        }

        /// <summary>
        /// Set color of timeline item from given color name.
        /// </summary>
        private void SetColorFromName(string colorName)
        {
            if (string.IsNullOrEmpty(colorName))
            {
                _color = IsTimeSpan ? SettingsViewModel.TimeLinerColors.TimeSpan
                                    : SettingsViewModel.TimeLinerColors.TimeEvent;
            }
            else
            {
                _color = (Color)ColorConverter.ConvertFromString(colorName)!;
            }
        }

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Get string representation of timeline item view model.
        /// </summary>
        public override string ToString()
        {
            return TimeLineItemModel.Name;
        }

        /// <summary>
        /// Implements the Dispose pattern.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    PropertyChangedEventManager.RemoveHandler(TimeLineViewModel.TimeLinesViewModel, TimeLinesViewModel_PropertyChanged, "");
                    PropertyChangedEventManager.RemoveHandler(_settingsViewModel, Settings_PropertyChanged, "");
                    PropertyChangedEventManager.RemoveHandler(this, TimeLineViewModel.TimeLinesViewModel.TimeLineItemViewModel_PropertyChanged, "");
                }

                _isDisposed = true;
            }
        }
    }
}
