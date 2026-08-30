// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2025 Christian Pistor

using System;
using System.Windows.Input;
using TimeLiner.Common;
using TimeLiner.UI;

namespace TimeLiner.ViewModels
{
    /// <summary>
    /// The view model of the find dialog.
    /// </summary>
    internal class FindDialogViewModel : ViewModelBase
    {
        /// <summary>
        /// Reference to the dialog service.
        /// </summary>
        private readonly IDialogService _dialogService;

        /// <summary>
        /// Reference to the timelines view model.
        /// </summary>
        private readonly TimeLinesViewModel _timeLinesViewModel;

        /// <summary>
        /// The index of the selected timeline item.
        /// </summary>
        private int _index = -1;

        /// <summary>
        /// The number of matching timeline items.
        /// </summary>
        private int _hits = 0;

        /// <summary>
        /// Is true if the dialog is visible.
        /// </summary>
        private bool _isVisible = false;

        /// <summary>
        /// Constructor.
        /// </summary>
        public FindDialogViewModel(IDialogService dialogService, TimeLinesViewModel timeLinesViewModel)
        {
            _dialogService = dialogService;
            _timeLinesViewModel = timeLinesViewModel;
        }

        /// <summary>
        /// Show the find dialog.
        /// </summary>
        public void ShowDialog()
        {
            if (_isVisible)
            {
                _dialogService.Activate(this);
            }
            else
            {
                _dialogService.Show(_timeLinesViewModel, this);
                _isVisible = true;
            }
        }

        /// <summary>
        /// Close the find dialog.
        /// </summary>
        public void CloseDialog()
        {
            _dialogService.Close(this);
        }

        public void DialogClosed()
        {
            Reset();
            _isVisible = false;
        }

        /// <summary>
        /// The string to use to find a timeline element by name.
        /// </summary>
        public string FindString
        {
            get => _findString;
            set
            {
                if (_findString != value)
                {
                    _findString = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string _findString;

        /// <summary>
        /// Reset find dialog.
        /// </summary>
        public void Reset()
        {
            FindString = "";
            _index = -1;
            _hits = 0;
        }

        /// <summary>
        /// Command to find the next timeline item.
        /// </summary>
        public ICommand FindNextCommand
        {
            get
            {
                return _findNextCommand ?? (_findNextCommand = new MyActionCommand(_ =>
                    {
                        FindNextTimeLineItem();
                    }));
            }
        }

        private ICommand _findNextCommand;

        /// <summary>
        /// Command to close the find dialog.
        /// </summary>
        public ICommand CloseCommand
        {
            get
            {
                return _closeCommand ?? (_closeCommand = new MyActionCommand(
                    _ => { CloseDialog(); }
                    ));
            }
        }

        private ICommand _closeCommand;

        /// <summary>
        /// Find and select next timeline item.
        /// </summary>
        private void FindNextTimeLineItem()
        {
            if (string.IsNullOrEmpty(FindString))
            {
                return;
            }

            int index = FindTimeLineItem(_index + 1, FindString);

            if (index > -1)
            {
                SelectTimeLineItem(index);
                _index = index;
                _hits++;
            }
            else
            {
                if (_hits > 0)
                {
                    _index = FindTimeLineItem(0, FindString);
                    SelectTimeLineItem(_index);
                    _hits = 1;
                }
            }
        }

        /// <summary>
        /// Select timeline item by its index.
        /// </summary>
        private void SelectTimeLineItem(int index)
        {
            if (index > -1)
            {
                TimeLineItemViewModel timeLineItem = GetTimeLineItem(index);
                _timeLinesViewModel.SelectTimeLineItem(timeLineItem);
            }
        }

        /// <summary>
        /// Find timeline item starting at the given index.
        /// </summary>
        private int FindTimeLineItem(int start, string findString)
        {
            string findStringTrimmed = findString.Trim();

            for (int index = start; index < _timeLinesViewModel.TimeLineItems.Count; index++)
            {
                TimeLineItemViewModel timeLineItem = GetTimeLineItem(index);

                if (timeLineItem.Name.IndexOf(findStringTrimmed, StringComparison.OrdinalIgnoreCase) > -1)
                {
                    return index;
                }
            }

            return -1;
        }

        /// <summary>
        /// Get the timeline item by its index.
        /// </summary>
        private TimeLineItemViewModel GetTimeLineItem(int index)
        {
            return _timeLinesViewModel.TimeLineItems[index];
        }
    }
}
