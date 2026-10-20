// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2026 Christian Pistor

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using TimeLiner.Common;
using TimeLiner.Models;
using TimeLiner.Themes;
using OffsetUtility = TimeLiner.Common.UtcOffset;

namespace TimeLiner.ViewModels
{
    /// <summary>
    /// The view model for the global settings.
    /// </summary>
    internal sealed class SettingsViewModel : ViewModelBase, ITimeZoneInfoProvider
    {
        /// <summary>
        /// Reference to the settings repository.
        /// </summary>
        private readonly ISettingsRepository _repository;

        /// <see cref="Settings"/>
        private readonly SettingsModel _settings;

        private string _offsetInput;
        private string _offsetError;
        private TimeSpan? _firstEventOffset;
        private IReadOnlyList<string> _offsetOptions;

        private const string UtcOptionLabel = "UTC (+00:00)";

        public SettingsViewModel(ISettingsRepository repository)
        {
            _repository = repository;
            _settings = _repository.Load();
            if (Math.Abs((long)_settings.FixedUtcOffsetMinutes) > 14 * 60)
                _settings.FixedUtcOffsetMinutes = 0;
            _offsetInput = OffsetText;
            _offsetOptions = CreateOffsetOptions();
        }

        /// <summary>
        /// If true, minimize the ribbon menu and the info text; otherwise, expand it.
        /// </summary>
        public bool IsMinimalUi { get; internal set; }

        /// <summary>
        /// The current settings.
        /// </summary>
        public SettingsModel Settings => _settings;

        /// <summary>
        /// If true, show compact time grid; otherwise, show normal grid.
        /// </summary>
        public bool IsCompactTimeGrid
        {
            get => _settings.IsCompactTimeGrid;
            set
            {
                if (_settings.IsCompactTimeGrid != value)
                {
                    _settings.IsCompactTimeGrid = value;

                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(TimeGridWidth));
                    NotifyPropertyChanged(nameof(TimeLineSpacerLeft));
                    NotifyPropertyChanged(nameof(TimeLineSpacerRight));
                }
            }
        }

        /// <summary>
        /// If true, show compact timelines; otherwise, show normal timelines.
        /// </summary>
        public bool IsCompactTimeLines
        {
            get => _settings.IsCompactTimeLines;
            set
            {
                if (_settings.IsCompactTimeLines != value)
                {
                    _settings.IsCompactTimeLines = value;
                    NotifyPropertyChanged(nameof(TimeLineHeight));
                    NotifyPropertyChanged();
                }
            }
        }


