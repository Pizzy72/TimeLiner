// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2026 Christian Pistor

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using TimeLiner.UI;

namespace TimeLiner.Views
{
    /// <summary>
    /// The color dialog.
    /// </summary>
    public partial class ColorDialog : BaseRibbonWindow
    {
        private readonly List<ListBox> _colorListBoxes;

        /// <seealso cref="SelectedColor"/>
        private static readonly DependencyProperty _selectedColorProperty = DependencyProperty.RegisterAttached(
            nameof(SelectedColor),
            typeof(Color),
            typeof(ColorDialog)
            );

        /// <summary>
        /// All available colors.
        /// </summary>
        private static readonly List<ColorInfo> _colors = CreatePalette();

        private static List<ColorInfo> CreatePalette()
        {
            List<ColorInfo> colors = [];

            double[] grayscaleLightness =
            {
                0.18, 0.23, 0.28, 0.33,
                0.38, 0.43, 0.48, 0.53,
                0.58, 0.63, 0.68, 0.73,
                0.78, 0.83, 0.88, 0.93
            };
            colors.AddRange(grayscaleLightness.Select(lightness =>
                new ColorInfo("Grayscale", ColorFromOklch(lightness, 0, 0))));

            AddColorFamily(colors, "Reds", 25,
                [0.34, 0.50, 0.66, 0.82], [0.13, 0.15, 0.145, 0.095]);
            AddColorFamily(colors, "Oranges", 55,
                [0.40, 0.54, 0.68, 0.82], [0.095, 0.125, 0.145, 0.105]);
            AddColorFamily(colors, "Yellows", 95,
                [0.48, 0.60, 0.72, 0.84], [0.095, 0.12, 0.14, 0.14]);
            AddColorFamily(colors, "Greens", 145,
                [0.34, 0.50, 0.66, 0.82], [0.10, 0.14, 0.16, 0.15]);
            AddColorFamily(colors, "Cyans", 195,
                [0.40, 0.54, 0.68, 0.82], [0.067, 0.092, 0.115, 0.115]);
            AddColorFamily(colors, "Blues", 255,
                [0.30, 0.47, 0.64, 0.81], [0.09, 0.135, 0.15, 0.09]);
            AddColorFamily(colors, "Violets", 305,
                [0.31, 0.48, 0.65, 0.82], [0.15, 0.195, 0.185, 0.11]);

            return colors;
        }

        private static void AddColorFamily(
            List<ColorInfo> colors,
            string category,
            double hue,
            double[] lightnessValues,
            double[] chromaValues)
        {
            double[] chromaFactors = [1.00, 0.72, 0.44, 0.16];

            foreach (double chromaFactor in chromaFactors)
            {
                for (int column = 0; column < lightnessValues.Length; column++)
                {
                    colors.Add(new ColorInfo(category, ColorFromOklch(
                        lightnessValues[column],
                        chromaValues[column] * chromaFactor,
                        hue)));
                }
            }
        }

        private static Color ColorFromOklch(double lightness, double chroma, double hue)
        {
            double hueRadians = hue * Math.PI / 180;
            double low = 0;
            double high = chroma;
            (double R, double G, double B) linearRgb = OklabToLinearSrgb(
                lightness,
                chroma * Math.Cos(hueRadians),
                chroma * Math.Sin(hueRadians));

            // Preserve lightness and hue, reducing only chroma until the color fits sRGB.
            if (!IsInSrgbGamut(linearRgb))
            {
                for (int iteration = 0; iteration < 16; iteration++)
                {
                    double candidate = (low + high) / 2;
                    (double R, double G, double B) candidateRgb = OklabToLinearSrgb(
                        lightness,
                        candidate * Math.Cos(hueRadians),
                        candidate * Math.Sin(hueRadians));

                    if (IsInSrgbGamut(candidateRgb))
                    {
                        low = candidate;
                        linearRgb = candidateRgb;
                    }
                    else
                    {
                        high = candidate;
                    }
                }
            }

            return Color.FromRgb(
                ToSrgbByte(linearRgb.R),
                ToSrgbByte(linearRgb.G),
                ToSrgbByte(linearRgb.B));
        }

