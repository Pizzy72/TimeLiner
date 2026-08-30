// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2025 Christian Pistor

using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace TimeLiner.Converter
{
    /// <summary>
    /// Converts a multi-value into a Margin to be used by a multi-binding.
    /// </summary>
    internal class MarginConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Contains(DependencyProperty.UnsetValue))
            {
                return DependencyProperty.UnsetValue;
            }

            Thickness margin = new()
            {
                Left = (double)values[0],
                Top = (double)values[1],
                Right = (double)values[2],
                Bottom = (double)values[3]
            };

            return margin;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
