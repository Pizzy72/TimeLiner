// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2026 Christian Pistor

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using TimeLiner.Common;
using TimeLiner.Models;
using TimeLiner.Properties;
using TimeLiner.UI;
using TimeLiner.Undo;

namespace TimeLiner.ViewModels
{
    /// <summary>
    /// The timelines view model.
    /// </summary>
    internal class TimeLinesViewModel : ViewModelBase
    {
        /// <summary>
        /// Reference to the global settings view model.
        /// </summary>
        private readonly SettingsViewModel _settingsViewModel;

        /// <summary>
        /// The timelines model.
        /// </summary>
        public TimeLinesModel TimeLinesModel { get; private set; }

        /// <summary>
        /// Manages the undo and redo stack for the timelines model.
        /// </summary>
        private readonly UndoManager _undoManager = new();

        /// <summary>
        /// Reference to the timeline scaling view model.
        /// </summary>
        private readonly TimeLineScalingViewModel _timeLineScalingViewModel;

        /// <summary>
        /// The timeline view models.
        /// </summary>
        private List<TimeLineViewModel> _timeLines = [];

        /// <summary>
        /// The total height of all timelines [pixel].
        /// </summary>
        private double _timeLinesTotalHeight;

        /// <summary>
        /// The currently selected timeline item.
        /// </summary>
        private TimeLineItemViewModel _selectedTimeLineItem;

        /// <see cref="TimeLinesVisibleWidth"/>
        private double _timeLinesVisibleWidth;

        /// <see cref="TimeLinesVisibleHeight"/>
        private double _timeLinesVisibleHeight;

        /// <see cref="IsLoading"/>
        private bool _isLoading;

        /// <summary>
        /// The number of remaining timelines to load.
        /// </summary>
        private int _timeLinesToLoad;

        /// <see cref="TimeLineItems"/>
        private List<TimeLineItemViewModel> _orderedTimeLineItems;

        /// <see cref="IsModified"/>
        private bool _isModified;

        /// <see cref="VerticalScrollOffset"/>
        private double _verticalScrollOffset;

        /// <see cref="HorizontalScrollOffset"/>
        private double _horizontalScrollOffset;

        /// <see cref="TotalStartTime"/>
        private DateTime _totalStartTime;

        /// <see cref="TotalEndTime"/>
        private DateTime _totalEndTime;

        /// <see cref="NewTimeLineCommand"/>
        private ICommand _newTimeLineCommand;

        /// <see cref="NewTimeLineItemCommand"/>
        private ICommand _newTimeLineItemCommand;

        /// <see cref="ZoomInCommand"/>
        private ICommand _zoomInCommand;

        /// <see cref="UndoCommand"/>
        private ICommand _undoCommand;

        /// <see cref="RedoCommand"/>
        private ICommand _redoCommand;

        /// <see cref="ZoomOutCommand"/>
        private ICommand _zoomOutCommand;

        /// <see cref="GotoFirstCommand"/>
        private ICommand _gotoFirstCommand;

        /// <see cref="GotoLastCommand"/>
        private ICommand _gotoLastCommand;

        /// <see cref="GotoNextCommand"/>
        private ICommand _gotoNextCommand;

        /// <see cref="GotoPreviousCommand"/>
        private ICommand _gotoPreviousCommand;

        /// <see cref="ResetStartLocatorCommand"/>
        private ICommand _resetStartLocatorCommand;

        /// <see cref="ResetEndLocatorCommand"/>
        private ICommand _resetEndLocatorCommand;

        /// <see cref="ScrollOneTimeLineUpCommand"/>
        private ICommand _scrollOneTimeLineUpCommand;

        /// <see cref="ScrollOneTimeLineDownCommand"/>
        private ICommand _scrollOneTimeLineDownCommand;

        /// <see cref="ScrollTimeLinePageUpCommand"/>
        private ICommand _scrollTimeLinePageUpCommand;

        /// <see cref="ScrollMultipleTimeLinesUpCommand"/>
        private ICommand _scrollMultipleTimeLinesDownCommand;

        /// <see cref="ScrollMultipleTimeLinesUpCommand"/>
        private ICommand _scrollMultipleTimeLinesUpCommand;

        /// <see cref="ScrollTimeLinePageDownCommand"/>
        private ICommand _scrollTimeLinePageDownCommand;

        /// <see cref="ScrollTimeLinesLeftCommand"/>
        private ICommand _scrollTimeLinesLeftCommand;

        /// <see cref="ScrollTimeLinesToStartCommand"/>
        private ICommand _scrollTimeLinesToStartCommand;

        /// <see cref="ScrollTimeLinesToEndCommand"/>
        private ICommand _scrollTimeLinesToEndCommand;

        /// <see cref="ScrollTimeLinesRightCommand"/>
        private ICommand _scrollTimeLinesRightCommand;

        /// <see cref="ScrollTimeLinesToTopCommand"/>
        private ICommand _scrollTimeLinesToTopCommand;

        /// <see cref="ScrollTimeLinesToBottomCommand"/>
        private ICommand _scrollTimeLinesToBottomCommand;

        /// <see cref="FindCommand"/>
        private ICommand _findCommand;

        /// <see cref="HelpCommand"/>
        private ICommand _helpCommand;

        /// <see cref="InfoCommand"/>
        private ICommand _infoCommand;

        /// <see cref="ScaleIndex"/>
        private ScaleIndex _scale = ScaleIndex.Second;

        /// <summary>
        /// Is raised when all timelines have been loaded.
        /// </summary>
        public event EventHandler Loaded;

        /// <summary>
        /// Constructor.
        /// </summary>
        public TimeLinesViewModel(IDialogService dialogService, SettingsViewModel settingsViewModel, TimeLineScalingViewModel timeLineScaling)
        {
            _settingsViewModel = settingsViewModel;
            _timeLineScalingViewModel = timeLineScaling;

            StartTimeLocatorViewModel = new TimeLocatorViewModel(this, _settingsViewModel, _timeLineScalingViewModel);
            EndTimeLocatorViewModel = new TimeLocatorViewModel(this, _settingsViewModel, _timeLineScalingViewModel);
            SelectedTimeLocatorViewModel = new TimeLocatorViewModel(this, _settingsViewModel, _timeLineScalingViewModel);
            TimeLineSelectorViewModel = new TimeLineSelectorViewModel(this, _settingsViewModel);

            ZoomToolViewModel = new ZoomToolViewModel(this, _settingsViewModel, _timeLineScalingViewModel);

            FindDialogViewModel = new FindDialogViewModel(dialogService, this);
            InfoDialogViewModel = new InfoDialogViewModel(dialogService, this);
            TimeLineDialogViewModel = new TimeLineDialogViewModel(dialogService, this);
            TimeLineItemDialogViewModel = new TimeLineItemDialogViewModel(dialogService, _settingsViewModel, this);

            PropertyChangedEventManager.AddHandler(_settingsViewModel, Settings_PropertyChanged, "");
        }

        /// <summary>
        /// The view model of the start-time locator.
        /// </summary>
        public TimeLocatorViewModel StartTimeLocatorViewModel { get; }