        private static (double R, double G, double B) OklabToLinearSrgb(double lightness, double a, double b)
        {
            double l = Math.Pow(lightness + 0.3963377774 * a + 0.2158037573 * b, 3);
            double m = Math.Pow(lightness - 0.1055613458 * a - 0.0638541728 * b, 3);
            double s = Math.Pow(lightness - 0.0894841775 * a - 1.2914855480 * b, 3);

            return (
                4.0767416621 * l - 3.3077115913 * m + 0.2309699292 * s,
                -1.2684380046 * l + 2.6097574011 * m - 0.3413193965 * s,
                -0.0041960863 * l - 0.7034186147 * m + 1.7076147010 * s);
        }

        private static bool IsInSrgbGamut((double R, double G, double B) color)
        {
            return color.R >= 0 && color.R <= 1
                && color.G >= 0 && color.G <= 1
                && color.B >= 0 && color.B <= 1;
        }

        private static byte ToSrgbByte(double linearComponent)
        {
            double component = Math.Clamp(linearComponent, 0, 1);
            double srgb = component <= 0.0031308
                ? 12.92 * component
                : 1.055 * Math.Pow(component, 1 / 2.4) - 0.055;

            return (byte)Math.Round(srgb * 255);
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ColorDialog()
        {
            InitializeComponent();

            _colorListBoxes =
            [
                ListBoxGrayscale,
                ListBoxReds,
                ListBoxOranges,
                ListBoxYellows,
                ListBoxGreens,
                ListBoxCyans,
                ListBoxBlues,
                ListBoxViolets
            ];

            foreach (ListBox listBox in _colorListBoxes)
            {
                listBox.ItemsSource = _colors.Where(c => c.Category == (string)listBox.Tag);
            }
        }

        /// <summary>
        /// The selected color.
        /// </summary>
        public Color SelectedColor
        {
            get => (Color)GetValue(_selectedColorProperty);
            set => SetValue(_selectedColorProperty, value);
        }

        /// <summary>
        /// Is called when the Cancel button is clicked.
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Is called when the color list box is double clicked.
        /// </summary>
        private void ListBoxColors_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource.GetType() == typeof(Rectangle))
            {
                DialogResult = true;
                Close();
            }
        }

        /// <summary>
        /// Is called when a color is selected in the list box.
        /// </summary>
        private void ListBoxColors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
            {
                return;
            }

            ListBox selectedListBox = (ListBox)sender;
            foreach (ListBox listBox in _colorListBoxes.Where(l => l != selectedListBox))
            {
                listBox.SelectedItem = null;
            }

            ColorInfo colorInfo = (ColorInfo)e.AddedItems[0];
            SelectedColor = colorInfo.Color;
        }

        /// <summary>
        /// Is called when the OK button is clicked.
        /// </summary>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Tries to select the given color in the color list box.
        /// </summary>
        private void SelectColor(Color color)
        {
            ColorInfo colorInfo = _colors.FirstOrDefault(c => c.Color == color);

            if (colorInfo != null)
            {
                ListBox listBox = _colorListBoxes.First(l => (string)l.Tag == colorInfo.Category);
                listBox.SelectedItem = colorInfo;
            }
        }

        /// <summary>
        /// Is called when the text box with the color name is double clicked.
        /// </summary>
        private void TextColor_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ((TextBox)sender)?.SelectAll();
        }

        /// <summary>
        /// Is called when a key is pressed in the color picker window.
        /// </summary>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        /// <summary>
        /// Is called when the color picker window is loaded.
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SelectColor(SelectedColor);

            ListBox selectedListBox = _colorListBoxes.FirstOrDefault(l => l.SelectedItem != null);
            ListBoxItem listBoxItem = selectedListBox == null
                ? null
                : (ListBoxItem)selectedListBox.ItemContainerGenerator.ContainerFromItem(selectedListBox.SelectedItem);

            listBoxItem?.Focus();
        }
    }
}
