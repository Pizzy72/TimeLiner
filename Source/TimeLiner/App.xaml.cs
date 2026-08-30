// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2025 Christian Pistor

using System.Windows;
using System.Windows.Controls;
using TimeLiner.Themes;
using TimeLiner.UI;
using TimeLiner.ViewModels;

namespace TimeLiner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Application startup logic.
        /// </summary>
        /// <param name="e">Startup event arguments.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Apply application theme
            ViewModelLocator locator = (ViewModelLocator)Current.Resources["Locator"];
            IThemeService themeService = new ThemeService();
            themeService.ApplyTheme(locator.Settings.SelectedTheme);

            // Register services
            AppServices.Initialize(themeService, new DialogService());

            // Select all text in text box with double click
            EventManager.RegisterClassHandler(typeof(TextBox),
               Control.MouseDoubleClickEvent,
               new RoutedEventHandler(TextBox_GotFocus));
        }

        /// <summary>
        /// Selects all text in a TextBox when it receives a double-click event.
        /// </summary>
        /// <param name="sender">The TextBox control.</param>
        /// <param name="e">Routed event arguments.</param>
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            (sender as TextBox)?.SelectAll();
        }
    }
}
