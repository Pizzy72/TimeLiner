// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Christian Pistor
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TimeLiner.Views;

namespace TimeLinerTest
{
    [TestClass]
    public class TestNaturalTextWidth
    {
        [STATestMethod]
        [DataRow("Text")]
        [DataRow("Empty")]
        [DataRow("Family")]
        [DataRow("Size")]
        [DataRow("Style")]
        [DataRow("Weight")]
        [DataRow("Stretch")]
        [DataRow("Direction")]
        [DataRow("Dpi")]
        [DataRow("Culture")]
        [DataRow("MutableCulture")]
        public void ChangedInput_MatchesMeasurementOfFreshTextBlock(string changedInput)
        {
            CultureInfo previousCulture = CultureInfo.CurrentUICulture;
            try
            {
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
                TextBlock reused = new()
                {
                    Text = "Timeline label 123 العربية", FontFamily = new FontFamily("Arial"),
                    FontSize = 14, Width = 1
                };
                VisualTreeHelper.SetRootDpi(reused, new DpiScale(1.25, 1.25));
                Assert.IsGreaterThan(1, TimelineItemTextBehavior.GetDesiredTextWidth(reused));
                switch (changedInput)
                {
                    case "Text": reused.Text = "WWWW a substantially longer replacement label"; break;
                    case "Empty": reused.Text = ""; break;
                    case "Family": reused.FontFamily = new FontFamily("Courier New"); break;
                    case "Size": reused.FontSize = 28; break;
                    case "Style": reused.FontStyle = FontStyles.Italic; break;
                    case "Weight": reused.FontWeight = FontWeights.Bold; break;
                    case "Stretch": reused.FontStretch = FontStretches.Condensed; break;
                    case "Direction": reused.FlowDirection = FlowDirection.RightToLeft; break;
                    case "Dpi": VisualTreeHelper.SetRootDpi(reused, new DpiScale(2, 2)); break;
                    case "Culture": CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("ar-SA"); break;
                    case "MutableCulture": CultureInfo.CurrentUICulture = new CultureInfo("ar-SA"); break;
                }

                // Differential oracle: a new visual has no previously cached measurement.
                TextBlock fresh = new()
                {
                    Text = reused.Text, FontFamily = reused.FontFamily, FontSize = reused.FontSize,
                    FontStyle = reused.FontStyle, FontWeight = reused.FontWeight,
                    FontStretch = reused.FontStretch, FlowDirection = reused.FlowDirection
                };
                VisualTreeHelper.SetRootDpi(fresh, VisualTreeHelper.GetDpi(reused));
                double expected = TimelineItemTextBehavior.GetDesiredTextWidth(fresh);
                Assert.AreEqual(expected, TimelineItemTextBehavior.GetDesiredTextWidth(reused), changedInput);
                Assert.AreEqual(expected, TimelineItemTextBehavior.GetDesiredTextWidth(reused), "Repeated lookup");
            }
            finally { CultureInfo.CurrentUICulture = previousCulture; }
        }

        [STATestMethod]
        public void CachedTextBlock_CanBeCollected()
        {
            WeakReference reference = CreateCachedTextBlock();
            for (int i = 0; i < 3; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            Assert.IsFalse(reference.IsAlive, "The cache must not retain its TextBlock globally.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference CreateCachedTextBlock()
        {
            TextBlock block = new() { Text = "A cached label" };
            TimelineItemTextBehavior.GetDesiredTextWidth(block);
            return new WeakReference(block);
        }
    }
}