        /// <summary>
        /// The selected application theme (Dark / Light).
        /// </summary>
        public AppTheme SelectedTheme
        {
            get => _settings.Theme;
            set
            {
                if (_settings.Theme != value)
                {
                    _settings.Theme = value;
                    AppServices.ThemeService.ApplyTheme(_settings.Theme);
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// If true, show timeline item name; otherwise, hide it.
        /// </summary>
        public bool IsNameVisible
        {
            get => _settings.IsNameVisible;
            set
            {
                if (_settings.IsNameVisible != value)
                {
                    _settings.IsNameVisible = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <inheritdoc/>
        public bool IsUniversalTime
        {
            get => IsFixedOffset && UtcOffset == TimeSpan.Zero;
            set
            {
                if (value)
                {
                    _settings.FixedUtcOffsetMinutes = 0;
                    _settings.UseFirstEventOffset = false;
                }
                _settings.DisplayMode = value ? TimeDisplayMode.FixedOffset : TimeDisplayMode.Regional;
                _offsetError = null;
                NotifyTimeDisplayChanged();
            }
        }

        public TimeDisplayMode DisplayMode
        {
            get => _settings.DisplayMode ?? (_settings.IsUniversalTime
                ? TimeDisplayMode.FixedOffset : TimeDisplayMode.Regional);
            set
            {
                if (DisplayMode == value && _settings.DisplayMode.HasValue)
                    return;
                _settings.DisplayMode = value;
                _offsetError = null;
                NotifyTimeDisplayChanged();
            }
        }

        public IReadOnlyList<KeyValuePair<TimeDisplayMode, string>> DisplayModes { get; } =
        [
            new(TimeDisplayMode.Regional, "Regional time"),
            new(TimeDisplayMode.FixedOffset, "Fixed UTC offset")
        ];

        public bool IsRegionalTime => DisplayMode == TimeDisplayMode.Regional;
        public bool IsFixedOffset => DisplayMode == TimeDisplayMode.FixedOffset;
        public TimeSpan UtcOffset => TimeSpan.FromMinutes(_settings.FixedUtcOffsetMinutes);
        public TimeSpan? FirstEventOffset => _firstEventOffset;
        public bool UsesFirstEventOffset => _settings.UseFirstEventOffset;

        public string FirstEventOffsetLabel => _firstEventOffset.HasValue
            ? $"First event ({OffsetUtility.Format(_firstEventOffset.Value)})"
            : "First event (unavailable)";

        // Replacing ItemsSource during an edit makes ComboBox restore its old
        // selection over the committed text. Only a new file context changes it.
        public IReadOnlyList<string> OffsetOptions => _offsetOptions;

        private IReadOnlyList<string> CreateOffsetOptions()
        {
            List<string> options = [FirstEventOffsetLabel, UtcOptionLabel];
            for (int hour = -12; hour <= 14; hour++)
            {
                // UTC is already available as the shortcut at the top.
                if (hour == 0)
                    continue;
                options.Add(OffsetUtility.Format(TimeSpan.FromHours(hour)));
                if (hour == 5)
                {
                    options.Add("+05:30");
                    options.Add("+05:45");
                }
            }
            return options.AsReadOnly();
        }

        public string OffsetText => UsesFirstEventOffset ? FirstEventOffsetLabel : OffsetUtility.Format(UtcOffset);

        // Editing is deliberately separate from committing the display setting.
        public string OffsetInput
        {
            get => _offsetInput;
            set
            {
                _offsetInput = value;
                NotifyPropertyChanged();
            }
        }

        public bool HasOffsetError => !string.IsNullOrEmpty(_offsetError)
            || (IsFixedOffset && UsesFirstEventOffset && !_firstEventOffset.HasValue);

        public string TimeDisplayHint => !string.IsNullOrEmpty(_offsetError) ? _offsetError
            : IsRegionalTime ? "Daylight saving rules apply."
            : UsesFirstEventOffset && !_firstEventOffset.HasValue
                ? "First event has no explicit offset. Select an offset."
                : "Fixed offset; no daylight saving adjustment.";

        public bool TryCommitOffsetInput()
        {
            string text = OffsetInput?.Trim();
            if (text == UtcOptionLabel)
                text = "+00:00";
            bool automatic = string.Equals(text, "auto", StringComparison.OrdinalIgnoreCase)
                || text == FirstEventOffsetLabel;
            TimeSpan offset;
            if (automatic)
            {
                if (!_firstEventOffset.HasValue)
                    return RejectOffset("First event has no explicit offset. Select an offset.");
                offset = _firstEventOffset.Value;
            }
            else if (!OffsetUtility.TryParse(text, out offset))
            {
                return RejectOffset("Use ±HH:mm between -14:00 and +14:00.");
            }

            _settings.FixedUtcOffsetMinutes = (int)offset.TotalMinutes;
            _settings.UseFirstEventOffset = automatic;
            _offsetError = null;
            NotifyTimeDisplayChanged();
            return true;
        }

        private bool RejectOffset(string message)
        {
            _offsetError = message;
            NotifyPropertyChanged(nameof(HasOffsetError));
            NotifyPropertyChanged(nameof(TimeDisplayHint));
            return false;
        }

        public void CancelOffsetInput()
        {
            _offsetError = null;
            _offsetInput = OffsetText;
            NotifyPropertyChanged(nameof(OffsetInput));
            NotifyPropertyChanged(nameof(HasOffsetError));
            NotifyPropertyChanged(nameof(TimeDisplayHint));
        }

        public void SetFirstEventOffset(TimeSpan? offset)
        {
            bool changed = _firstEventOffset != offset;
            _firstEventOffset = offset;
            if (UsesFirstEventOffset && offset.HasValue)
                _settings.FixedUtcOffsetMinutes = (int)offset.Value.TotalMinutes;
            if (changed)
            {
                _offsetOptions = CreateOffsetOptions();
                NotifyPropertyChanged(nameof(OffsetOptions));
            }
            _offsetError = null;
            NotifyTimeDisplayChanged();
        }

        public void SelectFixedOffset(TimeSpan offset, bool fromFirstEvent = false)
        {
            if (offset.Duration() > TimeSpan.FromHours(14) || offset.Ticks % TimeSpan.TicksPerMinute != 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            _settings.FixedUtcOffsetMinutes = (int)offset.TotalMinutes;
            _settings.UseFirstEventOffset = fromFirstEvent;
            _settings.DisplayMode = TimeDisplayMode.FixedOffset;
            _offsetError = null;
            NotifyTimeDisplayChanged();
        }

        private void NotifyTimeDisplayChanged()
        {
            _settings.IsUniversalTime = IsUniversalTime;
            _offsetInput = OffsetText;
            NotifyPropertyChanged(nameof(DisplayMode));
            NotifyPropertyChanged(nameof(IsRegionalTime));
            NotifyPropertyChanged(nameof(IsFixedOffset));
            NotifyPropertyChanged(nameof(UtcOffset));
            NotifyPropertyChanged(nameof(FirstEventOffset));
            NotifyPropertyChanged(nameof(UsesFirstEventOffset));
            NotifyPropertyChanged(nameof(OffsetText));
            NotifyPropertyChanged(nameof(OffsetInput));
            NotifyPropertyChanged(nameof(HasOffsetError));
            NotifyPropertyChanged(nameof(TimeDisplayHint));
            // Existing timeline, locator and clipboard views observe this property
            // as their display-format invalidation signal, including nonzero offsets.
            NotifyPropertyChanged(nameof(IsUniversalTime));
        }

        /// <summary>
        /// If true, lock the position of the time locators; otherwise, allow to scroll them with the timeline.
        /// </summary>
        public bool IsTimeLocatorLocked
        {
            get => _settings.IsTimeLocatorLocked;
            set
            {
                if (_settings.IsTimeLocatorLocked != value)
                {
                    _settings.IsTimeLocatorLocked = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The normal timeline height [pixel].
        /// </summary>
        public const double NormalTimeLineHeight = 30d;

        /// <summary>
        /// The compact timeline height [pixel].
        /// </summary>
        public const double CompactTimeLineHeight = 18d;

        /// <summary>
        /// The current timeline height [pixel].
        /// </summary>
        public double TimeLineHeight => IsCompactTimeLines ? CompactTimeLineHeight : NormalTimeLineHeight;

        /// <summary>
        /// The normal time grid width [pixel].
        /// </summary>
        public const double NormalTimeGridWidth = 100d;

        /// <summary>
        /// The compact time grid width [pixel].
        /// </summary>
        public const double CompactTimeGridWith = 50d;

        /// <summary>
        /// The current time grid width [pixel].
        /// </summary>
        public double TimeGridWidth => IsCompactTimeGrid ? CompactTimeGridWith : NormalTimeGridWidth;

        /// <summary>
        /// The opacity of a timeline item.
        /// </summary>
        public double TimeLineItemOpacity => 1d;

        /// <summary>
        /// The minimum width of a timeline item with duration [pixel].
        /// </summary>
        public const double TimeLineItemMinWidth = 3d;

        /// <summary>
        /// Default TimeLiner colors.
        /// </summary>
        public static class TimeLinerColors
        {
            public static Color TimeSpan = Color.FromRgb(0x3F, 0xFF, 0xFF);
            public static Color TimeEvent = Color.FromRgb(0xFF, 0x3F, 0xFF);
        }

        /// <summary>
        /// The empty space in pixels before the first timeline item.
        /// </summary>
        public double TimeLineSpacerLeft => TimeGridWidth;

        /// <summary>
        /// The empty space in pixels after the last timeline item.
        /// </summary>
        public double TimeLineSpacerRight => TimeGridWidth * 4.0;

        /// <inheritdoc/>
        public string TimeZone
        {
            get => _settings.TimeZone;
            set
            {
                if (_settings.TimeZone != value)
                {
                    _settings.TimeZone = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <inheritdoc/>
        public ReadOnlyCollection<TimeZoneInfo> TimeZones => TimeZoneInfo.GetSystemTimeZones();

        /// <summary>
        /// Supported extensions for TimeLiner files.
        /// </summary>
        public static readonly string[] FileExtensions = { ".tli", ".csv" };

        /// <summary>
        /// Toggles the between compact and normal time grid.
        /// </summary>
        public void ToggleTimeGridWidth()
        {
            IsCompactTimeGrid = !IsCompactTimeGrid;
        }

        /// <summary>
        /// Tooggles the between compact and normal timelines.
        /// </summary>
        public void ToggleTimeLineHeight()
        {
            IsCompactTimeLines = !IsCompactTimeLines;
        }

        /// <summary>
        /// Toggles the visibility of timeline item names.
        /// </summary>
        public void ToogleNameVisibility()
        {
            IsNameVisible = !IsNameVisible;
        }

        /// <summary>
        /// Toggles the between dark and light theme.
        /// </summary>
        public void ToggleTheme()
        {
            SelectedTheme = SelectedTheme == AppTheme.Dark ?
                AppTheme.Light :
                AppTheme.Dark;
        }

        /// <summary>
        /// Toggles regional time and the selected fixed offset (UTC by default).
        /// </summary>
        public void ToggleTimeFormat()
        {
            DisplayMode = IsRegionalTime ? TimeDisplayMode.FixedOffset : TimeDisplayMode.Regional;
        }

        /// <summary>
        /// Toggles the time locator lock.
        /// </summary>
        public void ToggleTimeLocatorLocking()
        {
            IsTimeLocatorLocked = !IsTimeLocatorLocked;
        }

        /// <summary>
        /// Save the current settings to the repository.
        /// </summary>
        public void Save()
        {
            _repository.Save(_settings);
        }
    }
}
