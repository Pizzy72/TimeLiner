// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2025 Christian Pistor

using System;
using System.Windows;
using TimeLiner.Models;

namespace TimeLiner.UI
{
    /// <summary>
    /// Helper class to apply and capture window size, position and state.
    /// </summary>
    internal static class WindowSettingsHelper
    {
        /// <summary>
        /// Applies the persisted window settings to the given window.
        /// </summary>
        public static void Apply(Window window, WindowSettingsModel settings)
        {
            if (settings.Width > 0 && settings.Height > 0)
            {
                // Geometry changes are ignored while maximized, so we must switch to Normal first.
                window.WindowState = WindowState.Normal;

                // Avoid sub-pixel values caused by DPI scaling.
                window.Top = Math.Round(settings.Top);
                window.Left = Math.Round(settings.Left);
                window.Width = Math.Round(settings.Width);
                window.Height = Math.Round(settings.Height);
            }

            // Restore the maximized state after geometry was applied.
            if (settings.WindowState == WindowState.Maximized)
            {
                window.WindowState = WindowState.Maximized;
            }
        }

        /// <summary>
        /// Captures the current window geometry and state into the given settings object.
        /// </summary>
        public static void Capture(Window window, WindowSettingsModel settings)
        {
            bool isMaximized = window.WindowState == WindowState.Maximized;

            // Use RestoreBounds when maximized, otherwise the current bounds.
            Rect bounds = isMaximized
                ? window.RestoreBounds
                : new Rect(
                    window.Left,
                    window.Top,
                    window.Width,
                    window.Height);

            // Do not overwrite existing settings with invalid geometry values.
            if (bounds.Width <= 0 || bounds.Height <= 0 ||
                double.IsNaN(bounds.Width) || double.IsNaN(bounds.Height) ||
                double.IsInfinity(bounds.Width) || double.IsInfinity(bounds.Height))
            {
                return;
            }

            settings.Top = bounds.Top;
            settings.Left = bounds.Left;
            settings.Width = bounds.Width;
            settings.Height = bounds.Height;

            // Persist maximized state separately from geometry.
            settings.WindowState = isMaximized
                ? WindowState.Maximized
                : WindowState.Normal;
        }
    }
}
