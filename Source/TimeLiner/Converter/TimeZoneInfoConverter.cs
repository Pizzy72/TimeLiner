// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2025 Christian Pistor

using System;
using System.Globalization;
using System.Windows.Data;

namespace TimeLiner.Converter
{
    /// <summary>
    /// Value converter for TimeZoneInfo.
    /// </summary>
    internal class TimeZoneInfoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string displayName)
            {
                // Keep the region visible. The base offset alone misleadingly
                // suggests a fixed offset, although daylight-saving rules apply.
                int pos = displayName.IndexOf(')');
                return pos >= 0 ? displayName[(pos + 1)..].Trim() : displayName;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
