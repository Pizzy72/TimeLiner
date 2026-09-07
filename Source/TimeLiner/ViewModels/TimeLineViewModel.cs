// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2026 Christian Pistor

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using TimeLiner.Common;
using TimeLiner.Models;

namespace TimeLiner.ViewModels
{
    /// <summary>
    /// The view model of a timeline.
    /// </summary>
    internal class TimeLineViewModel : ViewModelBase, IDisposable
    {
        /// <summary>
        /// Reference to the global settings view model.
        /// </summary>
        private readonly SettingsViewModel _settingsViewModel;

        /// <see cref="TimeLineItems"/>
        /// <see cref="TimeLineItemCollectionView"/>
        private List<TimeLineItemViewModel> _timeLineItems = [];

        private ICollectionView _timeLineItemCollectionView;
        private readonly ObservableCollection<TimeLineItemViewModel> _visibleItems = [];

        private HashSet<TimeLineItemViewModel> _visibleTimeLineItems;

        /// <see cref="IsUniversalTime" />
        private bool _isUniversalTime;

        /// <see cref="DeleteTimeLineCommand"/>
        private ICommand _deleteTimeLineCommand;

        /// <see cref="RenameTimeLineCommand"/>
        private ICommand _renameTimeLineCommand;

        /// <see cref="NewTimeLineCommand"/>
        private ICommand _newTimeLineCommand;

        /// <see cref="NewTimeLineItemCommand"/>
        private ICommand _newTimeLineItemCommand;

        /// <see cref="IsModified"/>
        private bool _isModified;

        /// <summary>
        /// Is true if instance is disposed.
        /// </summary>
        private bool _isDisposed;

        /// <see cref="IsLoaded"/>
        private bool _isLoaded;

        private bool _hasDeferredScrollUpdate;

        // Maintained by the owning list on structural changes, never searched while scrolling.
        internal int RowIndex { get; set; } = -1;

        internal bool IsInVerticalViewport
        {
            get
            {
                double top = RowIndex * Height - TimeLinesViewModel.VerticalScrollOffset;
                return RowIndex >= 0 && top >= 0 && top < TimeLinesViewModel.TimeLinesVisibleHeight;
            }
        }

        /// <summary>
        /// The model of the associated timeline.
        /// </summary>
        public TimeLineModel TimeLineModel
        {
            get;
        }

        /// <summary>
        /// The main view model.
        /// </summary>
        public TimeLinesViewModel TimeLinesViewModel
        {
            get;
        }

        /// <summary>
        /// The view models of the timeline items of this timeline.
        /// </summary>
        public IReadOnlyList<TimeLineItemViewModel> TimeLineItems => _timeLineItems.AsReadOnly();

        /// <summary>
        /// Is raised when the timeline has been loaded.
        /// </summary>
        public event EventHandler Loaded;

        /// <summary>
        /// Constructor.
        /// </summary>
        public TimeLineViewModel(
            TimeLinesViewModel timeLinesViewModel,
            TimeLineModel timeLineModel,
            SettingsViewModel settingsViewModel,
            TimeLineScalingViewModel timeLineScaling
            )
        {
            TimeLinesViewModel = timeLinesViewModel;
            TimeLineModel = timeLineModel;
            _settingsViewModel = settingsViewModel;

            PropertyChangedEventManager.AddHandler(timeLinesViewModel, TimeLinesViewModel_PropertyChanged, "");
            PropertyChangedEventManager.AddHandler(settingsViewModel, Settings_PropertyChanged, "");
            PropertyChangedEventManager.AddHandler(this, TimeLinesViewModel.TimeLineViewModel_Propertychanged, "");

            foreach (TimeLineItemModel timeLineItemModel in TimeLineModel.TimeLineItems)
            {
                TimeLineItemViewModel item = new(timeLineItemModel, this, _settingsViewModel, timeLineScaling);
                PropertyChangedEventManager.AddHandler(item, TimeLineItem_PropertyChanged, "");
                _timeLineItems.Add(item);
            }

        }

        /// <summary>
        /// Command to delete this timeline.
        /// </summary>
        public ICommand DeleteTimeLineCommand
        {
            get
            {
                return _deleteTimeLineCommand ??= new MyActionCommand(
                    async _ => { await TimeLinesViewModel.DeleteTimeLine(this); },
                    _ => true
                );
            }
        }

