// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2026 Christian Pistor

using System;
using System.Text.Json.Serialization;
using TimeLiner.Themes;

namespace TimeLiner.Models
{
    /// <summary>
    /// Represents the persistent TimeLiner settings.
    /// </summary>
    public sealed class SettingsModel
    {
        /// <summary>
        /// Gets or sets a value indicating whether times are displayed
        /// in Coordinated Universal Time (UTC).
        /// </summary>
        public bool IsUniversalTime { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether a compact time grid is used.
        /// </summary>
        public bool IsCompactTimeGrid { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether a compact timeline height is used.
        /// </summary>
        public bool IsCompactTimeLines { get; set; } = false;
        
        /// <summary>
        /// Gets or sets a value indicating whether the time locators are locked.
        /// </summary>
        public bool IsTimeLocatorLocked { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether timeline item names are visible.
        /// </summary>
        public bool IsNameVisible { get; set; } = true;

        /// <summary>
        /// Gets or sets the time zone identifier used for local time display.
        /// </summary>
        /// <remarks>
        public string TimeZone { get; set; } =
            TimeZoneInfo.FindSystemTimeZoneById(TimeZoneInfo.Local.Id).Id;

        /// <summary>
        /// Gets or sets the application theme.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AppTheme Theme { get; set; } = AppTheme.Light;

        /// <summary>
        /// Gets or sets the main window settings (width, height, position, state).
        /// </summary>
        public WindowSettingsModel MainWindowSettings { get; set; } = new WindowSettingsModel();
    }
}
