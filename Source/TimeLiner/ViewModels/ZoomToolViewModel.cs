// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2026 Christian Pistor

using System;
using System.ComponentModel;
using System.Linq;

namespace TimeLiner.ViewModels
{
    /// <summary>
    /// The view model of the zoom tool.
    /// </summary>
    internal class ZoomToolViewModel : ViewModelBase
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

        /// <see cref="Left"/>
        private double _left;

        /// <see cref="Width"/>
        private double _width;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ZoomToolViewModel(TimeLinesViewModel timeLinesViewModel, SettingsViewModel settingsViewModel, TimeLineScalingViewModel timeLineScaling)
        {
            _timeLinesViewModel = timeLinesViewModel;
            _settingsViewModel = settingsViewModel;
            _timeLineScalingViewModel = timeLineScaling;

            PropertyChangedEventManager.AddHandler(timeLinesViewModel, TimeLinesViewModel_PropertyChanged, "");
        }

        /// <summary>
        /// If true, show the zoom tool; otherwise, hide it.
        /// </summary>
        public bool IsVisible => _width > 0d;

        /// <summary>
        /// The height of the zoom tool.
        /// </summary>
        public double Height => _timeLinesViewModel.TimeLinesVisibleHeight;

        /// <summary>
        /// The left position of the zoom tool [pixel].
        /// </summary>
        public double Left
        {
            get => _left;
            set
            {
                if (Math.Abs(_left - value) > double.Epsilon)
                {
                    _left = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The width of the zoom tool [pixel].
        /// </summary>
        public double Width
        {
            get => _width;
            set
            {
                if (Math.Abs(_width - value) > double.Epsilon)
                {
                    _width = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(IsVisible));
                }
            }
        }

        /// <summary>
        /// Release zoom tool.
        /// </summary>
        public void Release()
        {
            if (Width > 0d)
            {
                ZoomTimeLines();
                Width = 0d;
            }
        }

        /// <summary>
        /// Zoom timelines into the selected width.
        /// </summary>
        private void ZoomTimeLines()
        {
            DateTime zoomStartTime = GetZoomStartTime();
            DateTime zoomEndTime = GetZoomEndTime();

            TimeLineItemViewModel firstTimeLineItem = _timeLinesViewModel.TimeLineItems
                .FirstOrDefault(timeLineItem => timeLineItem.StartTime >= zoomStartTime);

            if (firstTimeLineItem != null)
            {
                zoomStartTime = firstTimeLineItem.StartTime;
            }

            ScaleIndex newScale = GetFittingScale(zoomStartTime, zoomEndTime, _timeLinesViewModel.TimeLinesVisibleWidth);

            TimeSpan scrollTimeSpan = zoomStartTime - _timeLinesViewModel.TotalStartTime;

            double scrollOffset = _timeLineScalingViewModel.CalculatePixels(scrollTimeSpan, newScale) +
                                  _settingsViewModel.TimeLineSpacerLeft;

            _timeLinesViewModel.Scale = newScale;
            _timeLinesViewModel.HorizontalScrollOffset = scrollOffset;
        }

        /// <summary>
        /// Get the start-time for the left border of the zoom tool.
        /// </summary>
        private DateTime GetZoomStartTime()
        {
            double offsetPixels = Math.Max(_timeLinesViewModel.HorizontalScrollOffset + Left - _settingsViewModel.TimeLineSpacerLeft, 0d);
            double offsetSeconds = _timeLineScalingViewModel.CalculateSeconds(offsetPixels, _timeLinesViewModel.Scale);
            DateTime startTime = _timeLinesViewModel.TotalStartTime.AddSeconds(offsetSeconds);

            return startTime;
        }

        /// <summary>
        /// Get the end-time for the right border of the zoom tool.
        /// </summary>
        private DateTime GetZoomEndTime()
        {
            double offsetPixels = Math.Max(_timeLinesViewModel.HorizontalScrollOffset + Left + Width - _settingsViewModel.TimeLineSpacerLeft, 0d);
            double offsetSeconds = _timeLineScalingViewModel.CalculateSeconds(offsetPixels, _timeLinesViewModel.Scale);
            DateTime endTime = _timeLinesViewModel.TotalStartTime.AddSeconds(offsetSeconds);

            return endTime;
        }

        /// <summary>
        /// Get the scale which fits the timelines into the given width.
        /// </summary>
        private ScaleIndex GetFittingScale(DateTime startTime, DateTime endTime, double widthToFit)
        {
            TimeSpan timespan = endTime - startTime;

            ScaleIndex fittingScale = _timeLineScalingViewModel.Scales.Last();

            foreach (ScaleIndex scale in _timeLineScalingViewModel.Scales)
            {
                double width = _timeLineScalingViewModel.CalculatePixels(timespan, scale);

                if (width > widthToFit)
                {
                    break;
                }

                fittingScale = scale;
            }

            return fittingScale;
        }

        /// <summary>
        /// Is called when a property of the timelines view model has changed.
        /// </summary>
        private void TimeLinesViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(TimeLinesViewModel.TimeLinesVisibleHeight):
                    NotifyPropertyChanged(nameof(Height));
                    break;
            }
        }
    }
}