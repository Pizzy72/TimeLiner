// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Christian Pistor

using Microsoft.VisualStudio.TestTools.UnitTesting;
using TimeLiner.UI;
using TimeLiner.ViewModels;

namespace TimeLinerTest
{
    [TestClass]
    public class TestChangelogDialogViewModel
    {
        [TestMethod]
        public void ActivatesExistingDialogAndCanReopenAfterClosing()
        {
            RecordingDialogService service = new();
            ChangelogDialogViewModel viewModel = new(service);
            object owner = new();

            viewModel.ShowDialog(owner);
            Assert.AreEqual(1, service.ShowCount);
            Assert.AreSame(owner, service.Owner);
            Assert.AreSame(viewModel, service.ViewModel);
            StringAssert.Contains(viewModel.Text, "2.15.6 - 2026-10-14");

            viewModel.ShowDialog(owner);
            Assert.AreEqual(1, service.ShowCount);
            Assert.AreEqual(1, service.ActivateCount);

            viewModel.CloseCommand.Execute(null);
            Assert.AreEqual(1, service.CloseCount);
            viewModel.DialogClosed();

            viewModel.ShowDialog(owner);
            Assert.AreEqual(2, service.ShowCount);
        }

        private sealed class RecordingDialogService : IDialogService
        {
            public int ShowCount { get; private set; }
            public int ActivateCount { get; private set; }
            public int CloseCount { get; private set; }
            public object Owner { get; private set; }
            public object ViewModel { get; private set; }

            public void Show(object ownerViewModel, object viewModel)
            {
                ShowCount++;
                Owner = ownerViewModel;
                ViewModel = viewModel;
            }

            public bool? ShowDialog(object ownerViewModel, object viewModel) => null;
            public void Activate(object viewModel)
            {
                Assert.AreSame(ViewModel, viewModel);
                ActivateCount++;
            }
            public void Close(object viewModel)
            {
                Assert.AreSame(ViewModel, viewModel);
                CloseCount++;
            }
        }
    }
}