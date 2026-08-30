// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2025 Christian Pistor

using System;
using System.Collections.ObjectModel;

namespace TimeLiner.Common
{
    /// <summary>
    /// Interface of an object which provides current time zone information.
    /// </summary>
    public interface ITimeZoneInfoProvider
    {
        /// <summary>
        /// The ID of the selected time zone.
        /// </summary>
        string TimeZone { get; }

        /// <summary>
        /// If true, display UTC time; otherwise, display local time in selected time zone.
        /// </summary>
        bool IsUniversalTime { get; }

        /// <summary>
        /// The list of available system time zones.
        /// </summary>
        ReadOnlyCollection<TimeZoneInfo> TimeZones { get; }
    }
}