// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2025 Christian Pistor

using System.Text.Json.Serialization;
using System.Windows;

namespace TimeLiner.Models
{
    /// <summary>
    /// Represents persisted window size, position and state.
    /// 
    /// The stored geometry always reflects the last normal (non-maximized)
    /// window bounds. The maximized state is persisted separately.
    /// </summary>
    public sealed class WindowSettingsModel
    {
        /// <summary>
        /// Gets or sets the vertical position of the window (top edge).
        /// Coordinates are in device-independent pixels (DIP).
        /// </summary>
        public double Top { get; set; } = 0;

        /// <summary>
        /// Gets or sets the horizontal position of the window (left edge).
        /// Coordinates are in device-independent pixels (DIP).
        /// </summary>
        public double Left { get; set; } = 0;

        /// <summary>
        /// Gets or sets the width of the window in device-independent pixels (DIP).
        /// This value represents the last normal window width.
        /// </summary>
        public double Width { get; set; } = 1200;

        /// <summary>
        /// Gets or sets the height of the window in device-independent pixels (DIP).
        /// This value represents the last normal window height.
        /// </summary>
        public double Height { get; set; } = 800;

        /// <summary>
        /// Gets or sets the window state.
        /// 
        /// The value <see cref="WindowState.Maximized"/> is treated as a state flag
        /// and does not affect the persisted window geometry.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public WindowState WindowState { get; set; } = WindowState.Normal;
    }
}
