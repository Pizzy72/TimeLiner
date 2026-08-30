// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2025 Christian Pistor

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using TimeLiner.ViewModels;

namespace TimeLiner.Views
{
    /// <summary>
    /// The time locator view.
    /// </summary>
    public partial class TimeLocatorView : UserControl
    {
        /// <see cref="X">
        private static readonly DependencyProperty _xProperty = DependencyProperty.RegisterAttached(
            nameof(X),
            typeof(double),
            typeof(TimeLocatorView),
            new FrameworkPropertyMetadata(X_Changed));

        /// <see cref="Color">
        private static readonly DependencyProperty _colorProperty = DependencyProperty.RegisterAttached(
            nameof(Color),
            typeof(Color),
            typeof(TimeLocatorView),
            new FrameworkPropertyMetadata(Color_Changed));

        /// <summary>
        /// Constructor.
        /// </summary>
        public TimeLocatorView()
        {
            InitializeComponent();

            Line.SnapsToDevicePixels = true;
            Line.SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Unspecified);
        }

        /// <summary>
        /// The color of the time locator.
        /// </summary>
        public Color Color
        {
            get => (Color)GetValue(_colorProperty);
            set => SetValue(_colorProperty, value);
        }

        /// <summary>
        /// The x-coordinate of the time locator.
        /// </summary>
        public double X
        {
            get => (double)GetValue(_xProperty);
            set => SetValue(_xProperty, value);
        }

        /// <summary>
        /// Is called when the color has changed.
        /// </summary>
        /// <see cref="Color"/>
        private static void Color_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TimeLocatorView timeLocator)
            {
                Color oldColor = (Color)e.OldValue;
                Color newColor = (Color)e.NewValue;

                if (oldColor != newColor)
                {
                    timeLocator.Line.Stroke = new SolidColorBrush(newColor);
                }
            }
        }

        /// <summary>
        /// Is called when the x-coordinate has changed.
        /// </summary>
        /// <see cref="X"/>
        private static void X_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TimeLocatorView timeLocatorView)
            {
                double oldX = (double)e.OldValue;
                double newX = (double)e.NewValue;

                if (oldX != newX)
                {
                    // Set horizontal position of time locator.
                    Canvas.SetLeft(timeLocatorView.Line, Math.Max(0d, newX));
                }
            }
        }

        /// <summary>
        /// Is called when the mouse pointer moves while the mouse pointer is over the time locator.
        /// </summary>
        private void Line_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.ScrollWE;
        }

        /// <summary>
        /// Is called when the time locator is clicked with the left mouse button.
        /// </summary>
        private void TimeLocator_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CaptureMouse();
        }

        /// <summary>
        /// Is called when the time locator is moved with the mouse.
        /// </summary>
        private void TimeLocator_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (IsMouseCaptured)
            {
                TimeLocatorView timeLocatorView = (TimeLocatorView)e.OriginalSource;
                X = Math.Min(Math.Max(e.GetPosition(this).X, 0d), timeLocatorView.ActualWidth);
            }
        }

        /// <summary>
        /// Is called when the left mouse button is released on the time locator.
        /// </summary>
        private void TimeLocator_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            ReleaseMouseCapture();
        }

        /// <summary>
        /// Is called when the data context has changed.
        /// </summary>
        private void TimeLocator_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Bind TimeLocatorViewModel.X to TimeLocatorView.X

            Binding binding = new()
            {
                Source = DataContext,
                Path = new PropertyPath(nameof(TimeLocatorViewModel.X)),
                Mode = BindingMode.TwoWay
            };

            SetBinding(_xProperty, binding);
        }
    }
}
