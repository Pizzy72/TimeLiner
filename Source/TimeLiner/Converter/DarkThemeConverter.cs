// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2025 Christian Pistor

using System;
using System.Globalization;
using System.Windows.Data;
using TimeLiner.Themes;

namespace TimeLiner.Converter
{
    /// <summary>
    /// Value converter for AppTheme / bool.
    /// </summary>
    public sealed class DarkThemeConverter : IValueConverter
    {
        /// <summary>
        /// Converts AppTheme to bool. Returns true if theme is Dark.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (AppTheme)value == AppTheme.Dark;
        }

        /// <summary>
        /// Converts bool back to AppTheme. Returns AppTheme.Dark if true, otherwise AppTheme.Light.
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? AppTheme.Dark : AppTheme.Light;
        }
    }
}
