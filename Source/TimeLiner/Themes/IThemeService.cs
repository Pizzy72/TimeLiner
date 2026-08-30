// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2025 Christian Pistor

namespace TimeLiner.Themes
{
    /// <summary>
    /// Interface for theme switching services.
    /// </summary>
    public interface IThemeService
    {
        /// <summary>
        /// Gets the currently applied theme, or null if none is set.
        /// </summary>
        AppTheme? CurrentTheme { get; }

        /// <summary>
        /// Applies the specified theme to the application.
        /// </summary>
        /// <param name="theme">The theme to apply.</param>
        void ApplyTheme(AppTheme theme);
    }

}
