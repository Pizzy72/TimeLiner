// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2025 Christian Pistor

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

        private readonly TranslateTransform _scrollTransform = new();
        private long? _firstTick;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TimeScaleView()
        {
            InitializeComponent();

            _stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                RenderTransform = _scrollTransform
            };

            TimeScale.Children.Add(_stackPanel);
            DrawScale(true);
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
            ((TimeScaleView)d).DrawScale(
                e.Property != _scrollOffsetProperty && e.Property != _leftMarginProperty);
        }

        /// <summary>
        /// Draw the time scale for the visible time span.
        /// </summary>
        private void DrawScale(bool refreshTickContent)
        {
            // Bindings can change while InitializeComponent is still running.
            if (_stackPanel == null)
                return;

            if (!double.IsFinite(GridWidth) || GridWidth <= 0
                || !double.IsFinite(VisibleWidth) || VisibleWidth < 0
                || !double.IsFinite(ScrollOffset))
                return;

            // A render translation keeps fractional scrolling out of layout.
            _scrollTransform.X = -(ScrollOffset % GridWidth) - ScaleTextOffset + LeftMargin;

            long firstTick = (long)(ScrollOffset / GridWidth);
            int tickCount = (int)(VisibleWidth / GridWidth) + 1;

            if (!refreshTickContent && _firstTick == firstTick
                && _stackPanel.Children.Count == tickCount)
                return;

            // Resize only at viewport/zoom changes; scrolling reuses every label.
            while (_stackPanel.Children.Count > tickCount)
                _stackPanel.Children.RemoveAt(_stackPanel.Children.Count - 1);
            while (_stackPanel.Children.Count < tickCount)
                _stackPanel.Children.Add(new TextBlock { FontSize = 10d });

            for (int i = 0; i < tickCount; i++)
            {
                TextBlock textBlock = (TextBlock)_stackPanel.Children[i];
                textBlock.Width = GridWidth;
                textBlock.Height = Height;
                textBlock.Text = $"{Math.Round((firstTick + i) * Interval, 0)} {Unit}";
            }
            _firstTick = firstTick;
        }
    }
}
