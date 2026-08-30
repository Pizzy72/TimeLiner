// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2025 Christian Pistor

namespace TimeLiner.ViewModels
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    internal class ViewModelLocator
    {
        public TimeLinesViewModel MainWindow => AppServices.TimeLines;
        public TimeLineItemDialogViewModel EditTimeLineItemDialog => AppServices.TimeLineItemDialog;
        public InfoDialogViewModel InfoDialog => AppServices.InfoDialog;
        public FindDialogViewModel FindDialog => AppServices.FindDialog;
        public TimeLineDialogViewModel TimeLineDialog => AppServices.TimeLineDialog;
        public SettingsViewModel Settings => AppServices.Settings;
        public TimeLineScalingViewModel TimeLineScaling => AppServices.TimeLineScaling;

    }
}
