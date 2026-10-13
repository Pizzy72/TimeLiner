// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2026 Christian Pistor

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

            foreach (byte value in new byte[] { 0, 32, 80, 128, 176, 208, 240, 255 })
                colors.Add(new ColorInfo("Grayscale", Color.FromRgb(value, value, value)));

            AddColorFamily(colors, "Reds", Color.FromRgb(0xFF, 0x00, 0x00));
            AddColorFamily(colors, "Oranges", Color.FromRgb(0xFF, 0x80, 0x00));
            AddColorFamily(colors, "Yellows", Color.FromRgb(0xFF, 0xFF, 0x00));
            AddColorFamily(colors, "Greens", Color.FromRgb(0x00, 0xFF, 0x00));
            AddColorFamily(colors, "Cyans", Color.FromRgb(0x00, 0xFF, 0xFF));
            AddColorFamily(colors, "Blues", Color.FromRgb(0x00, 0x00, 0xFF));
            AddColorFamily(colors, "Violets", Color.FromRgb(0x80, 0x00, 0xFF));
            AddColorFamily(colors, "Magentas", Color.FromRgb(0xFF, 0x00, 0xFF));

            return colors;
        }

        private static void AddColorFamily(List<ColorInfo> colors, string category, Color anchor)
        {
            colors.Add(new ColorInfo(category, anchor));
            colors.Add(new ColorInfo(category, Blend(anchor, Colors.Black, 0.45)));
            colors.Add(new ColorInfo(category, Blend(anchor, Colors.White, 0.25)));
            colors.Add(new ColorInfo(category, Blend(anchor, Colors.White, 0.55)));
            colors.Add(new ColorInfo(category, Blend(anchor, Colors.White, 0.78)));
        }

        private static Color Blend(Color color, Color target, double amount)
        {
            return Color.FromRgb(
                (byte)(color.R + (target.R - color.R) * amount),
                (byte)(color.G + (target.G - color.G) * amount),
                (byte)(color.B + (target.B - color.B) * amount));
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
                ListBoxViolets,
                ListBoxMagentas
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
