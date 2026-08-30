// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2025 Christian Pistor

using System;
using System.Collections.Generic;
using System.Windows.Data;
using System.Windows.Media;
using TimeLiner.ViewModels;

namespace TimeLiner.Converter
{
    /// <summary>
    /// Converts the color for a timeline item into a solid color brush.
    /// Caches and freezes all used color brushes as performance optimization.
    /// </summary>
    internal class ColorToBrushConverter : IValueConverter
    {
        /// <summary>
        /// Reference to the global settings view model.
        /// </summary>
        private readonly SettingsViewModel _settingsViewModel = AppServices.Settings;

        private static readonly Dictionary<Color, Brush> _brushes = [];

        public object Convert(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture
            )
        {
            if (value is Color color)
            {
                if (!_brushes.TryGetValue(color, out Brush brush))
                {
                    brush = new SolidColorBrush(color)
                    {
                        Opacity = _settingsViewModel.TimeLineItemOpacity // performance
                    };

                    brush.Freeze(); // performance

                    _brushes[color] = brush; // performance
                }

                return brush;
            }

            return value;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture
            )
        {
            throw new NotImplementedException();
        }
    }
}
