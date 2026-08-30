// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2025 Christian Pistor

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TimeLiner.Converter
{
    /// <summary>
    /// Converts a multi-value into a Rect to be used by a multi-binding.
    /// </summary>
    internal class RectConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double x = (double)values[0];
            double y = (double)values[1];
            double width = (double)values[2];
            double height = (double)values[3];

            return new Rect(x, y, width, height);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
