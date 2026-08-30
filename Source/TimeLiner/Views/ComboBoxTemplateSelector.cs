// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2026 Christian Pistor

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TimeLiner.Views
{
    /// <summary>
    /// Selects the data templates for a combo box control.
    /// </summary>
    /// <remarks>
    /// https://stackoverflow.com/questions/4672867/can-i-use-a-different-template-for-the-selected-item-in-a-wpf-combobox-than-for/4672995
    /// </remarks>
    internal class ComboBoxTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SelectedItemTemplate { get; set; }
        public DataTemplateSelector SelectedItemTemplateSelector { get; set; }
        public DataTemplate DropdownItemsTemplate { get; set; }
        public DataTemplateSelector DropdownItemsTemplateSelector { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            DependencyObject itemToCheck = container;

            // Search up the visual tree, stopping at either a combo box or
            // a combo box item (or null). This will determine which template to use.
            while (itemToCheck != null && !(itemToCheck is ComboBoxItem) && !(itemToCheck is ComboBox))
            {
                itemToCheck = VisualTreeHelper.GetParent(itemToCheck);
            }

            // If you stopped at a combo box item, you're inside the drop-down.
            bool inDropDown = (itemToCheck is ComboBoxItem);

            return inDropDown
                ? DropdownItemsTemplate ?? DropdownItemsTemplateSelector?.SelectTemplate(item, container)
                : SelectedItemTemplate ?? SelectedItemTemplateSelector?.SelectTemplate(item, container);
        }
    }
}
