// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2026 Christian Pistor

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using TimeLiner.Views;

namespace TimeLinerTest
{
    [TestClass]
    public class TestTimelineItemTextBehavior
    {
        [STATestMethod]
        public void ScrollBatch_CoalescesLabelUpdates_AndPreservesSpacing()
        {
            Canvas host = new() { Width = 800, Height = 40 };
            TimelineItemTextBehavior.SetIsTimelineHost(host, true);
            (Canvas Anchor, TextBlock Text)[] items = new (Canvas Anchor, TextBlock Text)[40];
            for (int i = 0; i < items.Length; i++)
                items[i] = AddItem(host, 60 + i * 18, 4);

            Window window = new()
            {
                Content = host, Width = 820, Height = 80,
                ShowActivated = false, ShowInTaskbar = false,
                WindowStyle = WindowStyle.None, Left = -10000, Top = -10000
            };
            int scheduledUpdates = 0;
            void OnOperationPosted(object sender, DispatcherHookEventArgs e)
            {
                if (e.Operation.Priority == DispatcherPriority.Loaded)
                    scheduledUpdates++;
            }

            try
            {
                window.Show();
                FlushLayout(window);
                window.Dispatcher.Hooks.OperationPosted += OnOperationPosted;
                for (int step = 1; step <= 3; step++)
                    for (int i = 0; i < items.Length; i++)
                        Canvas.SetLeft(items[i].Anchor, 60 + i * 18 - step * 5);
                FlushLayout(window);

                Console.WriteLine($"Loaded-priority operations for 120 anchor moves: {scheduledUpdates}");
                Assert.IsLessThanOrEqualTo(8, scheduledUpdates,
                    "A scroll batch should schedule row updates, not one update per anchor and label.");
                for (int i = 0; i < items.Length - 1; i++)
                    Assert.AreEqual(12d, items[i].Text.Width);
                Assert.IsTrue(double.IsNaN(items[^1].Text.Width));
            }
            finally
            {
                window.Dispatcher.Hooks.OperationPosted -= OnOperationPosted;
                window.Close();
            }
        }

        [STATestMethod]
        [DataRow(0.25)]
        [DataRow(0.0)]
        public void ZoomOut_HidesCollidingLabels_AndZoomInRestoresThem(double spacing)
        {
            Canvas host = new() { Width = 800, Height = 40 };
            TimelineItemTextBehavior.SetIsTimelineHost(host, true);
            (Canvas Anchor, TextBlock Text) first = AddItem(host, 60, 4);
            (Canvas Anchor, TextBlock Text) second = AddItem(host, 160, 18);
            (Canvas Anchor, TextBlock Text) last = AddItem(host, 260, 18);
            Window window = new()
            {
                Content = host,
                Width = 820,
                Height = 80,
                ShowActivated = false,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None,
                Left = -10000,
                Top = -10000
            };

            try
            {
                window.Show();
                FlushLayout(window);
                Assert.IsGreaterThan(0, first.Text.ActualWidth);
                Assert.IsGreaterThan(0, second.Text.ActualWidth);

                Canvas.SetLeft(second.Anchor, 60 + spacing);
                Canvas.SetLeft(last.Anchor, 60 + 2 * spacing);
                FlushLayout(window);

                Assert.AreEqual(0d, first.Text.Width);
                Assert.AreEqual(0d, second.Text.Width);
                // WPF retains the ellipsis width in RenderSize, but clips it
                // to the explicitly assigned zero-width layout slot.
                Assert.AreEqual(0d, LayoutInformation.GetLayoutClip(first.Text).Bounds.Width);
                Assert.AreEqual(0d, LayoutInformation.GetLayoutClip(second.Text).Bounds.Width);
                Assert.IsTrue(double.IsNaN(last.Text.Width));
                Assert.IsGreaterThan(0, last.Text.ActualWidth);

                Canvas.SetLeft(second.Anchor, 160);
                Canvas.SetLeft(last.Anchor, 260);
                FlushLayout(window);
                Assert.IsGreaterThan(0, first.Text.ActualWidth);
                Assert.IsGreaterThan(0, second.Text.ActualWidth);
                Assert.IsLessThanOrEqualTo(158d, 64 + first.Text.ActualWidth);
                Assert.IsLessThanOrEqualTo(258d, 178 + second.Text.ActualWidth);
            }
            finally
            {
                window.Close();
            }
        }

        private static (Canvas Anchor, TextBlock Text) AddItem(Canvas host, double left, double textOffset)
        {
            Canvas anchor = new() { DataContext = new object() };
            Canvas.SetLeft(anchor, left);
            TimelineItemTextBehavior.SetIsTextAnchor(anchor, true);
            TextBlock text = new()
            {
                Text = "A long timeline item label that extends beyond its item",
                TextWrapping = TextWrapping.NoWrap,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            Canvas.SetLeft(text, textOffset);
            TimelineItemTextBehavior.SetEnableAutoWidth(text, true);
            anchor.Children.Add(text);
            host.Children.Add(anchor);
            return (anchor, text);
        }

        private static void FlushLayout(Window window)
        {
            window.UpdateLayout();
            window.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
            window.UpdateLayout();
        }
    }
}