        /// <summary>
        /// The view model of the end-time locator.
        /// </summary>
        public TimeLocatorViewModel EndTimeLocatorViewModel { get; }

        /// <summary>
        /// The view model of the selected-time locator.
        /// </summary>
        public TimeLocatorViewModel SelectedTimeLocatorViewModel { get; }

        /// <summary>
        /// The view model of the timeline selector.
        /// </summary>
        public TimeLineSelectorViewModel TimeLineSelectorViewModel { get; }

        /// <summary>
        /// The view model of the zoom tool.
        /// </summary>
        public ZoomToolViewModel ZoomToolViewModel { get; }

        /// <summary>
        /// The view model of the info dialog.
        /// </summary>
        public InfoDialogViewModel InfoDialogViewModel { get; }

        /// <summary>
        /// The view model of the find dialog.
        /// </summary>
        public FindDialogViewModel FindDialogViewModel { get; }

        /// <summary>
        /// The view model of the timeline item dialog.
        /// </summary>
        public TimeLineItemDialogViewModel TimeLineItemDialogViewModel { get; }

        /// <summary>
        /// The view model of the timeline dialog.
        /// </summary>
        public TimeLineDialogViewModel TimeLineDialogViewModel { get; }



        /// <summary>
        /// Command for creating a new timeline with a timeline item.
        /// </summary>
        public ICommand NewTimeLineItemCommand => _newTimeLineItemCommand ??= new MyActionCommand(
                    _ => { CreateTimeLineAndItem(); },
                    _ => true);

        /// <summary>
        /// Command for creating an empty timeline.
        /// </summary>
        public ICommand NewTimeLineCommand => _newTimeLineCommand ??= new MyActionCommand(
                    async _ => { await CreateAndAppendTimeLine(); },
                    _ => true);

        /// <summary>
        /// Command for zooming out.
        /// </summary>
        public ICommand ZoomInCommand => _zoomInCommand ??= new MyActionCommand(
                    _ => { ZoomIn(); },
                    _ => HasTimeLineItems);

        /// <summary>
        /// Undo the last change.
        /// </summary>
        public ICommand UndoCommand => _undoCommand ??= new MyActionCommand(
            async _ =>
            {
                await UndoAsync();
            },
            _ => _undoManager.CanUndo);


        /// <summary>
        /// Redo the last change.
        /// </summary>
        public ICommand RedoCommand => _redoCommand ??= new MyActionCommand(
            async _ =>
            {
                await RedoAsync();
            },
            _ => _undoManager.CanRedo);


        /// <summary>
        /// Captures the current model state for undo.
        /// </summary>
        public async Task CaptureUndoAsync()
        {
            await _undoManager.CaptureAsync(TimeLinesModel);
            CommandManager.InvalidateRequerySuggested();
        }

        /// <summary>
        /// Command for zooming in.
        /// </summary>
        public ICommand ZoomOutCommand => _zoomOutCommand ??= new MyActionCommand(
                    _ => { ZoomOut(); },
                    _ => HasTimeLineItems);

        /// <summary>
        /// Command for selecting the first timeline item.
        /// </summary>
        public ICommand GotoFirstCommand => _gotoFirstCommand ??= new MyActionCommand(
                    _ => { SelectFirstTimeLineItem(); },
                    _ => HasTimeLineItems);

        /// <summary>
        /// Command for selecting the last timeline item.
        /// </summary>
        public ICommand GotoLastCommand => _gotoLastCommand ??= new MyActionCommand(
                    _ => { SelectLastTimeLineItem(); },
                    _ => HasTimeLineItems);

        /// <summary>
        /// Command for selecting the next timeline item.
        /// </summary>
        public ICommand GotoNextCommand => _gotoNextCommand ??= new MyActionCommand(
                    _ => { SelectNextTimeLineItem(); },
                    _ => HasTimeLineItems);

        /// <summary>
        /// Command for selecting the previous timeline item.
        /// </summary>
        public ICommand GotoPreviousCommand => _gotoPreviousCommand ??= new MyActionCommand(
                    _ => { SelectPreviousTimeLineItem(); },
                    _ => HasTimeLineItems);

        /// <summary>
        /// Command for resetting the start-time locator.
        /// </summary>
        public ICommand ResetStartLocatorCommand => _resetStartLocatorCommand ??= new MyActionCommand(
                    _ => { ResetStartTimeLocator(); },
                    _ => HasTimeLineItems);

        /// <summary>
        /// Command for resetting the end-time locator.
        /// </summary>
        public ICommand ResetEndLocatorCommand => _resetEndLocatorCommand ??= new MyActionCommand(
                    _ => { ResetEndTimeLocator(); },
                    _ => HasTimeLineItems);

        /// <summary>
        /// Command for scrolling up by one timeline.
        /// </summary>
        public ICommand ScrollOneTimeLineUpCommand => _scrollOneTimeLineUpCommand ??= new MyActionCommand(
                    _ => { ScrollTimeLinesVertically(VerticalScrollDirection.OneTimeLineUp); },
                    _ => HasTimeLineItems);

        /// <summary>
        /// Command for scrolling down by one timeline.
        /// </summary>
        public ICommand ScrollOneTimeLineDownCommand => _scrollOneTimeLineDownCommand ??= new MyActionCommand(
                    _ => { ScrollTimeLinesVertically(VerticalScrollDirection.OneTimeLineDown); },
                    _ => HasTimeLineItems);

        /// <summary>
        /// Command for scrolling down by multiple timelines.
        /// </summary>
        public ICommand ScrollMultipleTimeLinesDownCommand => _scrollMultipleTimeLinesDownCommand ??= new MyActionCommand(
                    _ => { ScrollTimeLinesVertically(VerticalScrollDirection.MultipleTimeLinesDown); },
                    _ => HasTimeLineItems);

        /// <summary>
        /// Command for scrolling up by multiple timelines.
        /// </summary>
        public ICommand ScrollMultipleTimeLinesUpCommand => _scrollMultipleTimeLinesUpCommand ??= new MyActionCommand(
                    _ => { ScrollTimeLinesVertically(VerticalScrollDirection.MultipleTimeLinesUp); },
                    _ => HasTimeLineItems);

        /// <summary>
        /// Command for scrolling up the timelines by one page.
        /// </summary>
        public ICommand ScrollTimeLinePageUpCommand => _scrollTimeLinePageUpCommand ??= new MyActionCommand(
                    _ => { ScrollTimeLinesVertically(VerticalScrollDirection.TimeLinePageUp); },
                    _ => HasTimeLineItems);

        /// <summary>
        /// Command for scrolling the timelines down by one page.
        /// </summary>
        public ICommand ScrollTimeLinePageDownCommand => _scrollTimeLinePageDownCommand ??= new MyActionCommand(
                    _ => { ScrollTimeLinesVertically(VerticalScrollDirection.TimeLinePageDown); },
                    _ => HasTimeLineItems);

