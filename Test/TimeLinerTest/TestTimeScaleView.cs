// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Christian Pistor

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TimeLiner.Views;

namespace TimeLinerTest
{
    [TestClass]
    public class TestTimeScaleView
    {
        [STATestMethod]
        public void Scroll_ReusesLabels_AndKeepsTicksAligned()
        {
            TimeScaleView scale = CreateScale();
            StackPanel panel = GetPanel(scale);
            TextBlock[] labels = panel.Children.Cast<TextBlock>().ToArray();
            Assert.AreEqual(9, labels.Length);

            // Includes fractional motion, boundaries, large jumps and reverse scrolling.
            foreach (double offset in new[] { 0.25, 99.75, 100, 101, 1250, 999, 0 })
            {
                scale.ScrollOffset = offset;
                Assert.AreEqual(labels.Length, panel.Children.Count);
                for (int i = 0; i < labels.Length; i++)
                {
                    Assert.AreSame(labels[i], panel.Children[i]);
                    Assert.AreEqual($"{((long)(offset / 100) + i) * 5} s", labels[i].Text);
                }
                Assert.AreEqual(20 - 10 - offset % 100,
                    panel.RenderTransform.Transform(new Point()).X, 0.00001);
            }
        }

        [STATestMethod]
        public void FractionalScroll_DoesNotInvalidateScaleLayout()
        {
            TimeScaleView scale = CreateScale();
            scale.Measure(new Size(800, 18));
            scale.Arrange(new Rect(0, 0, 800, 18));
            StackPanel panel = GetPanel(scale);
            TextBlock first = (TextBlock)panel.Children[0];
            string originalText = first.Text;

            scale.ScrollOffset = 25.5;

            Assert.IsTrue(scale.IsMeasureValid);
            Assert.IsTrue(scale.IsArrangeValid);
            Assert.IsTrue(panel.IsMeasureValid);
            Assert.AreSame(originalText, first.Text);
            Assert.AreEqual(-15.5, first.TranslatePoint(new Point(), scale).X, 0.00001);
        }

        [STATestMethod]
        public void ZoomAndResize_UpdateLabels_AndReuseRemainingControls()
        {
            TimeScaleView scale = CreateScale();
            StackPanel panel = GetPanel(scale);
            TextBlock first = (TextBlock)panel.Children[0];
            scale.ScrollOffset = 250;
            scale.GridWidth = 50;
            scale.Interval = 2;
            scale.Unit = "min";

            Assert.AreEqual(17, panel.Children.Count);
            Assert.AreSame(first, panel.Children[0]);
            Assert.AreEqual(50d, first.Width);
            Assert.AreEqual("10 min", first.Text);
            Assert.AreEqual("42 min", ((TextBlock)panel.Children[16]).Text);

            scale.VisibleWidth = 125;
            Assert.AreEqual(3, panel.Children.Count);
            Assert.AreSame(first, panel.Children[0]);
            scale.VisibleWidth = 400;
            Assert.AreEqual(9, panel.Children.Count);
            Assert.AreSame(first, panel.Children[0]);
            Assert.AreEqual("26 min", ((TextBlock)panel.Children[8]).Text);

            scale.LeftMargin = 40;
            Assert.AreEqual(30d, panel.RenderTransform.Transform(new Point()).X);
            Assert.AreEqual("10 min", first.Text);
        }

        private static TimeScaleView CreateScale() => new()
        {
            Height = 18, GridWidth = 100, Interval = 5, Unit = "s",
            LeftMargin = 20, VisibleWidth = 800
        };

        private static StackPanel GetPanel(TimeScaleView scale) =>
            (StackPanel)((Canvas)scale.Content).Children[0];
    }
}
