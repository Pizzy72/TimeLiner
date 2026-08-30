// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2025 Christian Pistor

using System;
using System.Windows;
using System.Windows.Controls;

namespace TimeLiner.Views
{
    /// <summary>
    /// The timescale view.
    /// </summary>
    public partial class TimeScaleView : UserControl
    {
        /// <see cref="LeftMargin"/>
        private static readonly DependencyProperty _leftMarginProperty = DependencyProperty.RegisterAttached(
            nameof(LeftMargin),
            typeof(double),
            typeof(TimeScaleView),
            new FrameworkPropertyMetadata(default(double), DependencyProperty_Changed)
            );

        /// <see cref="GridWidth"/>
        private static readonly DependencyProperty _gridWidthProperty = DependencyProperty.RegisterAttached(
            nameof(GridWidth),
            typeof(double),
            typeof(TimeScaleView),
            new FrameworkPropertyMetadata(100d, DependencyProperty_Changed)
            );

        /// <see cref="Interval"/>
        private static readonly DependencyProperty _intervalProperty = DependencyProperty.RegisterAttached(
            nameof(Interval),
            typeof(double),
            typeof(TimeScaleView),
            new FrameworkPropertyMetadata(1d, DependencyProperty_Changed)
            );

        /// <see cref="ScrollOffset"/>
        private static readonly DependencyProperty _scrollOffsetProperty = DependencyProperty.RegisterAttached(
            nameof(ScrollOffset),
            typeof(double),
            typeof(TimeScaleView),
            new FrameworkPropertyMetadata(default(double), DependencyProperty_Changed)
            );

        /// <see cref="Unit"/>
        private static readonly DependencyProperty _unitProperty = DependencyProperty.RegisterAttached(
            nameof(Unit),
            typeof(string),
            typeof(TimeScaleView),
            new FrameworkPropertyMetadata(default(string), DependencyProperty_Changed)
            );

        /// <see cref="VisibleWidth"/>
        private static readonly DependencyProperty _visibleWidthProperty = DependencyProperty.RegisterAttached(
            nameof(VisibleWidth),
            typeof(double),
            typeof(TimeScaleView),
            new FrameworkPropertyMetadata(default(double), DependencyProperty_Changed)
            );

        /// <summary>
        /// The offset of the scale text.
        /// </summary>
        private const double ScaleTextOffset = 10d;

        /// <summary>
        /// The stack panel which holds the ticks of the time scale.
        /// </summary>
        private readonly StackPanel _stackPanel;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TimeScaleView()
        {
            InitializeComponent();

            _stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            TimeScale.Children.Add(_stackPanel);
        }

        /// <summary>
        /// The left position.
        /// </summary>
        public double LeftMargin
        {
            get => (double)GetValue(_leftMarginProperty);
            set => SetValue(_leftMarginProperty, value);
        }

        /// <summary>
        /// The grid width.
        /// </summary>
        public double GridWidth
        {
            get => (double)GetValue(_gridWidthProperty);
            set => SetValue(_gridWidthProperty, value);
        }

        /// <summary>
        /// The interval value.
        /// </summary>
        public double Interval
        {
            get => (double)GetValue(_intervalProperty);
            set => SetValue(_intervalProperty, value);
        }

        /// <summary>
        /// The scroll offset.
        /// </summary>
        public double ScrollOffset
        {
            get => (double)GetValue(_scrollOffsetProperty);
            set => SetValue(_scrollOffsetProperty, value);
        }

        /// <summary>
        /// The interval unit.
        /// </summary>
        public string Unit
        {
            get => (string)GetValue(_unitProperty);
            set => SetValue(_unitProperty, value);
        }

        /// <summary>
        /// The visible width of the time scale.
        /// </summary>
        public double VisibleWidth
        {
            get => (double)GetValue(_visibleWidthProperty);
            set => SetValue(_visibleWidthProperty, value);
        }

        /// <summary>
        /// Is called when a dependency property has changed.
        /// </summary>
        private static void DependencyProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != e.NewValue)
            {
                ((TimeScaleView)d).DrawScale();
            }
        }

        /// <summary>
        /// Draw the time scale for the visible time span.
        /// </summary>
        private void DrawScale()
        {
            _stackPanel.Children.Clear();

            SetLeftMargin();

            long firstTick = (long)(ScrollOffset / GridWidth);
            long numberOfTicks = (long)(VisibleWidth / GridWidth);
            long lastTick = firstTick + numberOfTicks;

            for (long currentTick = firstTick; currentTick <= lastTick; currentTick++)
            {
                DrawTick(currentTick);
            }
        }

        /// <summary>
        /// Set left margin of time scale to align ticks with grid.
        /// </summary>
        private void SetLeftMargin()
        {
            double left = -(ScrollOffset % GridWidth) - ScaleTextOffset + LeftMargin;
            Margin = new Thickness(left, 0d, 0d, 0d);
        }

        /// <summary>
        /// Draw one tick.
        /// </summary>
        private void DrawTick(long tick)
        {
            double value = Math.Round(tick * Interval, 0);
            string text = $"{value} {Unit}";

            TextBlock textBox = new()
            {
                Width = GridWidth,
                Height = Height,
                Text = text,
                FontSize = 10d
            };

            _stackPanel.Children.Add(textBox);
        }
    }
}
