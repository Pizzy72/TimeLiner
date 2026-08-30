// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2026 Christian Pistor

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TimeLiner.ViewModels;

namespace TimeLiner.Views
{
    /// <summary>
    /// The timeline view.
    /// </summary>
    public partial class TimeLineView : Canvas
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public TimeLineView()
        {
            Loaded += TimeLineView_Loaded;
            InitializeComponent();
        }

        /// <summary>
        /// The data context as TimeLineViewModel.
        /// </summary>
        internal TimeLineViewModel TimeLineViewModel => (TimeLineViewModel)DataContext;

        /// <summary>
        /// Is called when the left mouse button is pressed while the mouse pointer is over this element.
        /// </summary>
        private void Canvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is TimeLineView)
            {
                if (e.ClickCount == 2)
                {
                    // Edit timeline item with left mouse button double-click.
                    TimeLineViewModel.NewTimeLineItemCommand.Execute(null);
                }
                else
                {
                    if (Keyboard.IsKeyDown(Key.LeftShift))
                    {
                        // Initiated dragging of timeline with left-click + left Shift key.
                        CaptureMouse();
                    }
                }
            }
        }

        /// <summary>
        /// Is called when the mouse pointer moves while the mouse pointer is over this element.
        /// </summary>
        private void Canvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!IsMouseCaptured)
            {
                return;
            }

            if (e.LeftButton == MouseButtonState.Pressed && Keyboard.IsKeyDown(Key.LeftShift))
            {
                DataObject data = new(TimeLineViewModel);
                data.SetData(typeof(TimeLineView), this);

                // Drag timeline with left-click + left Shift key.
                DragDrop.DoDragDrop(this, data, DragDropEffects.Move);
            }
        }

        /// <summary>
        /// Is called when the input system reports an underlying drag event with this element as the potential drop target.
        /// </summary>
        private void Canvas_DragOver(object sender, DragEventArgs e)
        {
            if (e.Source is TimeLineView or TimeLineItemView)
            {
                // Allow dragging a timeline or timeline item to this timeline.
                e.Handled = true;
            }
        }

        /// <summary>
        /// Is called when the input system reports an underlying drag event with this element.
        /// </summary>
        private void Canvas_DragEnter(object sender, DragEventArgs e)
        {
            if (IsTimeLineItemDragged(e) || IsTimeLineDragged(e))
            {
                // Select the target timeline.
                TimeLineViewModel targetTimeLine = GetDragTargetTimeLine(e);
                SelectTimeLine(targetTimeLine);
            }
        }

        /// <summary>
        /// Is called continuously while a drag-and-drop operation is in progress, and enables the drag source to give feedback to the user.
        /// </summary>
        private void Canvas_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            if (e.Effects == DragDropEffects.Move)
            {
                // Show north-south cursor while dragging.
                e.UseDefaultCursors = false;
                Mouse.SetCursor(Cursors.ScrollNS);
            }
            else
            {
                // End dragging.
                DeselectTimeLine();
                e.UseDefaultCursors = true;
            }

            e.Handled = true;
        }

        /// <summary>
        /// Is called when the input system reports an underlying drop event with this element as the drop target.
        /// </summary>
        private async void Canvas_Drop(object sender, DragEventArgs e)
        {
            try
            {
                if (IsTimeLineItemDragged(e))
                {
                    TimeLineItemViewModel draggedItem = GetDraggedTimeLineItem(e);
                    TimeLineViewModel targetTimeLine = GetDragTargetTimeLine(e);

                    if (targetTimeLine != null && draggedItem.TimeLineViewModel != targetTimeLine)
                    {
                        // Move timeline item.
                        await TimeLineViewModel.TimeLinesViewModel.MoveTimeLineItem(draggedItem, targetTimeLine);
                    }

                    DeselectTimeLine();
                }
                else if (IsTimeLineDragged(e))
                {
                    TimeLineViewModel draggedTimeLine = GetDraggedTimeLine(e);
                    TimeLineViewModel targetTimeLine = GetDragTargetTimeLine(e);

                    if (targetTimeLine != null && draggedTimeLine != targetTimeLine)
                    {
                        // Move timeline.
                        await TimeLineViewModel.TimeLinesViewModel.MoveTimeLine(draggedTimeLine, targetTimeLine);
                    }

                    DeselectTimeLine();
                }
            }
            catch
            {
                // ignore
            }
        }

        /// <summary>
        /// Is called when the left mouse button is released while the mouse pointer is over this element.
        /// </summary>
        private void Canvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ReleaseMouseCapture();
        }

        /// <summary>
        /// Select the given timeline.
        /// </summary>
        private void SelectTimeLine(TimeLineViewModel timeLineViewModel)
        {
            if (timeLineViewModel != null)
            {
                TimeLineViewModel.TimeLinesViewModel.TimeLineSelectorViewModel.SelectedTimeLine = timeLineViewModel;
            }
        }

        /// <summary>
        /// Deselect the selected timeline.
        /// </summary>
        private void DeselectTimeLine()
        {
            TimeLineViewModel.TimeLinesViewModel.TimeLineSelectorViewModel.SelectedTimeLine = null;
        }

        /// <summary>
        /// Determine if a timeline is being dragged.
        /// </summary>
        private static bool IsTimeLineDragged(DragEventArgs e)
        {
            return e.Data.GetDataPresent(typeof(TimeLineViewModel));
        }

        /// <summary>
        /// Get the view model of the timeline which is the drag-target.
        /// </summary>
        private static TimeLineViewModel GetDragTargetTimeLine(DragEventArgs e)
        {
            if (e.Source is TimeLineView timeLineView)
            {
                return timeLineView.TimeLineViewModel;
            }
            else if (e.Source is TimeLineItemView timeLineItemView)
            {
                return timeLineItemView.TimeLineItemViewModel.TimeLineViewModel;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get the view model of the timeline which is being dragged.
        /// </summary>
        private static TimeLineViewModel GetDraggedTimeLine(DragEventArgs e)
        {
            return (TimeLineViewModel)e.Data.GetData(typeof(TimeLineViewModel));
        }

        /// <summary>
        /// Determine if a timeline item is being dragged.
        /// </summary>
        private static bool IsTimeLineItemDragged(DragEventArgs e)
        {
            return e.Data.GetDataPresent(typeof(TimeLineItemViewModel));
        }

        /// <summary>
        /// Get the view model of the timeline item which is being dragged.
        /// </summary>
        private static TimeLineItemViewModel GetDraggedTimeLineItem(DragEventArgs e)
        {
            return (TimeLineItemViewModel)e.Data.GetData(typeof(TimeLineItemViewModel));
        }

        /// <summary>
        /// Is called when the timeline view is loaded.
        /// </summary>
        private void TimeLineView_Loaded(object sender, RoutedEventArgs e)
        {
            TimeLineViewModel.IsLoaded = true;
            Loaded -= TimeLineView_Loaded;
        }
    }
}
