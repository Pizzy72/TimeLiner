// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2025 Christian Pistor

namespace TimeLiner.Common
{
    /// <summary>
    /// Time-related constants.
    /// </summary>
    internal static class TimeConstants
    {
        public const double WeekDays = 7d;
        public const double DayHours = 24d;
        public const double MinuteSeconds = 60d;

        public class UnitName
        {
            public const string Week = "wk";
            public const string Day = "d";
            public const string Hour = "h";
            public const string Minute = "min";
            public const string Second = "s";
            public const string Millisecond = "ms";
        }
    }
}