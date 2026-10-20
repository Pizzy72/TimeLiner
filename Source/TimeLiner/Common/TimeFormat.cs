// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2025 Christian Pistor

using System;
using System.Text;

namespace TimeLiner.Common
{
    /// <summary>
    /// Provides formatting methods for time and duration.
    /// </summary>
    /// <remarks>
    /// https://blog.submain.com/4-common-datetime-mistakes-c-avoid/
    /// https://stackoverflow.com/questions/1083955/how-to-get-difference-between-two-dates-in-year-month-week-day
    /// https://beginnersbook.com/2013/04/calculating-day-given-date/
    /// https://www.calculator.net/date-calculator.html
    /// </remarks>
    internal static class TimeFormat
    {
        public static string GetDurationString(TimeSpan duration)
        {
            StringBuilder durationString = new();

            if (Math.Abs(duration.TotalDays) >= 7)
            {
                durationString.Append(GetWeeksAndDaysString(duration));
            }
            else if (Math.Abs(duration.TotalDays) >= 1)
            {
                durationString.Append(GetDaysString(duration));

                if (Math.Abs(duration.Hours) > 0)
                {
                    durationString.Append(" ");
                    durationString.Append(GetHoursString(duration));
                }
            }
            else if (Math.Abs(duration.TotalHours) >= 1)
            {
                durationString.Append(GetHoursString(duration));

                if (Math.Abs(duration.Minutes) > 0)
                {
                    durationString.Append(" ");
                    durationString.Append(GetMinutesString(duration));
                }
            }
            else if (Math.Abs(duration.TotalMinutes) >= 1)
            {
                durationString.Append(GetMinutesString(duration));

                if (Math.Abs(duration.Seconds) > 0)
                {
                    durationString.Append(" ");
                    durationString.Append(GetSecondsString(duration));
                }
            }
            else if (Math.Abs(duration.TotalSeconds) >= 1)
            {
                if (Math.Abs(duration.Milliseconds) > 0)
                {
                    durationString.Append(GetSecondsAndMillisecondsString(duration));
                }
                else
                {
                    durationString.Append(GetSecondsString(duration));
                }
            }
            else if (Math.Abs(duration.TotalMilliseconds) >= 1)
            {
                durationString.Append(GetMillisecondsString(duration));
            }
            else
            {
                durationString.Append(GetSecondsString(duration));
            }

            return durationString.ToString();
        }

        public static string GetTimeString(DateTime utcTime, ITimeZoneInfoProvider timeZoneInfoProvider)
        {
            if (timeZoneInfoProvider.IsUniversalTime)
            {
                return $"{utcTime:yyyy-MM-dd HH:mm:ss.fff} Z";
            }

            DateTimeOffset instant = new(utcTime.ToUniversalTime());
            if (timeZoneInfoProvider.DisplayMode == TimeDisplayMode.FixedOffset)
            {
                long displayedTicks = instant.UtcTicks + timeZoneInfoProvider.UtcOffset.Ticks;
                if (displayedTicks < DateTime.MinValue.Ticks || displayedTicks > DateTime.MaxValue.Ticks)
                    return "Outside supported date range";
            }
            DateTimeOffset displayedTime = timeZoneInfoProvider.DisplayMode == TimeDisplayMode.FixedOffset
                ? instant.ToOffset(timeZoneInfoProvider.UtcOffset)
                : TimeZoneInfo.ConvertTime(instant, TimeZoneInfo.FindSystemTimeZoneById(timeZoneInfoProvider.TimeZone));

            return $"{displayedTime:yyyy-MM-dd HH:mm:ss.fff} {UtcOffset.Format(displayedTime.Offset)}";
        }

        /// <summary>
        /// Explicit offsets always win. Offset-free editor input uses the selected
        /// display context, never the operating system's implicit local zone.
        /// </summary>
        public static bool TryParseUtcTime(string text, ITimeZoneInfoProvider provider, out DateTime utcTime)
        {
            utcTime = default;
            if (UtcOffset.HasExplicitOffset(text))
            {
                if (!DateTimeOffset.TryParse(text, out DateTimeOffset explicitTime))
                    return false;
                utcTime = explicitTime.UtcDateTime;
                return true;
            }

            if (!DateTime.TryParse(text, out DateTime parsed))
                return false;

            DateTime wallTime = DateTime.SpecifyKind(parsed, DateTimeKind.Unspecified);
            try
            {
                if (provider.DisplayMode == TimeDisplayMode.FixedOffset)
                {
                    utcTime = new DateTimeOffset(wallTime, provider.UtcOffset).UtcDateTime;
                }
                else
                {
                    TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById(provider.TimeZone);
                    // The repeated hour needs an explicit offset to be unambiguous.
                    if (zone.IsInvalidTime(wallTime) || zone.IsAmbiguousTime(wallTime))
                        return false;
                    utcTime = TimeZoneInfo.ConvertTimeToUtc(wallTime, zone);
                }
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        private static string GetWeeksAndDaysString(TimeSpan duration)
        {
            StringBuilder durationString = new();

            int weeks = (int)Math.Abs(duration.Days / TimeConstants.WeekDays);
            int days = (int)Math.Abs(duration.Days / TimeConstants.WeekDays);

            durationString.Append($"{weeks} {TimeConstants.UnitName.Week}");

            if (days > 0)
            {
                durationString.Append($" {days} {TimeConstants.UnitName.Day}");
            }

            return durationString.ToString();
        }

        private static string GetDaysString(TimeSpan duration)
        {
            return $"{Math.Abs(duration.Days)} {TimeConstants.UnitName.Day}";
        }

        private static string GetHoursString(TimeSpan duration)
        {
            return $"{Math.Abs(duration.Hours)} {TimeConstants.UnitName.Hour}";
        }

        private static string GetMinutesString(TimeSpan duration)
        {
            return $"{Math.Abs(duration.Minutes)} {TimeConstants.UnitName.Minute}";
        }

        private static string GetSecondsString(TimeSpan duration)
        {
            return $"{Math.Abs(duration.Seconds)} {TimeConstants.UnitName.Second}";
        }

        private static string GetSecondsAndMillisecondsString(TimeSpan duration)
        {
            return $"{Math.Abs(duration.Seconds)}.{Math.Abs(duration.Milliseconds)} {TimeConstants.UnitName.Second}";
        }

        private static string GetMillisecondsString(TimeSpan duration)
        {
            return $"{Math.Abs(duration.Milliseconds)} {TimeConstants.UnitName.Millisecond}";
        }
    }
}
