// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Christian Pistor

using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace TimeLiner.Common
{
    internal static class UtcOffset
    {
        public static bool TryParse(string text, out TimeSpan offset)
        {
            offset = default;
            if (text == null || !Regex.IsMatch(text, @"\A[+-][0-9]{2}:[0-9]{2}\z"))
                return false;

            int hours = int.Parse(text.AsSpan(1, 2), CultureInfo.InvariantCulture);
            int minutes = int.Parse(text.AsSpan(4, 2), CultureInfo.InvariantCulture);
            if (minutes > 59 || hours > 14 || (hours == 14 && minutes != 0))
                return false;

            offset = TimeSpan.FromMinutes((hours * 60 + minutes) * (text[0] == '-' ? -1 : 1));
            return true;
        }

        public static string Format(TimeSpan offset) =>
            $"{(offset < TimeSpan.Zero ? '-' : '+')}{offset.Duration():hh\\:mm}";

        // Inspect the text before normalization: DateTimeOffset.Parse alone also
        // supplies a machine-local offset when none was present in the input.
        public static bool HasExplicitOffset(string text) =>
            text != null && Regex.IsMatch(text.Trim(), @"(?:[zZ]|[+-][0-9]{2}:[0-9]{2})$");

        public static TimeSpan? ReadExplicitOffset(string text) =>
            HasExplicitOffset(text) && DateTimeOffset.TryParse(text, out DateTimeOffset time)
                ? time.Offset : null;
    }
}
