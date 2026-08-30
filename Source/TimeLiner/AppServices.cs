// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Christian Pistor

using System;
using TimeLiner.Models;
using TimeLiner.Themes;
using TimeLiner.UI;
using TimeLiner.ViewModels;

namespace TimeLiner
{
    /// <summary>
    /// Creates and owns the application-wide services and view models.
    /// </summary>
    internal static class AppServices
    {
        private static IThemeService _themeService;
        private static IDialogService _dialogService;

        private static readonly Lazy<TimeLinesViewModel> _timeLines = new(() =>
            new TimeLinesViewModel(DialogService, Settings, TimeLineScaling));

        private static readonly Lazy<TimeLineItemDialogViewModel> _timeLineItemDialog = new(() =>
            new TimeLineItemDialogViewModel(DialogService, Settings, TimeLines));

        private static readonly Lazy<InfoDialogViewModel> _infoDialog = new(() =>
            new InfoDialogViewModel(DialogService, TimeLines));

        private static readonly Lazy<FindDialogViewModel> _findDialog = new(() =>
            new FindDialogViewModel(DialogService, TimeLines));

        private static readonly Lazy<TimeLineDialogViewModel> _timeLineDialog = new(() =>
            new TimeLineDialogViewModel(DialogService, TimeLines));

        public static SettingsViewModel Settings { get; } = new(new JsonSettingsRepository());

        public static TimeLineScalingViewModel TimeLineScaling { get; } = new(Settings);

        public static TimeLinesViewModel TimeLines => _timeLines.Value;

        public static TimeLineItemDialogViewModel TimeLineItemDialog => _timeLineItemDialog.Value;

        public static InfoDialogViewModel InfoDialog => _infoDialog.Value;

        public static FindDialogViewModel FindDialog => _findDialog.Value;

        public static TimeLineDialogViewModel TimeLineDialog => _timeLineDialog.Value;

        public static IThemeService ThemeService => _themeService
            ?? throw new InvalidOperationException("Application services have not been initialized.");

        private static IDialogService DialogService => _dialogService
            ?? throw new InvalidOperationException("Application services have not been initialized.");

        public static void Initialize(IThemeService themeService, IDialogService dialogService)
        {
            _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        }
    }
}
