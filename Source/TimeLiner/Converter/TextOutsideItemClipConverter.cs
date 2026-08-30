// SPDX-License-Identifier: MIT
// Copyright (c) 2021-2026 Christian Pistor

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace TimeLiner.Converter
{
    /// <summary>
    /// Clips timeline text to the area beyond its colored item.
    /// </summary>
    internal sealed class TextOutsideItemClipConverter : IMultiValueConverter
    {
        private const double TextOffset = 4d;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 3
                || values[0] is not double itemWidth
                || values[1] is not double textWidth
                || values[2] is not double textHeight)
            {
                return Geometry.Empty;
            }

            double clipLeft = Math.Max(0, itemWidth - TextOffset);
            double clipWidth = Math.Max(0, textWidth - clipLeft);
            return clipWidth > 0 && textHeight > 0
                ? new RectangleGeometry(new Rect(clipLeft, 0, clipWidth, textHeight))
                : Geometry.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
