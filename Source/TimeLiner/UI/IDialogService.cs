// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Christian Pistor

namespace TimeLiner.UI
{
    /// <summary>
    /// Opens and controls application dialogs for view models.
    /// </summary>
    public interface IDialogService
    {
        void Show(object ownerViewModel, object viewModel);

        bool? ShowDialog(object ownerViewModel, object viewModel);

        void Activate(object viewModel);

        void Close(object viewModel);
    }
}