        /// <summary>
        /// Command for scrolling the timelines left.
        /// </summary>
        public ICommand ScrollTimeLinesLeftCommand => _scrollTimeLinesLeftCommand ??= new MyActionCommand(
                    _ => { ScrollTimeLinesHorizontally(HorizontalScrollDirection.OneGridElementLeft); },
                    _ => HasTimeLineItems);

        /// <summary>
        /// Command for scrolling the timelines horizontally to the start.
        /// </summary>
        public ICommand ScrollTimeLinesToStartCommand => _scrollTimeLinesToStartCommand ??= new MyActionCommand(
                    _ => { ScrollTimeLinesToStart(); },
                    _ => HasTimeLineItems);

        /// <summary>
        /// Command for scrolling the timelines horizontally to the end.
        /// </summary>
        public ICommand ScrollTimeLinesToEndCommand => _scrollTimeLinesToEndCommand ??= new MyActionCommand(
                    _ => { ScrollTimeLinesToEnd(); },
                    _ => HasTimeLineItems);

        /// <summary>
        /// Command for scrolling the timelines to the right.
        /// </summary>
        public ICommand ScrollTimeLinesRightCommand => _scrollTimeLinesRightCommand ??= new MyActionCommand(
                    _ => { ScrollTimeLinesHorizontally(HorizontalScrollDirection.OneGridElementRight); },
                    _ => HasTimeLineItems);

        /// <summary>
        /// Command for scrolling the timelines to the top.
        /// </summary>
        public ICommand ScrollTimeLinesToTopCommand => _scrollTimeLinesToTopCommand ??= new MyActionCommand(
                    _ => { ScrollTimeLinesToTop(); },
                    _ => HasTimeLineItems);

        /// <summary>
        /// Command for scrolling the timelines to the bottom.
        /// </summary>
        public ICommand ScrollTimeLinesToBottomCommand => _scrollTimeLinesToBottomCommand ??= new MyActionCommand(
                    _ => { ScrollTimeLinesToBottom(); },
                    _ => HasTimeLineItems);

        /// <summary>
        /// Command to open the find dialog.
        /// </summary>
        public ICommand FindCommand => _findCommand ??= new MyActionCommand(
                    _ => { FindDialogViewModel.ShowDialog(); },
                    _ => HasTimeLineItems);

        /// <summary>
        /// Command to open the online help.
        /// </summary>
        public ICommand HelpCommand
        {
            get
            {
                return _helpCommand ??= new MyActionCommand(
                    _ =>
                    {
                        string binDirPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                        if (binDirPath != null)
                        {
                            string helpFilePath = Path.Combine(binDirPath, "Help", "TimeLinerHelp.pdf");
                            System.Diagnostics.Process.Start(helpFilePath);
                        }
                    }
                );
            }
        }

        /// <summary>
        /// Command to open the info dialog.
        /// </summary>
        public ICommand InfoCommand
        {
            get
            {
                return _infoCommand ??= new MyActionCommand(
                    _ =>
                    {
                        InfoDialogViewModel.ShowDialog();
                    }
                    );
            }
        }

        /// <summary>
        /// The assembly title.
        /// </summary>
        public string AssemblyTitle
        {
            get
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                AssemblyTitleAttribute attribute = (AssemblyTitleAttribute)assembly.GetCustomAttribute(typeof(AssemblyTitleAttribute));

                return attribute.Title;
            }
        }

        /// <summary>
        /// The duration between the start of the first timeline item and the end of the last as string.
        /// </summary>
        public string DeltaFirstLastItem => TimeFormat.GetDurationString(TotalEndTime - TotalStartTime);

        /// <summary>
        /// The duration of all timeline items as string.
        /// </summary>
        public string DurationAllItems => TimeFormat.GetDurationString(GetAggregatedDuration());

        /// <summary>
        /// The path of the loaded file.
        /// </summary>
        public string FilePath => TimeLinesModel?.FilePath;

        /// <summary>
        /// Check if file path is set.
        /// </summary>
        public bool HasFilePath => !string.IsNullOrEmpty(FilePath);

        /// <summary>
        /// Is true if the model has timeline items.
        /// </summary>
        public bool HasTimeLineItems => TimeLinesModel?.TimeLineItemCount > 0;

