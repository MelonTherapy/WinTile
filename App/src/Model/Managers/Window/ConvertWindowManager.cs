﻿using System;
using System.Collections.Generic;
using static System.Windows.SystemParameters;
using Rect = App.Model.Rect;

namespace App
{
    public class ConvertWindowManager : IWindowManager
    {
        private readonly IWindowManager wrapped;

        public ConvertWindowManager(IWindowManager wrapped)
        {
            this.wrapped = wrapped;
        }

        public IntPtr FocusedWindow
        {
            get => wrapped.FocusedWindow;
            set => wrapped.FocusedWindow = value;
        }

        public IEnumerable<IntPtr> GetVisibleWindows() => wrapped.GetVisibleWindows();
        public Rect GetWindowRect(IntPtr handle) => PxtoPercent(wrapped.GetWindowRect(handle));
        public void PositionWindow(IntPtr handle, Rect rect) => wrapped.PositionWindow(handle, PercentToPx(rect));

        private Rect MonitorRect => new Rect(0, 0, (int)WorkArea.Width, (int)WorkArea.Height);

        private Rect PxtoPercent(Rect rect)
        {
            return (rect * 100).shrink(MonitorRect.Width, MonitorRect.Height);
        }

        private Rect PercentToPx(Rect rect)
        {
            return rect.extend(MonitorRect.Width, MonitorRect.Height) / 100;
        }
    }
}