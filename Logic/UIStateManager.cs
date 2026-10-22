using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using APTTracker.Settings;
using Core = Hearthstone_Deck_Tracker.API.Core;

namespace APTTracker.Logic
{
    public class UIStateManager : INotifyPropertyChanged
    {
        private static UIStateManager _instance;
        public static UIStateManager Instance => _instance ?? (_instance = new UIStateManager());

        private double _manualScale = 1.0;
        private double _autoScale = 1.0;
        private double _totalScale = 1.0;

        public event PropertyChangedEventHandler PropertyChanged;

        public double ManualScale
        {
            get => _manualScale;
            set { 
                if (_manualScale != value) {
                    _manualScale = value;
                    UpdateTotalScale();
                    AppSettings.Default.Scale = value;
                    AppSettings.Default.Save();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ManualScale)));
                }
            }
        }

        public double AutoScale
        {
            get => _autoScale;
            set { 
                if (_autoScale != value) {
                    _autoScale = value;
                    UpdateTotalScale();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AutoScale)));
                }
            }
        }

        public double TotalScale
        {
            get => _totalScale;
            private set {
                if (_totalScale != value) {
                    _totalScale = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TotalScale)));
                }
            }
        }

        private UIStateManager()
        {
            _manualScale = AppSettings.Default.Scale;
            if (_manualScale <= 0) _manualScale = 1.0;
            
            UpdateAutoScale();
            
            var window = Application.Current?.MainWindow;
            if (window != null)
                window.SizeChanged += (s, e) => UpdateAutoScale();
            
            if (Core.OverlayCanvas != null)
                Core.OverlayCanvas.SizeChanged += (s, e) => UpdateAutoScale();
        }

        private void UpdateAutoScale()
        {
            var window = Application.Current?.MainWindow;
            double width = window?.ActualWidth ?? Core.OverlayCanvas?.ActualWidth ?? 1920;
            if (width <= 0) width = 1920;
            
            // Clamped 0.5 to 4.0
            AutoScale = Math.Max(0.5, Math.Min(4.0, width / 1920.0));
        }

        private void UpdateTotalScale()
        {
            TotalScale = ManualScale * AutoScale;
        }
    }
}
