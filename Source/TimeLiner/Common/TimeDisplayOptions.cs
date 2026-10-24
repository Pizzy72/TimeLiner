// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Christian Pistor

using System;

namespace TimeLiner.Common
{
    /// <summary>Validated, mutually exclusive command-line display options.</summary>
    internal sealed class TimeDisplayOptions
    {
        public string TimeZoneId { get; private set; }
        public TimeSpan? Offset { get; private set; }
        public bool AutoOffset { get; private set; }

        public static TimeDisplayOptions Parse(string[] args)
        {
            TimeDisplayOptions result = new();
            bool found = false;
            for (int i = 0; i < args.Length; i++)
            {
                bool zoneOption = string.Equals(args[i], "-timezone", StringComparison.OrdinalIgnoreCase);
                bool offsetOption = string.Equals(args[i], "-offset", StringComparison.OrdinalIgnoreCase);
                if (!zoneOption && !offsetOption)
                    continue;
                if (found)
                    throw new ArgumentException("Specify only one of -timezone or -offset, once.");
                found = true;
                string option = args[i];
                if (++i >= args.Length)
                    throw new ArgumentException($"Missing value for {option} option.");
                string value = args[i];
                if (zoneOption)
                {
                    try
                    {
                        result.TimeZoneId = string.Equals(value, "local", StringComparison.OrdinalIgnoreCase)
                            ? TimeZoneInfo.Local.Id : TimeZoneInfo.FindSystemTimeZoneById(value).Id;
                    }
                    catch (TimeZoneNotFoundException)
                    {
                        throw new ArgumentException($"Invalid timezone id: '{value}'");
                    }
                }
                else if (string.Equals(value, "auto", StringComparison.OrdinalIgnoreCase))
                {
                    result.AutoOffset = true;
                }
                else if (UtcOffset.TryParse(value, out TimeSpan offset))
                {
                    result.Offset = offset;
                }
                else
                {
                    throw new ArgumentException($"Invalid UTC offset: '{value}'. Use ±HH:mm between -14:00 and +14:00, or auto.");
                }
            }
            return result;
        }
    }
}
