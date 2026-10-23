using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using APTTracker.Settings;
using Core = Hearthstone_Deck_Tracker.API.Core;

namespace APTTracker.Logic
{
    public static class PluginPosition
    {
        private static readonly string LogPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "APTTracker", "position.log");

        private static void Log(string message)
        {
            try
            {
                string dir = Path.GetDirectoryName(LogPath);
                if (dir != null)
                {
                    Directory.CreateDirectory(dir);
                    File.AppendAllText(LogPath, $"[{DateTime.Now:HH:mm:ss}] {message}\n");
                }
            }
            catch { }
        }

        public static Point GetDefaultPosition()
        {
            return GetPosition(0.7, 0.0);
        }

        public static Point GetPosition(double leftPercent, double topPercent)
        {
            var size = GetViewportSize();
            return new Point(size.Width * leftPercent, size.Height * topPercent);
        }

        private static Size GetViewportSize()
        {
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
            var pos = GetPosition(leftPercent, topPercent);
            Log($"Setting Left={pos.X}, Top={pos.Y}");
            Canvas.SetTop(element, pos.Y);
            Canvas.SetLeft(element, pos.X);
        }

        public static bool IsOverlayAvailable => (Application.Current?.MainWindow?.ActualWidth ?? 0) > 0;

        public static Point GetSavedPosition()
        {
            var settings = AppSettings.Default;
            if (settings.PositionLeftPercent < 0 || settings.PositionTopPercent < 0)
                return new Point(double.NaN, double.NaN);

            return GetPosition(settings.PositionLeftPercent, settings.PositionTopPercent);
        }
         
        public static void SavePosition(FrameworkElement element)
        {
            var size = GetViewportSize();
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