        /// <summary>
        /// Is true if the view model is modified.
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
                    NotifyPropertyChanged(nameof(WindowTitle));
                }
            }
        }

        /// <summary>
        /// The delta between the start-time locator and the end-time locator.
        /// </summary>
        public string LocatorDelta => StartTimeLocatorViewModel != null ?
            TimeFormat.GetDurationString(EndTimeLocatorViewModel.Time - StartTimeLocatorViewModel.Time) : "";

        /// <summary>
        /// The timeline scale.
        /// </summary>
        public ScaleIndex Scale
        {
            get => _scale;
            set
            {
                if (_scale == value)
                {
                    return;
                }

                if (!Enum.IsDefined(typeof(ScaleIndex), value))
                {
                    return;
                }

                double oldScaleValue = _timeLineScalingViewModel.GetScaleValue(_scale);

                _scale = value;

                double newScaleValue = _timeLineScalingViewModel.GetScaleValue(_scale);

                HorizontalScrollOffset *= newScaleValue / oldScaleValue;

                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(ScaleText));
                NotifyPropertyChanged(nameof(ScaleUnit));
                NotifyPropertyChanged(nameof(ScaleInterval));
                NotifyPropertyChanged(nameof(LocatorDelta));
                NotifyPropertyChanged(nameof(HorizontalScrollMaximum));
            }
        }

        /// <summary>
        /// The default X-position for a time locator in pixels.
        /// </summary>
        private double TimeLocatorDefaultPosition => _settingsViewModel.TimeLineSpacerLeft;

        /// <summary>
        /// The scale interval.
        /// </summary>
        public int ScaleInterval => _timeLineScalingViewModel.GetScaleInterval(Scale);

        /// <summary>
        /// The timeline scale as text.
        /// </summary>
        public string ScaleText => _timeLineScalingViewModel.GetScaleText(Scale);

        /// <summary>
        /// The scale unit.
        /// </summary>
        public string ScaleUnit => _timeLineScalingViewModel.GetScaleUnit(Scale);

        /// <summary>
        /// The offset of the vertical scrollbar [pixel].
        /// </summary>
        public double VerticalScrollOffset
        {
            get => _verticalScrollOffset;

            set
            {
                double verticalScrollOffset = Math.Min(
                    Math.Max(Math.Ceiling(value / _settingsViewModel.TimeLineHeight)
                    * _settingsViewModel.TimeLineHeight, 0), VerticalScrollMaximum);

                if (Math.Abs(_verticalScrollOffset - verticalScrollOffset) > double.Epsilon)
                {
                    _verticalScrollOffset = verticalScrollOffset;

                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(TimeLineCollectionView));
                }
            }
        }

        /// <summary>
        /// The offset of the horizontal scrollbar [pixel].
        /// </summary>
        public double HorizontalScrollOffset
        {
            get => _horizontalScrollOffset;
            set
            {
                double horizontalScrollOffset = Math.Min(Math.Max(value, 0), HorizontalScrollMaximum);

                if (Math.Abs(horizontalScrollOffset - _horizontalScrollOffset) > double.Epsilon)
                {
                    _horizontalScrollOffset = horizontalScrollOffset;

                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(GridOffsetHorizontal));
                }
            }
        }

        /// <summary>
        /// The horizontal offset of the background grid [pixel].
        /// </summary>
        public double GridOffsetHorizontal => -(HorizontalScrollOffset % _settingsViewModel.TimeGridWidth);

        /// <summary>
        /// The start time of the first timeline item.
        /// </summary>
        public DateTime TotalStartTime
        {
            get => _totalStartTime;
            set
            {
                if (value != _totalStartTime)
                {
                    _totalStartTime = value;

                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(TotalStartTimeText));
                    NotifyPropertyChanged(nameof(HorizontalScrollMaximum));
                }
            }
        }

        /// <summary>
        /// The end time of the last timeline item.
        /// </summary>
        public DateTime TotalEndTime
        {
            get => _totalEndTime;
            private set
            {
                if (value != _totalEndTime)
                {
                    _totalEndTime = value;

                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(TotalEndTimeText));
                    NotifyPropertyChanged(nameof(HorizontalScrollMaximum));
                }
            }
        }

        /// <summary>
        /// The timeline view models only for the current vertical viewport.
        /// </summary>
        public ICollectionView TimeLineCollectionView
        {
            get
            {
                if (_timeLines == null)
                {
                    return null;
                }

                ICollectionView view = CollectionViewSource.GetDefaultView(_timeLines);

                double timeLinesTop = 0;
                double timeLinesBottom = TimeLinesVisibleHeight;

                int timeLineIndex = 0;

                view.Filter = o =>
                {
                    TimeLineViewModel timeLine = (TimeLineViewModel)o;

                    double timeLineTop = timeLineIndex * timeLine.Height - VerticalScrollOffset;
                    bool isVisible = timeLineTop >= timeLinesTop && timeLineTop < timeLinesBottom;

                    timeLineIndex++;

                    return isVisible;
                };

                return view;
            }
        }

        /// <summary>
        /// The pixel value to add to or subtract from the vertical scrollbar value on a small change.
        /// </summary>
        public double VerticalScrollSmallChange => _settingsViewModel.TimeLineHeight;

        /// <summary>
        /// The pixel value to add to or subtract from the vertical scrollbar value on a large change.
        /// </summary>
        public double VerticalScrollLargeChange => _settingsViewModel.TimeLineHeight * 5;

        /// <summary>
        /// The pixel value to add to or subtract from the horizontal scrollbar value on a small change.
        /// </summary>
        public double HorizontalScrollSmallChange => _settingsViewModel.TimeGridWidth;

        /// <summary>
        /// The pixel value to add to or subtract from the horizontal scrollbar value on a large change.
        /// </summary>
        public double HorizontalScrollLargeChange => _settingsViewModel.TimeGridWidth * 5;

        /// <summary>
        /// The minimum value of the vertical scrollbar [pixel].
        /// </summary>
        public double VerticalScrollMinimum => 0d;

        /// <summary>
        /// The maximum value of the vertical scrollbar [pixel].
        /// </summary>
        public double VerticalScrollMaximum
        {
            get
            {
                double offscreen = _timeLinesTotalHeight - TimeLinesVisibleHeight;

                double verticalScrollMaximum = Math.Ceiling(Math.Max(offscreen, 0) / _settingsViewModel.TimeLineHeight)
                    * _settingsViewModel.TimeLineHeight;

                return verticalScrollMaximum;
            }
        }

        /// <summary>
        /// The minimum value of the horizontal scrollbar [pixel].
        /// </summary>
        public double HorizontalScrollMinimum => 0d;

        /// <summary>
        /// The maximum value of the horizontal scrollbar [pixel].
        /// </summary>
        public double HorizontalScrollMaximum
        {
            get
            {
                TimeSpan totalDuration = TotalEndTime - TotalStartTime;
                double timeLinesTotalWidth = _timeLineScalingViewModel.CalculatePixels(totalDuration, Scale)
                    + _settingsViewModel.TimeLineSpacerLeft
                    + _settingsViewModel.TimeLineSpacerRight;

                double offscreen = timeLinesTotalWidth - TimeLinesVisibleWidth;
                double maximum = Math.Max(offscreen, 0);

                return maximum;
            }
        }

        /// <summary>
        /// The visible width of the timelines [pixel].
        /// </summary>
        public double TimeLinesVisibleWidth
        {
            get => _timeLinesVisibleWidth;
            set
            {
                if (value > 0 && Math.Abs(value - _timeLinesVisibleWidth) > double.Epsilon)
                {
                    _timeLinesVisibleWidth = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(HorizontalScrollMaximum));
                }
            }
        }

        /// <summary>
        /// The visible height of the timelines [pixel].
        /// </summary>
        public double TimeLinesVisibleHeight
        {
            get => _timeLinesVisibleHeight;
            set
            {
                if (value > 0 && Math.Abs(value - _timeLinesVisibleHeight) > double.Epsilon)
                {
                    _timeLinesVisibleHeight = value;

                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(VerticalScrollMaximum));
                    NotifyPropertyChanged(nameof(TimeLineCollectionView));
                }
            }
        }

        /// <summary>
        /// The time utilization.
        /// </summary>
        public double TimeUtilization
        {
            get
            {
                if (TimeLinesModel == null)
                {
                    return 0;
                }

                double totalDelta = (TotalEndTime - TotalStartTime).Ticks;

                if (totalDelta > 0)
                {
                    double totalDuration = GetAggregatedDuration().Ticks;
                    double utilization = totalDuration / totalDelta;

                    return utilization;
                }

                return 0d;
            }
        }

        /// <summary>
        /// The end-time of the last timeline item as text.
        /// </summary>
        public string TotalEndTimeText => TimeFormat.GetTimeString(TotalEndTime, _settingsViewModel);

        /// <summary>
        /// The start-time of the first timeline item as text.
        /// </summary>
        public string TotalStartTimeText => TimeFormat.GetTimeString(TotalStartTime, _settingsViewModel);

        /// <summary>
        /// The window title.
        /// </summary>
        public string WindowTitle => $"{(HasFilePath ? Path.GetFileName(FilePath) : Resources.TitleNewFile)}{(IsModified ? " * " : "")} - {AssemblyTitle}";

        /// <summary>
        /// If true, minimize the ribbon menu; otherwise, expand it.
        /// </summary>
        public bool IsRibbonMenuMinimized => _settingsViewModel.IsMinimalUi;

        /// <summary>
        /// If true, minimize the info text; otherwise, expand it.
        /// </summary>
        public bool IsInfoTextExpanded => !_settingsViewModel.IsMinimalUi;

        /// <summary>
        /// Is true if the timelines model is being loaded.
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The view models of the timelines.
        /// </summary>
        public IReadOnlyList<TimeLineViewModel> TimeLines => _timeLines.AsReadOnly();

        /// <summary>
        /// The view models of the timeline items ordered by start time.
        /// </summary>
        public IReadOnlyList<TimeLineItemViewModel> TimeLineItems
        {
            get
            {
                _orderedTimeLineItems ??= _timeLines.SelectMany(tl => tl.TimeLineItems)
                    .OrderBy(tli => tli.StartTime).ToList();

                return _orderedTimeLineItems.AsReadOnly();
            }
        }


        /// <summary>
        /// Create empty model.
        /// </summary>
        internal void Create(double timeLinesVisibleWidth)
        {
            CleanUp();

            _timeLinesVisibleWidth = timeLinesVisibleWidth;

            TimeLinesModel = new TimeLinesModel();
            IsModified = false;

            TotalStartTime = DateTime.Now;
            TotalEndTime = TotalStartTime + TimeSpan.FromMinutes(10);

            Scale = GetFittingScale(TotalStartTime, TotalEndTime, _timeLinesVisibleWidth);

            ResetStartTimeLocator();
            ResetEndTimeLocator();
            ResetSelectedTimeLocator();

            NotifyAllPropertiesChanged();
        }


        /// <summary>
        /// Load model from file.
        /// </summary>
        public async Task LoadAsync(string filePath, double timeLinesVisibleWidth)
        {
            if (IsLoading)
            {
                return;
            }

            if (!File.Exists(filePath))
            {
                throw new TimeLinerException(string.Format(Resources.ErrorFileDoesNotExist, filePath));
            }

            try
            {
                IsLoading = true;

                CleanUp();

                _timeLinesVisibleWidth = timeLinesVisibleWidth;

                TimeLinesModel loadedModel = await TimeLinesModel.LoadAsync(filePath);

                _undoManager.Clear();
                IsModified = false;

                ApplyTimeLinesModel(loadedModel);
            }
            catch (Exception ex)
            {
                throw new TimeLinerException(string.Format(Resources.ErrorCouldNotLoadFile, filePath), ex);
            }
            finally
            {
                IsLoading = false;
                NotifyAllPropertiesChanged();
            }
        }

        /// <summary>
        /// Undo the last change.
        /// </summary>
        public async Task UndoAsync()
        {
            if (!_undoManager.CanUndo)
            {
                return;
            }

            await ApplyUndoRedoAsync(() => _undoManager.UndoAsync(TimeLinesModel));
        }

        /// <summary>
        /// Redo the last undone change.
        /// </summary>
        public async Task RedoAsync()
        {
            if (!_undoManager.CanRedo)
            {
                return;
            }

            await ApplyUndoRedoAsync(() => _undoManager.RedoAsync(TimeLinesModel));
        }

        /// <summary>
        /// Apply undo or redo while preserving the view state.
        /// </summary>
        private async Task ApplyUndoRedoAsync(Func<Task<TimeLinesModel>> restoreModelAsync)
        {
            double horizontalScrollOffset = HorizontalScrollOffset;
            double verticalScrollOffset = VerticalScrollOffset;
            ScaleIndex scale = Scale;

            TimeLinesModel restoredModel = await restoreModelAsync();

            ApplyTimeLinesModel(restoredModel);

            HorizontalScrollOffset = horizontalScrollOffset;
            VerticalScrollOffset = verticalScrollOffset;
            Scale = scale;

            IsModified = !_undoManager.IsAtSavedVersion;

            CommandManager.InvalidateRequerySuggested();
            NotifyAllPropertiesChanged();
        }

        /// <summary>
        /// Apply the given model to the view model.
        /// </summary>
        private void ApplyTimeLinesModel(TimeLinesModel model)
        {
            CleanUp();

            TimeLinesModel = model;

            UpdateTotalStartTime();
            UpdateTotalEndTime();

            Scale = GetFittingScale(
                TotalStartTime,
                TotalEndTime,
                _timeLinesVisibleWidth
                );

            foreach (TimeLineModel timeLineModel in TimeLinesModel.TimeLines)
            {
                TimeLineViewModel timeLineViewModel = new(
                    this,
                    timeLineModel,
                    _settingsViewModel,
                    _timeLineScalingViewModel);

                timeLineViewModel.Loaded += TimeLineViewModel_Loaded;

                AddTimeLine(timeLineViewModel);
            }

            _timeLinesToLoad = TimeLineCollectionView
                .Cast<TimeLineViewModel>()
                .Count();

            UpdateTimeLinesTotalHeight();

            ResetStartTimeLocator();
            ResetEndTimeLocator();
            ResetSelectedTimeLocator();
        }

        /// <summary>
        /// Save model to file.
        /// </summary>
        public async Task SaveAsync(string filePath)
        {
            await TimeLinesModel.SaveAsync(filePath);

            _undoManager.MarkSaved();

            IsModified = false;

            NotifyAllPropertiesChanged();
        }

        /// <summary>
        /// Select next timeline item.
        /// </summary>
        public void SelectNextTimeLineItem()
        {
            DateTime startTime = _selectedTimeLineItem != null ?
                _selectedTimeLineItem.StartTime.AddMilliseconds(1) : GetScrollOffsetTime();

            TimeLineItemViewModel nextTimeLineItem = TimeLineItems
                .FirstOrDefault(timeLineItem => timeLineItem.StartTime >= startTime);

            if (nextTimeLineItem != null)
                SelectTimeLineItem(nextTimeLineItem);
            else
                SelectLastTimeLineItem();
        }

        /// <summary>
        /// Select previous timeline item.
        /// </summary>
        private void SelectPreviousTimeLineItem()
        {
            DateTime startTime = _selectedTimeLineItem?.StartTime ?? GetScrollOffsetTime();

            TimeLineItemViewModel previousTimeLineItem = TimeLineItems
                .LastOrDefault(timeLineItem => timeLineItem.StartTime < startTime);

            if (previousTimeLineItem != null)
                SelectTimeLineItem(previousTimeLineItem);
            else
                SelectFirstTimeLineItem();
        }

        /// <summary>
        /// Select first timeline item.
        /// </summary>
        public void SelectFirstTimeLineItem()
        {
            TimeLineItemViewModel timeLineItem = TimeLineItems.FirstOrDefault();
            SelectTimeLineItem(timeLineItem);
        }

        /// <summary>
        /// Select timeline item.
        /// </summary>
        public void SelectLastTimeLineItem()
        {
            TimeLineItemViewModel timeLineItem = TimeLineItems.LastOrDefault();
            SelectTimeLineItem(timeLineItem);
        }

        /// <summary>
        /// Select given timeline item, and scroll if necessary.
        /// </summary>
        public void SelectTimeLineItem(TimeLineItemViewModel timeLineItem)
        {
            DeselectTimeLineItem();

            _selectedTimeLineItem = timeLineItem;
            _selectedTimeLineItem.IsSelected = true;

            double timeLineItemLeft = _timeLineScalingViewModel.CalculatePixels(timeLineItem.StartTime - TotalStartTime, Scale);

            if (timeLineItemLeft < HorizontalScrollOffset || timeLineItemLeft > HorizontalScrollOffset + TimeLinesVisibleWidth)
            {
                HorizontalScrollOffset = timeLineItemLeft;
            }

            int timeLineIndex = _timeLines.IndexOf(timeLineItem.TimeLineViewModel);
            double timeLineItemTop = timeLineIndex * timeLineItem.TimeLineViewModel.Height;

            if (timeLineItemTop < VerticalScrollOffset || timeLineItemTop > VerticalScrollOffset + TimeLinesVisibleHeight)
            {
                VerticalScrollOffset = timeLineItemTop;
            }
        }

        /// <summary>
        /// Deselect selected timeline item.
        /// </summary>
        public void DeselectTimeLineItem()
        {
            if (_selectedTimeLineItem != null)
            {
                _selectedTimeLineItem.IsSelected = false;
            }
            _selectedTimeLineItem = null;
        }

        /// <summary>
        /// Is called when a property of the time locator view model has changed.
        /// </summary>
        public void TimeLocatorViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(TimeLocatorViewModel.Time):
                    NotifyPropertyChanged(nameof(LocatorDelta));
                    break;
            }
        }

        /// <summary>
        /// Is called when a property of a timeline view model has changed.
        /// </summary>
        public void TimeLineViewModel_Propertychanged(object sender, PropertyChangedEventArgs e)
        {
            TimeLineViewModel timeLine = (TimeLineViewModel)sender;

            switch (e.PropertyName)
            {
                case nameof(TimeLineViewModel.IsModified):
                    IsModified = timeLine.IsModified;
                    break;
            }
        }

        /// <summary>
        /// Is called when a property of a timeline item view model has changed.
        /// </summary>
        public void TimeLineItemViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            TimeLineItemViewModel timeLineItem = (TimeLineItemViewModel)sender;

            switch (e.PropertyName)
            {
                case nameof(TimeLineItemViewModel.StartTime):
                    UpdateTotalStartTime();
                    NotifyPropertyChanged(nameof(TimeUtilization));
                    break;

                case nameof(TimeLineItemViewModel.EndTime):
                    UpdateTotalEndTime();
                    NotifyPropertyChanged(nameof(TimeUtilization));
                    break;

                case nameof(TimeLineItemViewModel.IsModified):
                    IsModified = timeLineItem.IsModified;
                    break;
            }
        }

        /// <summary>
        /// Reset the start-time locator.
        /// </summary>
        private void ResetStartTimeLocator()
        {
            StartTimeLocatorViewModel.X = HasTimeLineItems ? GetXPostionOfFirstTimeLineItem() : TimeLocatorDefaultPosition;
        }

        /// <summary>
        /// Reset the end-time locator.
        /// </summary>
        private void ResetEndTimeLocator()
        {
            EndTimeLocatorViewModel.X = HasTimeLineItems ? GetXPositionOfLastTimeLineItem() : TimeLocatorDefaultPosition;
        }

        /// <summary>
        /// Reset the selected-time locator.
        /// </summary>
        private void ResetSelectedTimeLocator()
        {
            StartTimeLocatorViewModel.X = HasTimeLineItems ? GetXPostionOfFirstTimeLineItem() : TimeLocatorDefaultPosition;
        }

        /// <summary>
        /// Scroll timelines horizontally to the start.
        /// </summary>
        private void ScrollTimeLinesToStart()
        {
            HorizontalScrollOffset = 0d;
        }

        /// <summary>
        /// Scroll timelines horizontally to the end.
        /// </summary>
        private void ScrollTimeLinesToEnd()
        {
            HorizontalScrollOffset = HorizontalScrollMaximum;
        }

        /// <summary>
        /// The direction to scroll the timelines horizontally.
        /// </summary>
        enum HorizontalScrollDirection
        {
            OneGridElementRight,
            OneGridElementLeft,
        }

        /// <summary>
        /// Scroll timelines right/left by one grid element.
        /// </summary>
        private void ScrollTimeLinesHorizontally(HorizontalScrollDirection direction)
        {
            switch (direction)
            {
                case HorizontalScrollDirection.OneGridElementRight:
                    HorizontalScrollOffset += HorizontalScrollSmallChange;
                    break;

                case HorizontalScrollDirection.OneGridElementLeft:
                    HorizontalScrollOffset -= HorizontalScrollSmallChange;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, "Invalid direction");

            }
        }

        /// <summary>
        /// The direction to scroll the timelines vertically.
        /// </summary>
        enum VerticalScrollDirection
        {
            OneTimeLineUp,
            OneTimeLineDown,
            MultipleTimeLinesUp,
            MultipleTimeLinesDown,
            TimeLinePageUp,
            TimeLinePageDown
        }

        /// <summary>
        /// Scroll timelines vertically.
        /// </summary>
        private void ScrollTimeLinesVertically(VerticalScrollDirection direction)
        {
            switch (direction)
            {
                case VerticalScrollDirection.OneTimeLineUp:
                    VerticalScrollOffset += VerticalScrollSmallChange;
                    break;
                case VerticalScrollDirection.OneTimeLineDown:
                    VerticalScrollOffset -= VerticalScrollSmallChange;
                    break;
                case VerticalScrollDirection.MultipleTimeLinesUp:
                    VerticalScrollOffset += VerticalScrollLargeChange;
                    break;
                case VerticalScrollDirection.MultipleTimeLinesDown:
                    VerticalScrollOffset -= VerticalScrollLargeChange;
                    break;
                case VerticalScrollDirection.TimeLinePageUp:
                    VerticalScrollOffset += TimeLinesVisibleHeight;
                    return;
                case VerticalScrollDirection.TimeLinePageDown:
                    VerticalScrollOffset -= TimeLinesVisibleHeight;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, "Invalid direction");

            }
        }


        /// <summary>
        /// Scroll timelines to the bottom.
        /// </summary>
        private void ScrollTimeLinesToBottom()
        {
            VerticalScrollOffset = VerticalScrollMaximum;
        }

        /// <summary>
        /// Scroll timelines to the top.
        /// </summary>
        private void ScrollTimeLinesToTop()
        {
            VerticalScrollOffset = 0;
        }

        /// <summary>
        /// Get aggregated duration of all timeline items.
        /// </summary>
        private TimeSpan GetAggregatedDuration()
        {
            if (TimeLinesModel == null)
                return TimeSpan.Zero;

            TimeSpan duration = new(
                TimeLinesModel.TimeLines
                .SelectMany(tl => tl.TimeLineItems)
                .Select(tli => tli.EndTime - tli.StartTime)
                .Sum(s => s.Ticks)
                );

            return duration;
        }

        /// <summary>
        /// Get X position of first timeline item.
        /// </summary>
        private double GetXPostionOfFirstTimeLineItem()
        {
            return _settingsViewModel.TimeLineSpacerLeft - HorizontalScrollOffset;
        }

        /// <summary>
        /// Get X position of last timeline item.
        /// </summary>
        private double GetXPositionOfLastTimeLineItem()
        {
            TimeSpan span = GetEndTimeOfLastTimeLineItem() - TotalStartTime;
            double x = _timeLineScalingViewModel.CalculatePixels(span, Scale) + _settingsViewModel.TimeLineSpacerLeft - HorizontalScrollOffset;
            return x;
        }

        /// <summary>
        /// Get start time of first timeline item.
        /// </summary>
        private DateTime GetStartTimeOfFirstTimeLineItem()
        {
            DateTime startTime = TimeLinesModel.TimeLines
                          .SelectMany(tl => tl.TimeLineItems)
                          .OrderBy(tli => tli.StartTime)
                          .Select(tli => tli.StartTime)
                          .FirstOrDefault();

            return startTime;
        }

        /// <summary>
        /// Get time of last timeline item.
        /// </summary>
        private DateTime GetEndTimeOfLastTimeLineItem()
        {
            DateTime endTime = TimeLinesModel.TimeLines
                .SelectMany(tl => tl.TimeLineItems)
                .OrderBy(tli => tli.EndTime)
                .Select(tli => tli.EndTime)
                .LastOrDefault();

            return endTime;
        }

        /// <summary>
        /// Get the scale which fits all timeline items into the given pixel width.
        /// </summary>
        private ScaleIndex GetFittingScale(DateTime startTime, DateTime endTime, double widthToFit)
        {
            TimeSpan timespan = endTime - startTime;

            ScaleIndex? suitableScale = null;

            foreach (ScaleIndex scale in _timeLineScalingViewModel.Scales)
            {
                double width = _timeLineScalingViewModel.CalculatePixels(timespan, scale);

                if (width > widthToFit)
                {
                    break;
                }

                suitableScale = scale;
            }

            if (suitableScale.HasValue)
            {
                return suitableScale.Value;
            }

            return _timeLineScalingViewModel.Scales.First();
        }

        /// <summary>
        /// Get time of scroll offset.
        /// </summary>
        private DateTime GetScrollOffsetTime()
        {
            double offsetPixels = HorizontalScrollOffset - _settingsViewModel.TimeLineSpacerLeft;
            double offsetSeconds = _timeLineScalingViewModel.CalculateSeconds(offsetPixels, Scale);
            DateTime offsetTime = TotalStartTime.AddSeconds(offsetSeconds);

            return offsetTime;
        }

        /// <summary>
        /// Is called when a global setting has changed.
        /// </summary>
        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SettingsViewModel.IsUniversalTime):
                case nameof(SettingsViewModel.TimeZone):

                    NotifyPropertyChanged(nameof(TotalEndTimeText));
                    NotifyPropertyChanged(nameof(TotalStartTimeText));

                    break;

                case nameof(SettingsViewModel.IsCompactTimeLines):

                    // Store the old maximum before updating
                    double oldMaximum = VerticalScrollMaximum;

                    // Calculate relative scroll position (0.0 = top, 1.0 = bottom)
                    double relativePosition = oldMaximum > 0 ? _verticalScrollOffset / oldMaximum : 0;

                    // Update the total height with the new timeline height
                    UpdateTimeLinesTotalHeight();

                    // Calculate new scroll offset based on relative position
                    double newScrollOffset = relativePosition * VerticalScrollMaximum;

                    // Apply grid snapping manually to avoid drift from the setter
                    _verticalScrollOffset = Math.Min(
                        Math.Max(Math.Ceiling(newScrollOffset / _settingsViewModel.TimeLineHeight)
                            * _settingsViewModel.TimeLineHeight, 0),
                        VerticalScrollMaximum);

                    // Notify about changes
                    NotifyPropertyChanged(nameof(VerticalScrollMaximum));
                    NotifyPropertyChanged(nameof(VerticalScrollSmallChange));
                    NotifyPropertyChanged(nameof(VerticalScrollLargeChange));
                    NotifyPropertyChanged(nameof(VerticalScrollOffset));
                    NotifyPropertyChanged(nameof(TimeLineCollectionView));

                    break;

                case nameof(SettingsViewModel.IsCompactTimeGrid):

                    double gridRatio = _settingsViewModel.IsCompactTimeGrid ?
                        SettingsViewModel.CompactTimeGridWith / SettingsViewModel.NormalTimeGridWidth :
                        SettingsViewModel.NormalTimeGridWidth / SettingsViewModel.CompactTimeGridWith;

                    HorizontalScrollOffset *= gridRatio;

                    NotifyPropertyChanged(nameof(HorizontalScrollMaximum));
                    NotifyPropertyChanged(nameof(HorizontalScrollSmallChange));
                    NotifyPropertyChanged(nameof(HorizontalScrollLargeChange));
                    NotifyPropertyChanged(nameof(ScaleInterval));

                    break;
            }
        }

        /// <summary>
        /// Zoom into timeline.
        /// </summary>
        private void ZoomIn()
        {
            Scale++;
        }

        /// <summary>
        /// Zoom out of timeline.
        /// </summary>
        private void ZoomOut()
        {
            Scale--;
        }

        /// <summary>
        /// Edit the given timeline item.
        /// </summary>
        internal void EditTimeLineItem(TimeLineItemViewModel timeLineItem)
        {
            if (TimeLineItemDialogViewModel.ShowDialog(timeLineItem, Resources.TitleEditItem))
            {
                UpdateTotalStartTime();
                UpdateTotalEndTime();

                NotifyAllPropertiesChanged();
            }
        }

        /// <summary>
        /// Create a new timeline item on the given timeline.
        /// </summary>
        public void CreateTimeLineItemOn(TimeLineViewModel targetTimeLine)
        {
            if (CreateTimeLineItemOnInternal(targetTimeLine))
            {
                _orderedTimeLineItems = null;

                UpdateTotalStartTime();
                UpdateTotalEndTime();

                NotifyAllPropertiesChanged();
            }
        }

        /// <summary>
        /// Create a new timeline at the end.
        /// </summary>
        public async Task CreateAndAppendTimeLine()
        {
            int index = _timeLines.Count;
            await CreateTimeLineOnInternal(index);
        }

        /// <summary>
        /// Create a new timeline at the position of the given timeline.
        /// </summary>
        public async Task CreateTimeLineOn(TimeLineViewModel targetTimeLine)
        {
            int index = _timeLines.IndexOf(targetTimeLine);
            await CreateTimeLineOnInternal(index);
        }

        /// <summary>
        /// Internal: create a new timeline and insert it at the given timeline index.
        /// </summary>
        private async Task CreateTimeLineOnInternal(int index)
        {
            await CaptureUndoAsync();

            TimeLineModel newTimeLineModel = new();
            TimeLineViewModel newTimeLineViewModel = new(this, newTimeLineModel, _settingsViewModel, _timeLineScalingViewModel);

            if (TimeLineDialogViewModel.ShowDialog(newTimeLineViewModel, Resources.TitleNewTimeLine))
            {
                TimeLinesModel.InsertTimeLine(index, newTimeLineModel);
                InsertTimeLine(index, newTimeLineViewModel);

                UpdateTimeLinesTotalHeight();

                NotifyAllPropertiesChanged();
            }
        }

        /// <summary>
        /// Create a new timeline with a new timeline item.
        /// </summary>
        private void CreateTimeLineAndItem()
        {
            TimeLineModel newTimeLineModel = new();
            TimeLineViewModel newTimeLineViewModel = new(this, newTimeLineModel, _settingsViewModel, _timeLineScalingViewModel);

            if (CreateTimeLineItemOnInternal(newTimeLineViewModel))
            {
                TimeLinesModel.AddTimeLine(newTimeLineModel);
                AddTimeLine(newTimeLineViewModel);

                _orderedTimeLineItems = null;

                UpdateTotalStartTime();
                UpdateTotalEndTime();

                UpdateTimeLinesTotalHeight();

                NotifyAllPropertiesChanged();
            }
        }

        /// <summary>
        /// Internal: create a new timeline item on the given timeline.
        /// </summary>
        private bool CreateTimeLineItemOnInternal(TimeLineViewModel timeLine)
        {
            DateTime startTime = SelectedTimeLocatorViewModel.Time;

            TimeLineItemModel timeLineItemModel = new()
            {
                Name = "",
                StartTime = startTime,
                EndTime = startTime,
                Color = SettingsViewModel.TimeLinerColors.TimeEvent.ToString()
            };

            TimeLineItemViewModel timeLineItemViewModel = new(timeLineItemModel, timeLine, _settingsViewModel, _timeLineScalingViewModel);

            if (TimeLineItemDialogViewModel.ShowDialog(timeLineItemViewModel, Resources.TitleNewItem))
            {
                timeLine.AddTimeLineItem(timeLineItemViewModel);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Delete the given timeline item.
        /// </summary>
        public async Task DeleteTimeLineItem(TimeLineItemViewModel timeLineItem)
        {
            await CaptureUndoAsync();

            TimeLinesModel.DeleteTimeLineItem(timeLineItem.TimeLineItemModel);

            _orderedTimeLineItems = null;

            foreach (TimeLineViewModel timeLine in _timeLines.ToList())
            {
                if (timeLine.DeleteTimeLineItem(timeLineItem))
                {
                    break;
                }
            }

            IsModified = true;

            UpdateTotalStartTime();
            UpdateTotalEndTime();

            NotifyAllPropertiesChanged();
        }

        /// <summary>
        /// Move the given timeline item to another timeline.
        /// </summary>
        public async Task MoveTimeLineItem(TimeLineItemViewModel itemToMove, TimeLineViewModel timeLine)
        {
            if (itemToMove.TimeLineViewModel == timeLine)
            {
                return;
            }

            await CaptureUndoAsync();

            foreach (TimeLineViewModel sourceTimeLine in _timeLines)
            {
                if (sourceTimeLine.DeleteTimeLineItem(itemToMove))
                {
                    break;
                }
            }

            timeLine.AddTimeLineItem(itemToMove);

            IsModified = true;

            UpdateTotalStartTime();
            UpdateTotalEndTime();

            NotifyAllPropertiesChanged();
        }

        /// <summary>
        /// Rename the given timeline.
        /// </summary>
        public void RenameTimeLine(TimeLineViewModel timeLine)
        {
            if (TimeLineDialogViewModel.ShowDialog(timeLine, Resources.TitleRenameTimeline))
            {
                NotifyAllPropertiesChanged();
            }
        }

        /// <summary>
        /// Delete the given timeline.
        /// </summary>
        public async Task DeleteTimeLine(TimeLineViewModel timeLine)
        {
            await CaptureUndoAsync();

            TimeLinesModel.RemoveTimeLine(timeLine.TimeLineModel);

            RemoveTimeLine(timeLine);
            timeLine.Dispose();

            _orderedTimeLineItems = null;

            IsModified = true;

            UpdateTotalStartTime();
            UpdateTotalEndTime();

            UpdateTimeLinesTotalHeight();

            VerticalScrollOffset -= _settingsViewModel.TimeLineHeight;

            NotifyAllPropertiesChanged();
        }

        /// <summary>
        /// Move the given timeline to the position of the other timeline.
        /// </summary>
        public async Task MoveTimeLine(TimeLineViewModel timeLineFrom, TimeLineViewModel timeLineTo)
        {
            if (timeLineFrom == timeLineTo)
            {
                return;
            }

            await CaptureUndoAsync();

            int oldIndex = _timeLines.IndexOf(timeLineFrom);
            int newIndex = _timeLines.IndexOf(timeLineTo);

            _timeLines.Move(oldIndex, newIndex);

            TimeLinesModel.MoveTimeLine(oldIndex, newIndex);

            IsModified = true;

            NotifyPropertyChanged(nameof(TimeLineCollectionView));
        }

        /// <summary>
        /// Add the given timeline to the view model list.
        /// </summary>
        private void AddTimeLine(TimeLineViewModel timeLine)
        {
            _timeLines.Add(timeLine);
        }

        /// <summary>
        /// Insert timeline at given index.
        /// </summary>
        private void InsertTimeLine(int index, TimeLineViewModel timeLine)
        {
            _timeLines.Insert(index, timeLine);
        }

        /// <summary>
        /// Remove the given timeline from the view model list.
        /// </summary>
        private void RemoveTimeLine(TimeLineViewModel timeLine)
        {
            _timeLines.Remove(timeLine);
        }

        /// <summary>
        /// Update the total height of all timelines.
        /// </summary>
        private void UpdateTimeLinesTotalHeight()
        {
            _timeLinesTotalHeight = _timeLines.Count * _settingsViewModel.TimeLineHeight;
        }

        /// <summary>
        /// Update the start time of the first timeline item.
        /// </summary>
        private void UpdateTotalStartTime()
        {
            TotalStartTime = GetStartTimeOfFirstTimeLineItem();
        }

        /// <summary>
        /// Update the end time of the last timeline item.
        /// </summary>
        private void UpdateTotalEndTime()
        {
            TotalEndTime = GetEndTimeOfLastTimeLineItem();
        }

        /// <summary>
        /// Is called when a timeline has been loaded.
        /// </summary>
        private void TimeLineViewModel_Loaded(object sender, EventArgs e)
        {
            ((TimeLineViewModel)sender).Loaded -= TimeLineViewModel_Loaded;

            _timeLinesToLoad--;

            if (_timeLinesToLoad == 0)
            {
                Loaded?.Invoke(this, null);
                IsLoading = false;
            }
        }

        /// <summary>
        /// Clean up timeline view model.
        /// </summary>
        private void CleanUp()
        {
            if (_timeLines.Count == 0)
            {
                return;
            }

            _timeLinesToLoad = 0;

            TimeLinesModel = null;

            _orderedTimeLineItems = null;

            _selectedTimeLineItem = null;

            HorizontalScrollOffset = 0;
            VerticalScrollOffset = 0;

            //IsModified = false;

            List<TimeLineViewModel> timeLinesToDispose = _timeLines;
            _timeLines = new List<TimeLineViewModel>();

            Task.Run(() =>
            {
                foreach (TimeLineViewModel timeLine in timeLinesToDispose)
                {
                    timeLine.Dispose();
                }
            });
        }
    }
}
