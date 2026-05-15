using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Core = Hearthstone_Deck_Tracker.API.Core;

namespace HDTPlugin.Logic
{
    public enum ResolutionTier
    {
        HD1080p,
        QHD1440p,
        UHD4K
    }

    public static class UIScaler
    {
        private static readonly string LogPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HDTPlugin", "uiscaler.log");

        public static ResolutionTier CurrentTier = ResolutionTier.HD1080p;

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

        public static void SetResolutionTier(ResolutionTier tier)
        {
            CurrentTier = tier;
            Log($"Resolution tier set to: {tier}");
        }

        private static Window GetParentWindow()
        {
            if (Core.OverlayCanvas != null)
            {
                var window = Window.GetWindow(Core.OverlayCanvas);
                if (window != null)
                    return window;
            }

            var appWindow = Application.Current?.MainWindow;
            if (appWindow != null && appWindow.ActualWidth > 0)
                return appWindow;

            return null;
        }

        public static void DetectResolutionTier()
        {
            var window = GetParentWindow();
            double width = window?.ActualWidth ?? 1920;

            ResolutionTier newTier;
            if (width >= 3840)
                newTier = ResolutionTier.UHD4K;
            else if (width >= 2560)
                newTier = ResolutionTier.QHD1440p;
            else
                newTier = ResolutionTier.HD1080p;

            if (newTier != CurrentTier)
            {
                CurrentTier = newTier;
                Log($"Resolution tier detected: {newTier} (width: {width})");
            }
        }

        public static double GetWindowWidth()
        {
            var window = GetParentWindow();
            return window?.ActualWidth ?? 1920;
        }

        public static double GetWindowHeight()
        {
            var window = GetParentWindow();
            return window?.ActualHeight ?? 1080;
        }

        public static double GetSummaryFontSize()
        {
            if (CurrentTier == ResolutionTier.UHD4K) return 84;
            if (CurrentTier == ResolutionTier.QHD1440p) return 63;
            return 42;
        }

        public static double GetHeaderFontSize()
        {
            if (CurrentTier == ResolutionTier.UHD4K) return 28;
            if (CurrentTier == ResolutionTier.QHD1440p) return 21;
            return 14;
        }

        public static double GetBodyFontSize()
        {
            if (CurrentTier == ResolutionTier.UHD4K) return 28;
            if (CurrentTier == ResolutionTier.QHD1440p) return 21;
            return 14;
        }

        public static double GetRankBorderSize()
        {
            if (CurrentTier == ResolutionTier.UHD4K) return 160;
            if (CurrentTier == ResolutionTier.QHD1440p) return 120;
            return 80;
        }

        public static double GetMinWidth()
        {
            if (CurrentTier == ResolutionTier.UHD4K) return 450;
            if (CurrentTier == ResolutionTier.QHD1440p) return 338;
            return 225;
        }

        public static double GetCounterHeight()
        {
            if (CurrentTier == ResolutionTier.UHD4K) return 50;
            if (CurrentTier == ResolutionTier.QHD1440p) return 45;
            return 40;
        }

        public static double GetSettingsFontSize()
        {
            if (CurrentTier == ResolutionTier.UHD4K) return 28;
            if (CurrentTier == ResolutionTier.QHD1440p) return 21;
            return 14;
        }

        public static double GetSettingsButtonSize()
        {
            if (CurrentTier == ResolutionTier.UHD4K) return 48;
            if (CurrentTier == ResolutionTier.QHD1440p) return 36;
            return 24;
        }

        public static double GetSettingsMaxHeight()
        {
            if (CurrentTier == ResolutionTier.UHD4K) return 600;
            if (CurrentTier == ResolutionTier.QHD1440p) return 450;
            return 300;
        }
    }
}