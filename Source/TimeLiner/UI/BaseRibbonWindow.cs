// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2025 Christian Pistor

using Fluent;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace TimeLiner.UI
{
    /// <summary>
    /// Base class for all main windows using the Fluent Ribbon.
    ///
    /// This class derives from <see cref="RibbonWindow"/> in order to
    /// inherit the Fluent Ribbon visual style.
    ///
    /// In addition, it disables the system menu and the maximize action
    /// when interacting with the application icon in the title bar.
    ///
    /// The Fluent Ribbon opens the system menu very early during
    /// non-client hit testing, so the interception must happen at the
    /// WM_NCHITTEST stage.
    /// </summary>    
    public abstract class BaseRibbonWindow : RibbonWindow
    {
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Enforce left alignment of the title bar content
            TitleBar.HorizontalAlignment = HorizontalAlignment.Left;
            TitleBar.Margin = new Thickness(8, 0, 0, 0);

            // Attach a window message hook to the native window handle
            if (PresentationSource.FromVisual(this) is HwndSource source)
            {
                source.AddHook(WndProc);
            }
        }

        private IntPtr WndProc(
            IntPtr hwnd,
            int msg,
            IntPtr wParam,
            IntPtr lParam,
            ref bool handled)
        {
            // We only care about hit testing and non-client double clicks
            if (msg == Win32.WM_NCHITTEST || msg == Win32.WM_NCLBUTTONDBLCLK)
            {
                // Extract mouse position from lParam (screen coordinates)
                int x = unchecked((short)(lParam.ToInt32() & 0xFFFF));
                int y = unchecked((short)((lParam.ToInt32() >> 16) & 0xFFFF));

                Point screenPoint = new(x, y);
                Point windowPoint = PointFromScreen(screenPoint);

                int iconWidth = Win32.GetSystemMetrics(Win32.SM_CXSMICON);
                int iconHeight = Win32.GetSystemMetrics(Win32.SM_CYSMICON);

                // Check whether the mouse is within the application icon area
                // located in the top-left corner of the window
                if (windowPoint.X >= 0 &&
                    windowPoint.Y >= 0 &&
                    windowPoint.X < iconWidth &&
                    windowPoint.Y < iconHeight)
                {
                    // Mark the message as handled and return HTCAPTION
                    // to suppress the system menu and maximize behavior
                    handled = true;
                    return new IntPtr(Win32.HTCAPTION);
                }
            }

            // All other messages are processed normally
            return IntPtr.Zero;
        }

        private static class Win32
        {
            // Sent to determine which part of the window the mouse is over
            public const int WM_NCHITTEST = 0x0084;

            // Sent when the user double-clicks in the non-client area
            public const int WM_NCLBUTTONDBLCLK = 0x00A3;

            // Treat the area as a normal caption to avoid system commands
            public const int HTCAPTION = 2;

            // System metrics for the small window icon size
            public const int SM_CXSMICON = 49;
            public const int SM_CYSMICON = 50;

            [DllImport("user32.dll")]
            public static extern int GetSystemMetrics(int nIndex);
        }
    }
}
