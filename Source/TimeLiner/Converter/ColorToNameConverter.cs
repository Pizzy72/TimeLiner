// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2026 Christian Pistor

using System;
using System.Windows.Data;
using System.Windows.Media;

namespace TimeLiner.Converter
{
    /// <summary>
    /// Converts a color into a color name.
    /// </summary>
    internal class ColorToNameConverter : IValueConverter
    {
        public static string Convert(Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is Color color)
            {
                return Convert(color);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
