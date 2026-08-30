// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Christian Pistor

using System;
using System.Collections.ObjectModel;
using TimeLiner.Common;

namespace TimeLinerTest.TestDoubles
{
    internal sealed class TimeZoneInfoProviderStub : ITimeZoneInfoProvider
    {
        public TimeZoneInfoProviderStub(string timeZone, bool isUniversalTime)
        {
            TimeZone = timeZone;
            IsUniversalTime = isUniversalTime;
            TimeZones = TimeZoneInfo.GetSystemTimeZones();
        }

        public string TimeZone { get; }

        public bool IsUniversalTime { get; }

        public ReadOnlyCollection<TimeZoneInfo> TimeZones { get; }
    }
}
