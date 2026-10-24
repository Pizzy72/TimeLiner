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
        /// If true, the selected fixed offset is zero (UTC).
        /// </summary>
        bool IsUniversalTime { get; }

        /// <summary>The selected regional or fixed-offset display mode.</summary>
        TimeDisplayMode DisplayMode { get; }

        /// <summary>The selected fixed offset, including a resolved first-event offset.</summary>
        TimeSpan UtcOffset { get; }

        /// <summary>
        /// The list of available system time zones.
        /// </summary>
        ReadOnlyCollection<TimeZoneInfo> TimeZones { get; }
    }
}
