// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2025 Christian Pistor

using ControlzEx.Theming;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace TimeLiner.Themes
{
    /// <summary>
    /// Provides theme switching functionality for the application.
    /// </summary>
    internal sealed class ThemeService : IThemeService
    {
        /// <summary>
        /// Cache for loaded theme resource dictionaries.
        /// </summary>
        private readonly Dictionary<AppTheme, ResourceDictionary> _themeCache = [];

        /// <summary>
        /// Gets the currently applied theme, or null if none is set.
        /// </summary>
        public AppTheme? CurrentTheme
        {
            get;
            private set;
        }

        /// <summary>
        /// Applies the specified theme to the application.
        /// </summary>
        /// <param name="theme">The theme to apply.</param>
        public void ApplyTheme(AppTheme theme)
        {
            if (CurrentTheme == theme)
            {
                return;
            }

            Collection<ResourceDictionary> dictionaries = Application.Current.Resources.MergedDictionaries;

            ThemeManager.Current.ChangeTheme(Application.Current, theme.ToString(), "Blue");

            // Load or get cached theme dictionary
            if (!_themeCache.TryGetValue(theme, out ResourceDictionary newTheme))
            {
                newTheme = new ResourceDictionary
                {
                    Source = new Uri($"Themes/Resources/{theme}Theme.xaml", UriKind.Relative)
                };
                _themeCache[theme] = newTheme;
            }

            // Find existing application theme dictionary
            for (int i = dictionaries.Count - 1; i >= 0; i--)
            {
                if (dictionaries[i].Contains("IsApplicationTheme"))
                {
                    // Keep the application theme after Fluent.Ribbon's dictionaries so
                    // our theme-specific brushes win after a runtime theme switch.
                    dictionaries.RemoveAt(i);
                    dictionaries.Add(newTheme);
                    CurrentTheme = theme;
                    return;
                }
            }

            // No theme applied yet (startup case)
            dictionaries.Add(newTheme);
            CurrentTheme = theme;
        }
    }
}
