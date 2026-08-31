// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2026 Christian Pistor

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace TimeLiner.Views
{

    // Provides attached properties for timeline item text layout.
    //
    // This behavior automatically limits the width of a TextBlock so that
    // single-line text may extend beyond its own timeline item, but stops
    // before the next visual obstacle on the same timeline.
    //
    // Usage:
    // - Mark the shared timeline Canvas with IsTimelineHost.
    // - Mark visual elements that should stop text flow with IsTextObstacle.
    // - Enable automatic width calculation on the TextBlock with EnableAutoWidth.
    //
    // The behavior works in view coordinates and is independent of the
    // timeline item view model. It uses transformed visual bounds so that
    // translated or rotated obstacles, such as time event markers, are
    // handled correctly.
    public static class TimelineItemTextBehavior
    {
        public static readonly DependencyProperty EnableAutoWidthProperty =
            DependencyProperty.RegisterAttached(
                "EnableAutoWidth",
                typeof(bool),
                typeof(TimelineItemTextBehavior),
                new PropertyMetadata(false, OnEnableAutoWidthChanged));

        public static void SetEnableAutoWidth(DependencyObject element, bool value)
            => element.SetValue(EnableAutoWidthProperty, value);

        public static bool GetEnableAutoWidth(DependencyObject element)
            => (bool)element.GetValue(EnableAutoWidthProperty);


        public static readonly DependencyProperty RightPaddingProperty =
            DependencyProperty.RegisterAttached(
                "RightPadding",
                typeof(double),
                typeof(TimelineItemTextBehavior),
                new PropertyMetadata(2.0, OnWidthRelevantPropertyChanged));

        public static void SetRightPadding(DependencyObject element, double value)
            => element.SetValue(RightPaddingProperty, value);

        public static double GetRightPadding(DependencyObject element)
            => (double)element.GetValue(RightPaddingProperty);


        public static readonly DependencyProperty IsTimelineHostProperty =
            DependencyProperty.RegisterAttached(
                "IsTimelineHost",
                typeof(bool),
                typeof(TimelineItemTextBehavior),
                new PropertyMetadata(false));

        public static void SetIsTimelineHost(DependencyObject element, bool value)
            => element.SetValue(IsTimelineHostProperty, value);

        public static bool GetIsTimelineHost(DependencyObject element)
            => (bool)element.GetValue(IsTimelineHostProperty);


        public static readonly DependencyProperty IsTextObstacleProperty =
            DependencyProperty.RegisterAttached(
                "IsTextObstacle",
                typeof(bool),
                typeof(TimelineItemTextBehavior),
                new PropertyMetadata(false));

        public static void SetIsTextObstacle(DependencyObject element, bool value)
            => element.SetValue(IsTextObstacleProperty, value);

        public static bool GetIsTextObstacle(DependencyObject element)
            => (bool)element.GetValue(IsTextObstacleProperty);


        public static readonly DependencyProperty IsTextAnchorProperty =
            DependencyProperty.RegisterAttached(
                "IsTextAnchor",
                typeof(bool),
                typeof(TimelineItemTextBehavior),
                new PropertyMetadata(false, OnIsTextAnchorChanged));

        public static void SetIsTextAnchor(DependencyObject element, bool value)
            => element.SetValue(IsTextAnchorProperty, value);

        public static bool GetIsTextAnchor(DependencyObject element)
            => (bool)element.GetValue(IsTextAnchorProperty);

        private static readonly DependencyPropertyDescriptor CanvasLeftDescriptor =
            DependencyPropertyDescriptor.FromProperty(
                Canvas.LeftProperty,
                typeof(FrameworkElement));

        private static void OnIsTextAnchorChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is not FrameworkElement anchor)
                return;

            CanvasLeftDescriptor.RemoveValueChanged(anchor, OnTextAnchorLeftChanged);

            if ((bool)e.NewValue)
                CanvasLeftDescriptor.AddValueChanged(anchor, OnTextAnchorLeftChanged);
        }

        private static void OnTextAnchorLeftChanged(object sender, EventArgs e)
        {
            if (sender is not FrameworkElement anchor)
                return;

            FrameworkElement host = FindTimelineHost(anchor);

            if (host == null)
                return;

            foreach (TextBlock textBlock in FindVisualChildren<TextBlock>(host)
                .Where(GetEnableAutoWidth))
            {
                ScheduleUpdate(textBlock);
            }
        }

        private static void OnEnableAutoWidthChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is not TextBlock textBlock)
                return;

            if ((bool)e.NewValue)
            {
                textBlock.Loaded += OnTextBlockLoaded;
                textBlock.Unloaded += OnTextBlockUnloaded;
                textBlock.LayoutUpdated += OnTextBlockLayoutUpdated;
                textBlock.IsVisibleChanged += OnTextBlockIsVisibleChanged;

                ScheduleUpdate(textBlock);
            }
            else
            {
                textBlock.Loaded -= OnTextBlockLoaded;
                textBlock.Unloaded -= OnTextBlockUnloaded;
                textBlock.LayoutUpdated -= OnTextBlockLayoutUpdated;
                textBlock.IsVisibleChanged -= OnTextBlockIsVisibleChanged;

                textBlock.ClearValue(FrameworkElement.WidthProperty);
            }
        }

        private static void OnWidthRelevantPropertyChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e
            )
        {
            if (d is TextBlock textBlock)
                ScheduleUpdate(textBlock);
        }

        private static void OnTextBlockLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBlock textBlock)
            {
                // A CollectionView refresh can unload and reload item visuals
                // while an item is being dragged. Unloaded detaches these
                // handlers, so restore them when the same TextBlock returns.
                textBlock.LayoutUpdated -= OnTextBlockLayoutUpdated;
                textBlock.LayoutUpdated += OnTextBlockLayoutUpdated;
                textBlock.IsVisibleChanged -= OnTextBlockIsVisibleChanged;
                textBlock.IsVisibleChanged += OnTextBlockIsVisibleChanged;

                ScheduleUpdate(textBlock);
            }
        }

        private static void OnTextBlockUnloaded(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBlock textBlock)
                return;

            textBlock.LayoutUpdated -= OnTextBlockLayoutUpdated;
            textBlock.IsVisibleChanged -= OnTextBlockIsVisibleChanged;
        }

        private static void OnTextBlockLayoutUpdated(object sender, EventArgs e)
        {
            if (sender is TextBlock textBlock)
                UpdateWidth(textBlock);
        }

        private static void OnTextBlockIsVisibleChanged(
            object sender,
            DependencyPropertyChangedEventArgs e)
        {
            if (sender is TextBlock textBlock)
                ScheduleUpdate(textBlock);
        }

        private static void ScheduleUpdate(TextBlock textBlock)
        {
            if (!textBlock.IsLoaded)
                return;

            textBlock.Dispatcher.BeginInvoke(
                DispatcherPriority.Loaded,
                new Action(() => UpdateWidth(textBlock)));
        }

        private static void UpdateWidth(TextBlock textBlock)
        {
            if (!GetEnableAutoWidth(textBlock))
                return;

            if (!textBlock.IsLoaded || !textBlock.IsVisible)
                return;

            FrameworkElement host = FindTimelineHost(textBlock);

            if (host == null)
                return;

            if (host.ActualWidth <= 0)
                return;

            double? textLeftValue = GetLeftRelativeTo(textBlock, host);

            if (!textLeftValue.HasValue)
                return;

            // The origin remains available when a previous collision reduced
            // the TextBlock width to zero. This allows the text to become
            // visible again when the following item is moved farther right.
            double textLeft = textLeftValue.Value;
            double rightPadding = GetRightPadding(textBlock);
            object textDataContext = textBlock.DataContext;

            IEnumerable<FrameworkElement> visualChildren =
                FindVisualChildren<FrameworkElement>(host);

            List<double> obstacleLefts = visualChildren
                .Where(x => x != textBlock)
                .Where(GetIsTextObstacle)
                .Where(x => x.IsVisible)
                .Where(x => !ReferenceEquals(x.DataContext, textDataContext))
                .Select(x => GetBoundsRelativeTo(x, host))
                .Where(x => !x.IsEmpty)

                // Only obstacles that start to the right of the text can limit its width.
                // Obstacles that already overlap the text from the left are ignored here,
                // because otherwise the calculated width could become 0.
                .Where(x => x.Left > textLeft + 0.5)
                .Select(x => x.Left)
                .ToList();

            // Canvas-based item presenters can have an ActualHeight of zero.
            // Their origin is nevertheless valid and, unlike the rotated
            // marker bounds, is already available during the first layout
            // pass. Use that origin as a stable collision boundary.
            List<(object DataContext, double Left)> anchors = visualChildren
                .Where(GetIsTextAnchor)
                .Where(x => x.IsVisible)
                .Select(x => (x.DataContext, Left: GetLeftRelativeTo(x, host)))
                .Where(x => x.Left.HasValue)
                .Select(x => (x.DataContext, x.Left.Value))
                .ToList();

            double? ownAnchorLeft = anchors
                .Where(x => ReferenceEquals(x.DataContext, textDataContext))
                .Select(x => (double?)x.Left)
                .FirstOrDefault();

            if (ownAnchorLeft.HasValue)
            {
                // Compare anchors with the current item's position rather than
                // the label's position. With tightly packed event markers the
                // next marker can already be left of the current label start;
                // that correctly leaves no room for the current label.
                obstacleLefts.AddRange(
                    anchors
                        .Where(x => !ReferenceEquals(x.DataContext, textDataContext))
                        .Select(x => x.Left)
                        .Where(x => x > ownAnchorLeft.Value + 0.5));
            }

            if (obstacleLefts.Count == 0)
            {
                // No obstacle to the right:
                // Let the TextBlock use its natural width.
                if (!double.IsNaN(textBlock.Width))
                    textBlock.Width = double.NaN;

                return;
            }

            double nextObstacleLeft = obstacleLefts.Min();

            double availableWidth = Math.Max(0, nextObstacleLeft - textLeft - rightPadding);
            double desiredWidth = GetDesiredTextWidth(textBlock);

            // There is enough room for the complete text:
            // Let the TextBlock use its natural width, so the tooltip hit area
            // does not extend into the empty timeline area.
            if (availableWidth >= desiredWidth - 0.5)
            {
                if (!double.IsNaN(textBlock.Width))
                    textBlock.Width = double.NaN;

                return;
            }

            // There is not enough room:
            // Limit the TextBlock width so CharacterEllipsis can be applied.
            double newWidth = availableWidth;

            if (double.IsNaN(textBlock.Width) || Math.Abs(textBlock.Width - newWidth) > 0.5)
                textBlock.Width = newWidth;
        }

        private static double GetDesiredTextWidth(TextBlock textBlock)
        {
            string text = textBlock.Text ?? string.Empty;

            if (string.IsNullOrEmpty(text))
                return 0d;

            DpiScale dpi = VisualTreeHelper.GetDpi(textBlock);

            FormattedText formattedText = new(
                text,
                System.Globalization.CultureInfo.CurrentUICulture,
                textBlock.FlowDirection,
                new Typeface(
                    textBlock.FontFamily,
                    textBlock.FontStyle,
                    textBlock.FontWeight,
                    textBlock.FontStretch
                    ),
                textBlock.FontSize,
                Brushes.Transparent,
                dpi.PixelsPerDip);

            return Math.Ceiling(formattedText.WidthIncludingTrailingWhitespace);
        }

        private static FrameworkElement FindTimelineHost(DependencyObject start)
        {
            DependencyObject current = start;

            while (current != null)
            {
                if (current is FrameworkElement element && GetIsTimelineHost(element))
                    return element;

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent)
            where T : DependencyObject
        {
            if (parent == null)
                yield break;

            int count = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is T typedChild)
                    yield return typedChild;

                foreach (T descendant in FindVisualChildren<T>(child))
                    yield return descendant;
            }
        }

        private static Rect GetBoundsRelativeTo(FrameworkElement element, FrameworkElement ancestor)
        {
            if (element.ActualWidth <= 0 || element.ActualHeight <= 0)
                return Rect.Empty;

            try
            {
                return element
                    .TransformToAncestor(ancestor)
                    .TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));
            }
            catch (InvalidOperationException)
            {
                return Rect.Empty;
            }
        }

        private static double? GetLeftRelativeTo(
            FrameworkElement element,
            FrameworkElement ancestor)
        {
            try
            {
                return element.TransformToAncestor(ancestor).Transform(new Point()).X;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }



    }
}
