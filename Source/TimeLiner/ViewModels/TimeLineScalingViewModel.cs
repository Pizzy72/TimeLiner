// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2026 Christian Pistor

using System;
using System.Collections.Generic;
using System.Linq;
using TimeLiner.Common;

namespace TimeLiner.ViewModels
{
    /// <summary>
    /// Index for selecting a timeline scale.
    /// </summary>
    public enum ScaleIndex
    {
        OneWeek,
        OneDay,
        HalfDay,
        FourHours,
        TwoHours,
        OneHour,
        HalfHour,
        QuarterHour,
        TenMinutes,
        FiveMinutes,
        OneMinute,
        HalfMinute,
        TenSeconds,
        FiveSeconds,
        TwoSeconds,
        Second,
        HalfSecond,
        QuarterSecond,
        DeciSecond,
        FiftyMilliseconds,
        TwentyfiveMilliseconds,
        TenMilliseconds,
        FiveMilliseconds,
        Millisecond
    }

    /// <summary>
    /// Handles the timeline scaling.
    /// </summary>
    internal class TimeLineScalingViewModel
    {
        /// <summary>
        /// The adjustable timeline scales.
        /// </summary>
        private readonly Dictionary<ScaleIndex, ScaleViewModel> _scales;

        public TimeLineScalingViewModel(SettingsViewModel settingsViewModel)
        {
            _scales = new()
            {
                // d.hh:mm:ss.ff,
                {ScaleIndex.OneWeek,
                    new ScaleViewModel(1, TimeConstants.UnitName.Week, TimeSpan.FromDays(TimeConstants.WeekDays).TotalSeconds, settingsViewModel) },
                {ScaleIndex.OneDay,
                    new ScaleViewModel(1, TimeConstants.UnitName.Day, TimeSpan.FromDays(1).TotalSeconds, settingsViewModel) },
                {ScaleIndex.HalfDay,
                    new ScaleViewModel(12, TimeConstants.UnitName.Hour, TimeSpan.FromHours(TimeConstants.DayHours / 2d).TotalSeconds, settingsViewModel) },
                {ScaleIndex.FourHours,
                    new ScaleViewModel(4, TimeConstants.UnitName.Hour, TimeSpan.FromHours(4).TotalSeconds, settingsViewModel )},
                {ScaleIndex.TwoHours,
                    new ScaleViewModel(2, TimeConstants.UnitName.Hour, TimeSpan.FromHours(2).TotalSeconds, settingsViewModel )},
                {ScaleIndex.OneHour,
                    new ScaleViewModel(1, TimeConstants.UnitName.Hour, TimeSpan.FromHours(1).TotalSeconds, settingsViewModel) },
                {ScaleIndex.HalfHour,
                    new ScaleViewModel(30, TimeConstants.UnitName.Minute, TimeSpan.FromMinutes(30).TotalSeconds, settingsViewModel) },
                {ScaleIndex.QuarterHour,
                    new ScaleViewModel(15, TimeConstants.UnitName.Minute, TimeSpan.FromMinutes(15).TotalSeconds, settingsViewModel) },
                {ScaleIndex.TenMinutes,
                    new ScaleViewModel(10, TimeConstants.UnitName.Minute, TimeSpan.FromMinutes(10).TotalSeconds, settingsViewModel) },
                {ScaleIndex.FiveMinutes,
                    new ScaleViewModel(5, TimeConstants.UnitName.Minute, TimeSpan.FromMinutes(5).TotalSeconds, settingsViewModel) },
                {ScaleIndex.OneMinute,
                    new ScaleViewModel(1, TimeConstants.UnitName.Minute, TimeConstants.MinuteSeconds, settingsViewModel) },
                {ScaleIndex.HalfMinute,
                    new ScaleViewModel(30, TimeConstants.UnitName.Second, TimeConstants.MinuteSeconds / 2d, settingsViewModel) },
                {ScaleIndex.TenSeconds,
                    new ScaleViewModel(10, TimeConstants.UnitName.Second, 10.0, settingsViewModel) },
                {ScaleIndex.FiveSeconds,
                    new ScaleViewModel(5, TimeConstants.UnitName.Second, 5.0, settingsViewModel) },
                {ScaleIndex.TwoSeconds,
                    new ScaleViewModel(2, TimeConstants.UnitName.Second, 2.0, settingsViewModel)  },
                {ScaleIndex.Second,
                    new ScaleViewModel(1, TimeConstants.UnitName.Second, 1.0, settingsViewModel)  },
                {ScaleIndex.HalfSecond,
                    new ScaleViewModel(500, TimeConstants.UnitName.Millisecond, TimeSpan.FromMilliseconds(500).TotalSeconds, settingsViewModel) },
                {ScaleIndex.QuarterSecond,
                    new ScaleViewModel(250, TimeConstants.UnitName.Millisecond, TimeSpan.FromMilliseconds(250).TotalSeconds, settingsViewModel) },
                {ScaleIndex.DeciSecond,
                    new ScaleViewModel(100, TimeConstants.UnitName.Millisecond, TimeSpan.FromMilliseconds(100).TotalSeconds, settingsViewModel) },
                {ScaleIndex.FiftyMilliseconds,
                    new ScaleViewModel(50, TimeConstants.UnitName.Millisecond, TimeSpan.FromMilliseconds(50).TotalSeconds, settingsViewModel) },
                {ScaleIndex.TwentyfiveMilliseconds,
                    new ScaleViewModel(25, TimeConstants.UnitName.Millisecond, TimeSpan.FromMilliseconds(25).TotalSeconds, settingsViewModel) },
                {ScaleIndex.TenMilliseconds,
                    new ScaleViewModel(10, TimeConstants.UnitName.Millisecond, TimeSpan.FromMilliseconds(10).TotalSeconds, settingsViewModel) },
                {ScaleIndex.FiveMilliseconds,
                    new ScaleViewModel(5, TimeConstants.UnitName.Millisecond, TimeSpan.FromMilliseconds(5).TotalSeconds, settingsViewModel) },
                {ScaleIndex.Millisecond,
                    new ScaleViewModel(1, TimeConstants.UnitName.Millisecond, 0.001, settingsViewModel) },
            };
        }

