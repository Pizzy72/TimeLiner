// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Christian Pistor

using TimeLiner.UI;

namespace TimeLinerTest.TestDoubles
{
    internal sealed class DialogServiceStub : IDialogService
    {
        public void Show(object ownerViewModel, object viewModel)
        {
        }

        public bool? ShowDialog(object ownerViewModel, object viewModel)
        {
            return null;
        }

        public void Activate(object viewModel)
        {
        }

        public void Close(object viewModel)
        {
        }
    }
}
