// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2025 Christian Pistor

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TimeLiner.ViewModels;

namespace TimeLiner.Views
{
    /// <summary>
    /// The timeline list view.
    /// </summary>
    public partial class TimeLinesView : UserControl
    {
        /// <summary>
        /// The last mouse position before dragging the timelines.
        /// </summary>
        private Point _lastMousePosition;

        /// <summary>
        /// The last horizontal scroll offset before dragging the timelines.
        /// </summary>
        private double _lastHorizontalScrollOffset;

        /// <summary>
        /// The last vertical scroll offset before dragging the timelines.
        /// </summary>
        private double _lastVerticalScrollOffset;

        /// <summary>
        /// Constructor.
        /// </summary>
        public TimeLinesView()
        {
            InitializeComponent();
        }

        /// <see cref="GridWidth"/>
        private static readonly DependencyProperty _gridWidthProperty = DependencyProperty.RegisterAttached(
            nameof(GridWidth),
            typeof(double),
            typeof(TimeLinesView),
            new FrameworkPropertyMetadata(100d)
            );

        /// <see cref="GridHeight"/>
        private static readonly DependencyProperty _gridHeightProperty = DependencyProperty.RegisterAttached(
            nameof(GridHeight),
            typeof(double),
            typeof(TimeLinesView),
            new FrameworkPropertyMetadata(30d)
            );

        /// <summary>
        /// The data context as TimeLinesViewModel.
        /// </summary>
        private TimeLinesViewModel TimeLinesViewModel => (TimeLinesViewModel)DataContext;

        /// <summary>
        /// The width of a grid element [pixel].
        /// </summary>
        public double GridWidth
        {
            get => (double)GetValue(_gridWidthProperty);
            set => SetValue(_gridWidthProperty, value);
        }

        /// <summary>
        /// The height of a grid element [pixel].
        /// </summary>
        public double GridHeight
        {
            get => (double)GetValue(_gridHeightProperty);
            set => SetValue(_gridHeightProperty, value);
        }

        /// <summary>
        /// Is called when the size of the timelines view has changed.
        /// </summary>
        private void TimeLines_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TimeLinesViewModel.TimeLinesVisibleHeight = e.NewSize.Height;
            TimeLinesViewModel.TimeLinesVisibleWidth = e.NewSize.Width;
        }

        /// <summary>
        /// Is called when the data context of the timelines view has changed.
        /// </summary>
        private void TimeLines_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ItemsControl itemsControl = (ItemsControl)sender;

