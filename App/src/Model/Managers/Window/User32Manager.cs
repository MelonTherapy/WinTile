﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using App.Model;

namespace App.Utils
{
    public class User32Manager : IWindowManager
    {
        private const short SWP_NOZORDER = 0X4;
        private const int SWP_SHOWWINDOW = 0x0040;

        public void PositionWindow(IntPtr handle, Rect rect)
        {
            var nativeWinRect = new NativeRect();
            GetWindowRect(handle, ref nativeWinRect);
            var nativeClientRect = new NativeRect();
            GetClientRect(handle, ref nativeClientRect);

            var winRect = rerct(nativeWinRect);
            var clientRect = rerct(nativeClientRect);

            var dx = Math.Min(winRect.Width - clientRect.Right, 16);
            var dy = Math.Min(winRect.Height - clientRect.Bottom, 8);

            SetWindowPos(handle, 0, (int) (rect.Left - dx / 2), (int) rect.Top, (int) (rect.Width + dx), (int) (rect.Height + dy),
                SWP_NOZORDER | SWP_SHOWWINDOW);
        }

        public Rect GetWindowRect(IntPtr handle)
        {
            var nativeWinRect = new NativeRect();
            GetWindowRect(handle, ref nativeWinRect);
            var nativeClientRect = new NativeRect();
            GetClientRect(handle, ref nativeClientRect);

            var winRect = rerct(nativeWinRect);
            var clientRect = rerct(nativeClientRect);

            var dx = Math.Min(winRect.Width - clientRect.Right, 16);
            var dy = Math.Min(winRect.Height - clientRect.Bottom, 8);

            winRect.Left += dx / 2;
            winRect.Right -= dx / 2;
            winRect.Top += 0;
            winRect.Bottom -= dy;

            return winRect;
        }

        public IntPtr FocusedWindow
        {
            get => GetForegroundWindow();
            set => SetForegroundWindow(value);
        }

        public IEnumerable<IntPtr> GetVisibleWindows()
        {
            var windows = new List<IntPtr>();

            EnumWindows(delegate(IntPtr wnd, IntPtr param)
            {
                if (IsWindowVisible(wnd)) windows.Add(wnd);
                return true;
            }, IntPtr.Zero);

            return windows;
        }

        private Rect rerct(NativeRect rect)
        {
            return new Rect(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }

        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hwnd, ref NativeRect rectangle);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hwnd, ref NativeRect rectangle);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        private static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy,
            int wFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        private struct NativeRect
        {
            public int Left { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
        }

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    }
}