// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2026 Christian Pistor

using TimeLiner.Common;

namespace TimeLiner.ViewModels
{
    /// <summary>
    /// The view model of the timeline selector.
    /// </summary>
    internal class TimeLineSelectorViewModel : ViewModelBase
    {
        /// <summary>
        /// Reference to the timelines view model.
        /// </summary>
        private readonly TimeLinesViewModel _timeLinesViewModel;

        /// <summary>
        /// Reference to the global settings view model.
        /// </summary>
        private readonly SettingsViewModel _settingsViewModel;

        /// <summary>
        /// The selected timeline.
        /// </summary>
        private TimeLineViewModel _selectedTimeLine;

        /// <see cref="IsVisible"/>
        private bool _isVisible;

        /// <see cref="MarginTop"/>
        private double _marginTop;

        /// <summary>
        /// Constructor.
        /// </summary>
        public TimeLineSelectorViewModel(TimeLinesViewModel timeLinesViewModel, SettingsViewModel settingsViewModel)
        {
            _timeLinesViewModel = timeLinesViewModel;
            _settingsViewModel = settingsViewModel;
        }

        /// <summary>
        /// The top margin in pixels, which vertically positions the timeline selector on the timeline grid.
        /// </summary>
        public double MarginTop
        {
            get => _marginTop;

            set
            {
                if (_marginTop != value)
                {
                    _marginTop = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Determines if the timeline selector is visible.
        /// </summary>
        public bool IsVisible
        {
            get => _isVisible;

            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The selected timeline. Is null not selected.
        /// </summary>
        public TimeLineViewModel SelectedTimeLine
        {
            get => _selectedTimeLine;
            set
            {
                if (_selectedTimeLine == value)
                {
                    return;
                }

                _selectedTimeLine = value;

                if (_selectedTimeLine == null)
                {
                    IsVisible = false;
                    return;
                }

                MarginTop = GetTimeLineIndex(_selectedTimeLine) *
                    _settingsViewModel.TimeLineHeight -
                    _timeLinesViewModel.VerticalScrollOffset;

                IsVisible = true;
            }
        }

        /// <summary>
        /// Get the index of the given timeline in the list of timelines.
        /// </summary>
        private int GetTimeLineIndex(TimeLineViewModel timeLine)
        {
            int index = 0;

            foreach (TimeLineViewModel currentTimeLine in _timeLinesViewModel.TimeLines)
            {
                if (currentTimeLine == timeLine)
                {
                    return index;
                }
                index++;
            }

            throw new TimeLinerException($"Could not determine index of timeline '{timeLine.Name}'");
        }
    }
}
