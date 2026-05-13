using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace HDTPlugin.Logic
{
    public static class PluginPosition
    {
        private static readonly string LogPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HDTPlugin", "position.log");

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
            var tier = UIScaler.CurrentTier;

            Point pos;
            if (tier == ResolutionTier.UHD4K)
                pos = new Point(2680, 0);
            else if (tier == ResolutionTier.QHD1440p)
                pos = new Point(1792, 0);
            else
                pos = new Point(1344, 0);

            return pos;
        }

        public static Point GetPosition(double leftPercent, double topPercent)
        {
            double width = UIScaler.GetWindowWidth();
            double height = UIScaler.GetWindowHeight();

            return new Point(width * leftPercent, height * topPercent);
        }

        public static void UpdatePosition(FrameworkElement element, double leftPercent, double topPercent)
        {
            var pos = GetPosition(leftPercent, topPercent);
            Log($"Setting Left={pos.X}, Top={pos.Y}");
            Canvas.SetTop(element, pos.Y);
            Canvas.SetLeft(element, pos.X);
        }

        public static bool IsOverlayAvailable => UIScaler.GetWindowWidth() > 0 && UIScaler.GetWindowHeight() > 0;

        public static Point GetSavedPosition()
        {
            var settings = Properties.Settings.Default;
            if (settings.PositionLeftPercent < 0 || settings.PositionTopPercent < 0)
                return new Point(double.NaN, double.NaN);

            return GetPosition(settings.PositionLeftPercent, settings.PositionTopPercent);
        }

        public static void SavePosition(FrameworkElement element)
        {
            double width = UIScaler.GetWindowWidth();
            double height = UIScaler.GetWindowHeight();

            if (width <= 0 || height <= 0)
                return;

            double leftPercent = Canvas.GetLeft(element) / width;
            double topPercent = Canvas.GetTop(element) / height;

            var settings = Properties.Settings.Default;
            settings.PositionLeftPercent = leftPercent;
            settings.PositionTopPercent = topPercent;
            settings.Save();
        }

        public static bool HasSavedPosition => Properties.Settings.Default.PositionLeftPercent >= 0 && Properties.Settings.Default.PositionTopPercent >= 0;
    }
}