            TimeLinesViewModel.TimeLinesVisibleWidth = itemsControl.ActualWidth;
            TimeLinesViewModel.TimeLinesVisibleHeight = itemsControl.ActualHeight;
        }

        /// <summary>
        /// Is called when the timelines view is clicked with the left mouse button.
        /// </summary>
        private void TimeLines_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MoveSelectedTimeLocatorByMouse(e);

            _lastMousePosition = e.GetPosition(TimeLines);

            _lastHorizontalScrollOffset = TimeLinesViewModel.HorizontalScrollOffset;
            _lastVerticalScrollOffset = TimeLinesViewModel.VerticalScrollOffset;

            TimeLines.CaptureMouse();
        }

        /// <summary>
        /// Is called when the timelines view is clicked with the right mouse button.
        /// </summary>
        private void TimeLines_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            MoveSelectedTimeLocatorByMouse(e);

            TimeLines.CaptureMouse();
        }

        /// <summary>
        /// Is called when the mouse is moved over the timelines view.
        /// </summary>
        private void TimeLines_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!TimeLines.IsMouseCaptured)
            {
                return;
            }

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    // Left mouse button pressed + left CTRL key
                    MoveStartTimeLocatorByMouse(e);
                }
                else if (Keyboard.IsKeyDown(Key.LeftAlt))
                {
                    // Left mouse button pressed + left ALT key
                    ZoomTimeLinesByMouse(e);
                }
                else
                {
                    // Left mouse button pressed
                    ScrollTimeLinesByMouse(e);
                }
            }

            if (e.RightButton == MouseButtonState.Pressed)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    // Right mouse button pressed + left CTRL key
                    MoveEndTimeLocatorByMouse(e);
                }
            }
        }

        /// <summary>
        /// Scroll timelines horizontally and vertically by mouse.
        /// </summary>
        private void ScrollTimeLinesByMouse(MouseEventArgs e)
        {
            double horizontalMouseOffset = _lastMousePosition.X - e.GetPosition(TimeLines).X;
            double verticalMouseOffset = _lastMousePosition.Y - e.GetPosition(TimeLines).Y;

            double newHorizontalScrollOffset = _lastHorizontalScrollOffset + horizontalMouseOffset;
            double newVerticalScrollOffset = _lastVerticalScrollOffset + verticalMouseOffset;

            TimeLinesViewModel.HorizontalScrollOffset = newHorizontalScrollOffset;
            TimeLinesViewModel.VerticalScrollOffset = newVerticalScrollOffset;
        }

        /// <summary>
        /// Move the start-time locator by mouse.
        /// </summary>
        private void MoveStartTimeLocatorByMouse(MouseEventArgs e)
        {
            Cursor = Cursors.ScrollWE;
            TimeLinesViewModel.StartTimeLocatorViewModel.X = e.GetPosition(StartTimeLocator).X;
        }

        /// <summary>
        /// Move the end-time locator by mouse.
        /// </summary>
        private void MoveEndTimeLocatorByMouse(MouseEventArgs e)
        {
            Cursor = Cursors.ScrollWE;
            TimeLinesViewModel.EndTimeLocatorViewModel.X = e.GetPosition(EndTimeLocator).X;
        }

        /// <summary>
        /// Move the selected-time locator by mouse.
        /// </summary>
        private void MoveSelectedTimeLocatorByMouse(MouseEventArgs e)
        {
            TimeLinesViewModel.SelectedTimeLocatorViewModel.X = e.GetPosition(SelectedTimeLocator).X;
        }

        /// <summary>
        /// Zoom the time lines by mouse.
        /// </summary>
        private void ZoomTimeLinesByMouse(MouseEventArgs e)
        {
            TimeLinesViewModel.ZoomToolViewModel.Left = _lastMousePosition.X;
            TimeLinesViewModel.ZoomToolViewModel.Width = e.GetPosition(ZoomView).X - _lastMousePosition.X;
        }

        /// <summary>
        /// Is called when the left mouse button is released on the timelines view.
        /// </summary>
        private void TimeLines_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Cursor = Cursors.Arrow;
            TimeLines.ReleaseMouseCapture();
            TimeLinesViewModel.ZoomToolViewModel?.Release();
        }

        /// <summary>
        /// Is called when the right mouse button is released on the timelines view.
        /// </summary>
        private void TimeLines_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Cursor = Cursors.Arrow;
            TimeLines.ReleaseMouseCapture();
        }

        /// <summary>
        /// Is called when the timelines view is double-clicked outside.
        /// </summary>
        private void TimeLines_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Border)
            {
                TimeLinesViewModel.NewTimeLineItemCommand.Execute(null);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Is called when the mouse wheel is scrolled.
        /// </summary>
        private void UserControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (e.Delta > 0)
                    TimeLinesViewModel.ZoomInCommand.Execute(null);
                else
                    TimeLinesViewModel.ZoomOutCommand.Execute(null);
            }
            else if (Keyboard.IsKeyDown(Key.LeftAlt))
            {
                if (e.Delta > 0)
                    TimeLinesViewModel.ScrollMultipleTimeLinesDownCommand.Execute(null);
                else
                    TimeLinesViewModel.ScrollMultipleTimeLinesUpCommand.Execute(null);
            }
            else
            {
                if (e.Delta > 0)
                    TimeLinesViewModel.ScrollTimeLinesRightCommand.Execute(null);
                else
                    TimeLinesViewModel.ScrollTimeLinesLeftCommand.Execute(null);
            }
        }
    }
}
