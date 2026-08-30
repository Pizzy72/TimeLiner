// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2026 Christian Pistor

using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using TimeLiner.Common;
using TimeLiner.Models;
using TimeLiner.Themes;

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

        public SettingsViewModel(ISettingsRepository repository)
        {
            _repository = repository;
            _settings = _repository.Load();
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
            get => _settings.IsUniversalTime;
            set
            {
                if (_settings.IsUniversalTime != value)
                {
                    _settings.IsUniversalTime = value;
                    NotifyPropertyChanged();
                }
            }
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
            public static Color TimeSpan = Colors.DeepSkyBlue;
            public static Color TimeEvent = Colors.DeepPink;
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
        /// Toggles the between UTC time and local time.
        /// </summary>
        public void ToggleTimeFormat()
        {
            IsUniversalTime = !IsUniversalTime;
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
