// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2026 Christian Pistor

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TimeLiner.ViewModels;

// WPF handle drag and drop as well as left click
// https://stackoverflow.com/questions/12802122/wpf-handle-drag-and-drop-as-well-as-left-click

namespace TimeLiner.Views
{
    /// <summary>
    /// The timeline item view.
    /// </summary>
    public partial class TimeLineItemView : UserControl
    {
        /// <summary>
        /// Reference to the timeline scaling view model.
        /// </summary>
        private readonly TimeLineScalingViewModel _timeLineScalingViewModel;

        /// <summary>
        /// The last mouse position before dragging the timeline item.
        /// </summary>
        private Point _lastMousePosition;

        /// <summary>
        /// The last start time before dragging the timeline item.
        /// </summary>
        private DateTime _lastStartTime;

        /// <summary>
        /// Indicates that the shifting of the start time is still going on.
        /// </summary>
        private bool _isShiftingStartTime = false;

        /// <summary>
        /// True if an undo snapshot was taken for the current drag of the timeline item.
        /// </summary>
        private bool _undoCapturedForDrag;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TimeLineItemView()
        {
            _timeLineScalingViewModel = AppServices.TimeLineScaling;

            InitializeComponent();
        }

        /// <summary>
        /// The data context as TimeLineItemViewModel.
        /// </summary>
        internal TimeLineItemViewModel TimeLineItemViewModel => (TimeLineItemViewModel)DataContext;

        /// <summary>
        /// Is called when the timeline item is clicked with the left mouse button.
        /// </summary>
        private void UserControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                TimeLineItemViewModel.EditTimeLineItemCommand.Execute(null);
            }
            else
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                {
                    _lastMousePosition = e.GetPosition(this);
                    _lastStartTime = TimeLineItemViewModel.StartTime;

                    CaptureMouse();
                }
            }
        }

        /// <summary>
        /// Is called when the mouse is moved over the timeline item.
        /// </summary>
        private async void UserControl_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!IsMouseCaptured)
            {
                return;
            }

            // Drag timeline item with left-click + left Shift key.
            if (e.LeftButton == MouseButtonState.Pressed && Keyboard.IsKeyDown(Key.LeftShift))
            {
                Point position = e.GetPosition(this);
                double offsetY = position.Y - _lastMousePosition.Y;
                double offsetX = position.X - _lastMousePosition.X;

                if (Math.Abs(offsetX) > SystemParameters.MinimumHorizontalDragDistance)
                {
                    if (!_undoCapturedForDrag)
                    {
                        _undoCapturedForDrag = true;
                        await TimeLineItemViewModel.TimeLineViewModel.TimeLinesViewModel.CaptureUndoAsync();
                    }

                    Cursor = Cursors.ScrollWE;
                    double offsetSeconds = _timeLineScalingViewModel.CalculateSeconds(offsetX, TimeLineItemViewModel.TimeLineViewModel.TimeLinesViewModel.Scale);
                    DateTime newStartTime = _lastStartTime.AddSeconds(offsetSeconds);
                    TimeLineItemViewModel.ShiftStartTime(newStartTime);

                    _isShiftingStartTime = true;

                    return;
                }

                if (!_isShiftingStartTime && Math.Abs(offsetY) > SystemParameters.MinimumVerticalDragDistance)
                {
                    if (!_undoCapturedForDrag)
                    {
                        _undoCapturedForDrag = true;
                        await TimeLineItemViewModel.TimeLineViewModel.TimeLinesViewModel.CaptureUndoAsync();

                    }

                    DataObject data = new(TimeLineItemViewModel);
                    data.SetData(typeof(TimeLineItemView), this);
                    DragDrop.DoDragDrop(this, data, DragDropEffects.Move);
                }
            }
        }

        /// <summary>
        /// Is called when the left mouse button is released on the timeline item.
        /// </summary>
        private void UserControl_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _undoCapturedForDrag = false;

            Cursor = Cursors.Arrow;

            _isShiftingStartTime = false;

            ReleaseMouseCapture();
        }
    }
}
