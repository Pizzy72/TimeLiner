// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Christian Pistor

using System;
using System.Collections.ObjectModel;
using TimeLiner.Common;

namespace TimeLinerTest.TestDoubles
{
    internal sealed class TimeZoneInfoProviderStub : ITimeZoneInfoProvider
    {
        public TimeZoneInfoProviderStub(string timeZone, bool isUniversalTime, TimeSpan? fixedOffset = null)
        {
            TimeZone = timeZone;
            IsUniversalTime = isUniversalTime;
            TimeZones = TimeZoneInfo.GetSystemTimeZones();
            DisplayMode = fixedOffset.HasValue || isUniversalTime ? TimeDisplayMode.FixedOffset : TimeDisplayMode.Regional;
            UtcOffset = fixedOffset ?? TimeSpan.Zero;
        }

        public string TimeZone { get; }

        public bool IsUniversalTime { get; }

        public TimeDisplayMode DisplayMode { get; }

        public TimeSpan UtcOffset { get; }

        public ReadOnlyCollection<TimeZoneInfo> TimeZones { get; }
    }
}