        /// <summary>
        /// The scale indexes.
        /// </summary>
        public IReadOnlyList<ScaleIndex> Scales => Enum.GetValues(typeof(ScaleIndex)).OfType<ScaleIndex>().ToList().AsReadOnly();

        /// <summary>
        /// Calculate pixels from given timespan based on given scale.
        /// </summary>
        public double CalculatePixels(TimeSpan timespan, ScaleIndex scale)
        {
            return GetScaleValue(scale) * timespan.TotalSeconds;
        }

        /// <summary>
        /// Calculate seconds from given pixels based on given scale.
        /// </summary>
        public double CalculateSeconds(double pixels, ScaleIndex scale)
        {
            return pixels / GetScaleValue(scale);
        }

        /// <summary>
        /// Get interval of given scale.
        /// </summary>
        public int GetScaleInterval(ScaleIndex scale)
        {
            return _scales[scale].Interval;
        }

        /// <summary>
        /// Get text of given scale.
        /// </summary>
        public string GetScaleText(ScaleIndex scale)
        {
            return _scales[scale].ToString();
        }

        /// <summary>
        /// Get unit of given scale.
        /// </summary>
        public string GetScaleUnit(ScaleIndex scale)
        {
            return _scales[scale].Unit;
        }

        /// <summary>
        /// Get scale value in pixel/s for given scale.
        /// </summary>
        public double GetScaleValue(ScaleIndex scale)
        {
            return _scales[scale].Scale;
        }

        /// <summary>
        /// Represents a timeline scale.
        /// </summary>
        private class ScaleViewModel
        {
            /// <summary>
            /// Reference to the global settings view model.
            /// </summary>
            private readonly SettingsViewModel _settingsViewModel;

            public ScaleViewModel(int interval, string unit, double seconds, SettingsViewModel settingsViewModel)
            {
                Interval = interval;
                Unit = unit;
                Seconds = seconds;
                _settingsViewModel = settingsViewModel;
            }

            public int Interval { get; }

            public string Unit { get; }

            public double Seconds { get; }

            public double Scale => _settingsViewModel.TimeGridWidth / Seconds;

            public override string ToString()
            {
                return $"{Interval} {Unit}";
            }
        }
    }
}
