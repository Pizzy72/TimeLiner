// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2025 Christian Pistor

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TimeLiner.Views
{
    /// <summary>
    /// The view of the timeline selector.
    /// </summary>
    public partial class TimeLineSelectorView : UserControl
    {
        /// <see cref="Color">
        private static readonly DependencyProperty _colorProperty = DependencyProperty.RegisterAttached(
            nameof(Color),
            typeof(Color),
            typeof(TimeLineSelectorView),
            new FrameworkPropertyMetadata(Color_Changed));

        /// <summary>
        /// Constructor.
        /// </summary>
        public TimeLineSelectorView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The color of the timeline selector.
        /// </summary>
        public Color Color
        {
            get => (Color)GetValue(_colorProperty);
            set => SetValue(_colorProperty, value);
        }

        /// <summary>
        /// Is called when the color has changed.
        /// </summary>
        /// <see cref="Color"/>
        private static void Color_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TimeLineSelectorView timeSelector)
            {
                Color oldColor = (Color)e.OldValue;
                Color newColor = (Color)e.NewValue;

                if (oldColor != newColor)
                {
                    timeSelector.Rectangle.Fill = new SolidColorBrush(newColor);
                }
            }
        }
    }
}
