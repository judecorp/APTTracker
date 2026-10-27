using System;
using System.Windows;
using System.Windows.Controls;
using APTTracker.Settings;
using Core = Hearthstone_Deck_Tracker.API.Core;

namespace APTTracker.Logic
{
    public static class PluginPosition
    {
        public static void RestorePosition(FrameworkElement element)
        {
            // Clamping must not overwrite the preferred position: resizing back restores it.
            var settings = AppSettings.Default;
            UpdatePosition(element,
                HasSavedPosition ? settings.PositionLeftPercent : 0.7,
                HasSavedPosition ? settings.PositionTopPercent : 0.0);
        }

        private static Size GetViewportSize(FrameworkElement element = null)
        {
            if (element?.Parent is Canvas canvas)
                return new Size(canvas.ActualWidth, canvas.ActualHeight);

            var window = Application.Current?.MainWindow;
            double width = Core.OverlayCanvas?.ActualWidth ?? 0;
            double height = Core.OverlayCanvas?.ActualHeight ?? 0;
            if (width <= 0) width = window?.ActualWidth ?? 0;
            if (height <= 0) height = window?.ActualHeight ?? 0;
            if (width <= 0) width = SystemParameters.PrimaryScreenWidth;
            if (height <= 0) height = SystemParameters.PrimaryScreenHeight;

            return new Size(width, height);
        }

        public static void UpdatePosition(FrameworkElement element, double leftPercent, double topPercent)
        {
            var size = GetViewportSize(element);
            var pos = new Point(size.Width * leftPercent, size.Height * topPercent);
            SetPosition(element, pos.X, pos.Y);
        }

        public static void SetPosition(FrameworkElement element, double left, double top)
        {
            var size = GetViewportSize(element);
            var bounds = element.RenderTransform.TransformBounds(new Rect(element.RenderSize));
            var pos = new Point(
                Math.Max(0, Math.Min(left, Math.Max(0, size.Width - bounds.Right))),
                Math.Max(0, Math.Min(top, Math.Max(0, size.Height - bounds.Bottom))));
            Canvas.SetTop(element, pos.Y);
            Canvas.SetLeft(element, pos.X);
        }

        public static void SavePosition(FrameworkElement element)
        {
            var size = GetViewportSize(element);
            double width = size.Width;
            double height = size.Height;

            if (width <= 0 || height <= 0)
                return;

            double leftPercent = Canvas.GetLeft(element) / width;
            double topPercent = Canvas.GetTop(element) / height;

            var settings = AppSettings.Default;
            settings.PositionLeftPercent = leftPercent;
            settings.PositionTopPercent = topPercent;
            settings.Save();
        }

        public static bool HasSavedPosition => AppSettings.Default.PositionLeftPercent >= 0 && AppSettings.Default.PositionTopPercent >= 0;
    }
}
