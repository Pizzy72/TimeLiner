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
                // Cut off text after UTC offset:
                // "(UTC-12:00) International Date Line West" --> "(UTC-12:00)"
                int pos = displayName.IndexOf(')') + 1;
                return displayName.Substring(0, pos);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}