        /// <summary>
        /// Command to rename this timeline.
        /// </summary>
        public ICommand RenameTimeLineCommand
        {
            get
            {
                return _renameTimeLineCommand ??= new MyActionCommand(
                    _ => { TimeLinesViewModel.RenameTimeLine(this); },
                    _ => true
                );
            }
        }

        /// <summary>
        /// Command to create a new timeline item for this timeline.
        /// </summary>
        public ICommand NewTimeLineItemCommand
        {
            get
            {
                return _newTimeLineItemCommand ??= new MyActionCommand(
                    _ => { TimeLinesViewModel.CreateTimeLineItemOn(this); },
                    _ => true
                );
            }
        }

        /// <summary>
        /// Command to create a new timeline at the position of this timeline.
        /// </summary>
        public ICommand NewTimeLineCommand =>
            _newTimeLineCommand ??= new MyActionCommand(
                async _ => { await TimeLinesViewModel.CreateTimeLineOn(this); },
                _ => true);

        /// <summary>
        /// Is true if the timeline is modified.
        /// </summary>
        public bool IsModified
        {
            get => _isModified;
            set
            {
                if (value != _isModified)
                {
                    _isModified = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// If true, show UTC time; otherwise, show local time.
        /// </summary>
        public bool IsUniversalTime
        {
            get => _isUniversalTime;
            set
            {
                if (value != _isUniversalTime)
                {
                    _isUniversalTime = value;

                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The timeline item view models only for the current horizontal viewport.
        /// </summary>
        public ICollectionView TimeLineItemCollectionView
        {
            get
            {
                EnsureVisibleTimeLineItems();
                // Create the dispatcher-bound view on first use by the UI,
                // rather than while the model is being loaded.
                return _timeLineItemCollectionView ??= CollectionViewSource.GetDefaultView(_visibleItems);
            }
        }

        /// <summary>
        /// The timeline name.
        /// </summary>
        public string Name
        {
            get => TimeLineModel.Name;
            set
            {
                if (TimeLineModel.Name != value)
                {
                    TimeLineModel.Name = value;

                    IsModified = true;

                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The timeline tool-tip.
        /// </summary>
        public string ToolTip => TimeLineModel.Name;

        /// <summary>
        /// The timeline height [pixel].
        /// </summary>
        public double Height => _settingsViewModel.TimeLineHeight;

        /// <summary>
        /// Is true if the timeline is loaded.
        /// </summary>
        public bool IsLoaded
        {
            get => _isLoaded;

            set
            {
                if (_isLoaded != value)
                {
                    _isLoaded = value;
                    Loaded?.Invoke(this, null);
                }
            }
        }

        /// <summary>
        /// Is called when a property of the timelines view has changed.
        /// </summary>
        internal void TimeLinesViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(TimeLinesViewModel.Scale):
                    InvalidateHorizontalViewport();
                    break;

                case nameof(TimeLinesViewModel.HorizontalScrollOffset):
                    if (IsInVerticalViewport)
                        RefreshHorizontalViewport();
                    else
                        _hasDeferredScrollUpdate = true;
                    break;

                case nameof(TimeLinesViewModel.TimeLinesVisibleWidth):
                    _hasDeferredScrollUpdate = true;
                    RefreshDeferredScrollUpdate();
                    break;

                case nameof(TimeLinesViewModel.TimeLineCollectionView):
                case null:
                case "":
                    RefreshDeferredScrollUpdate();
                    break;
            }
        }

        private void RefreshDeferredScrollUpdate()
        {
            if (!_hasDeferredScrollUpdate || !IsInVerticalViewport)
                return;

            _hasDeferredScrollUpdate = false;
            RefreshHorizontalViewport();
        }

        private void InvalidateHorizontalViewport()
        {
            _visibleTimeLineItems = null;
            if (IsInVerticalViewport)
                RefreshHorizontalViewport();
            else
                _hasDeferredScrollUpdate = true;
        }

        private void EnsureVisibleTimeLineItems()
        {
            if (_visibleTimeLineItems == null)
            {
                _visibleTimeLineItems = CalculateVisibleTimeLineItems();
                SynchronizeVisibleItems(_visibleTimeLineItems);
            }
        }

        private HashSet<TimeLineItemViewModel> CalculateVisibleTimeLineItems()
        {
            return _timeLineItems.Where(item => item.IsInHorizontalViewport).ToHashSet();
        }

        private void RefreshHorizontalViewport()
        {
            HashSet<TimeLineItemViewModel> oldVisibleItems = _visibleTimeLineItems;
            HashSet<TimeLineItemViewModel> newVisibleItems = CalculateVisibleTimeLineItems();

            IEnumerable<TimeLineItemViewModel> changedItems = oldVisibleItems == null
                ? newVisibleItems
                : oldVisibleItems.Union(newVisibleItems);

            _visibleTimeLineItems = newVisibleItems;

            foreach (TimeLineItemViewModel item in changedItems)
                item.RefreshViewportGeometry();

            // Preserve the containers of surviving items even when a neighbour
            // enters or leaves the viewport.
            if (oldVisibleItems == null || !oldVisibleItems.SetEquals(newVisibleItems))
                SynchronizeVisibleItems(newVisibleItems);
        }

        private void SynchronizeVisibleItems(HashSet<TimeLineItemViewModel> visibleItems)
        {
            for (int i = _visibleItems.Count - 1; i >= 0; i--)
                if (!visibleItems.Contains(_visibleItems[i]))
                    _visibleItems.RemoveAt(i);

            // Model order determines the visual order of coincident markers.
            // Surviving items retain their relative order; only insert missing ones.
            int index = 0;
            foreach (TimeLineItemViewModel item in _timeLineItems)
            {
                if (!visibleItems.Contains(item))
                    continue;

                if (index == _visibleItems.Count || !ReferenceEquals(_visibleItems[index], item))
                    _visibleItems.Insert(index, item);
                index++;
            }
        }

        /// <summary>
        /// Is called when a global setting has changed.
        /// </summary>
        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SettingsViewModel.IsCompactTimeGrid):
                    InvalidateHorizontalViewport();
                    break;
            }
        }

        private void TimeLineItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TimeLineItemViewModel.StartTime)
                || e.PropertyName == nameof(TimeLineItemViewModel.EndTime))
                InvalidateHorizontalViewport();
        }

        /// <summary>
        /// Delete the given timeline item.
        /// </summary>
        public bool DeleteTimeLineItem(TimeLineItemViewModel timeLineItem)
        {
            TimeLineModel.RemoveTimeLineItem(timeLineItem.TimeLineItemModel);
            bool isDeleted = _timeLineItems.Remove(timeLineItem);
            PropertyChangedEventManager.RemoveHandler(timeLineItem, TimeLineItem_PropertyChanged, "");
            timeLineItem.Dispose();

            InvalidateHorizontalViewport();

            return isDeleted;
        }

        /// <summary>
        /// Add the given timeline item to this timeline.
        /// </summary>
        public void AddTimeLineItem(TimeLineItemViewModel timeLineItem)
        {
            timeLineItem.TimeLineViewModel = this;
            TimeLineModel.AddTimeLineItem(timeLineItem.TimeLineItemModel);
            PropertyChangedEventManager.AddHandler(timeLineItem, TimeLineItem_PropertyChanged, "");
            _timeLineItems.Add(timeLineItem);
            InvalidateHorizontalViewport();
        }

        /// <summary>
        /// Implements the Dispose pattern.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    PropertyChangedEventManager.RemoveHandler(TimeLinesViewModel, TimeLinesViewModel_PropertyChanged, "");
                    PropertyChangedEventManager.RemoveHandler(_settingsViewModel, Settings_PropertyChanged, "");
                    PropertyChangedEventManager.RemoveHandler(this, TimeLinesViewModel.TimeLineViewModel_Propertychanged, "");

                    foreach (TimeLineItemViewModel timeLineItem in _timeLineItems)
                    {
                        PropertyChangedEventManager.RemoveHandler(timeLineItem, TimeLineItem_PropertyChanged, "");
                        timeLineItem.Dispose();
                    }

                    _timeLineItems = new List<TimeLineItemViewModel>();
                }

                _isDisposed = true;
            }
        }

        /// <summary>
        /// Get string representation of timeline view model.
        /// </summary>
        public override string ToString()
        {
            return TimeLineModel.Name;
        }

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
