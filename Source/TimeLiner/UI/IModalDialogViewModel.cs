// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Christian Pistor

namespace TimeLiner.UI
{
    /// <summary>
    /// Provides the result returned by a modal dialog.
    /// </summary>
    public interface IModalDialogViewModel
    {
        bool? DialogResult { get; set; }
    }
}
