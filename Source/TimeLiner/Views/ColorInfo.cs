// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2026 Christian Pistor

using System.Windows.Media;
using TimeLiner.Converter;

namespace TimeLiner.Views
{
    /// <summary>
    /// Represents a color.
    /// </summary>
    internal class ColorInfo
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ColorInfo(string category, Color color)
        {
            Category = category;
            Color = color;
        }

        /// <summary>
        /// The color.
        /// </summary>
        public Color Color
        {
            get;
        }

        /// <summary>
        /// The category of the color (e.g., "Greens").
        /// </summary>
        public string Category
        {
            get;
        }

        /// <summary>
        /// The color as hex code.
        /// </summary>
        public string Hex => ColorToNameConverter.Convert(Color);
    }
}
