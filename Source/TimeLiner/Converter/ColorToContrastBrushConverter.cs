// SPDX-License-Identifier: MIT
// Copyright (c) 2021-2026 Christian Pistor

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TimeLiner.Converter
{
    /// <summary>
    /// Selects black or white depending on which provides better contrast with a color.
    /// </summary>
    internal sealed class ColorToContrastBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Color color)
            {
                return Brushes.Black;
            }

            double luminance = 0.2126 * GetLinearComponent(color.R)
                + 0.7152 * GetLinearComponent(color.G)
                + 0.0722 * GetLinearComponent(color.B);
            double blackContrast = (luminance + 0.05) / 0.05;
            double whiteContrast = 1.05 / (luminance + 0.05);

            return blackContrast >= whiteContrast ? Brushes.Black : Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static double GetLinearComponent(byte component)
        {
            double value = component / 255d;
            return value <= 0.04045
                ? value / 12.92
                : Math.Pow((value + 0.055) / 1.055, 2.4);
        }
    }
}
