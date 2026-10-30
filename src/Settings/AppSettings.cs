using APTTracker.Counters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace APTTracker.Settings
{
    public class AppSettings : INotifyPropertyChanged
    {
        private static readonly string SettingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "APTTracker", // Application-specific folder
            "settings.json"
        );

        // Eager initialization: Load settings as soon as possible
        public static AppSettings Default { get; } = Load();

        // Constructor for AppSettings
        private AppSettings()
        {
            // Initialize CounterVisibility with defaults from CounterDataProvider if empty
            if (CounterVisibility == null || CounterVisibility.Count == 0)
            {
                CounterVisibility = new Dictionary<CounterKey, int>();
                foreach (var kvp in CounterDataProvider.Counters)
                {
                    // Copy the counter's three-state default visibility.
                    CounterVisibility[kvp.Key] = kvp.Value.DefaultVisibility;
                }
            }
        }

        public const double MinimumScale = 0.5;
        public const double MaximumScale = 2.0;
        public const double DefaultPositionLeftPercent = 0.75351745700885875;
        public const double DefaultPositionTopPercent = 0.0;

        public static double ClampScale(double value)
        {
            return double.IsNaN(value) ? 1.0 : Math.Max(MinimumScale, Math.Min(MaximumScale, value));
        }

        private double _scale = 1.0;
        public double Scale
        {
            get => _scale;
            set => SetProperty(ref _scale, ClampScale(value));
        }

        private double _positionLeftPercent = DefaultPositionLeftPercent;
        public double PositionLeftPercent
        {
            get => _positionLeftPercent;
            set => SetProperty(ref _positionLeftPercent, value);
        }

        private double _positionTopPercent = DefaultPositionTopPercent;
        public double PositionTopPercent
        {
            get => _positionTopPercent;
            set => SetProperty(ref _positionTopPercent, value);
        }

        // Three-state visibility settings (0 = Never, 1 = Hover, 2 = Always).
        private int _showCombatTimeVisibility = 2;
        public int ShowCombatTimeVisibility
        {
            get => _showCombatTimeVisibility;
            set => SetProperty(ref _showCombatTimeVisibility, value);
        }
        private int _showShopTimeVisibility = 2;
        public int ShowShopTimeVisibility
        {
            get => _showShopTimeVisibility;
            set => SetProperty(ref _showShopTimeVisibility, value);
        }
        private int _showAPMVisibility = 2;
        public int ShowAPMVisibility
        {
            get => _showAPMVisibility;
            set => SetProperty(ref _showAPMVisibility, value);
        }
        private int _showRankVisibility = 2;
        public int ShowRankVisibility
        {
            get => _showRankVisibility;
            set => SetProperty(ref _showRankVisibility, value);
        }

        // --- Dynamic Counter Visibility Setting ---
        public Dictionary<CounterKey, int> CounterVisibility { get; set; }

        // Highest Actions‑Per‑Turn ever achieved (persisted)
        public int HighestApt { get; set; } = 0;

        // --- Serialization/Deserialization ---
        public void Save()
        {
            string jsonString = JsonConvert.SerializeObject(this, Formatting.Indented);
            string directory = Path.GetDirectoryName(SettingsFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllText(SettingsFilePath, jsonString);
        }

        private static AppSettings Load()
        {
            if (File.Exists(SettingsFilePath))
            {
                try
                {
                    string jsonString = File.ReadAllText(SettingsFilePath);
                    var loadedSettings = JsonConvert.DeserializeObject<AppSettings>(jsonString);

                    
                    // Merge loaded settings with current defaults to handle new counters
                    var defaultSettings = new AppSettings(); // This will initialize with defaults
                    foreach (var kvp in defaultSettings.CounterVisibility)
                    {
                        if (!loadedSettings.CounterVisibility.ContainsKey(kvp.Key))
                        {
                            loadedSettings.CounterVisibility[kvp.Key] = kvp.Value;
                        }
                    }
                    return loadedSettings;
                }
                catch (Exception ex)
                {
                    // Log the exception (e.g., HearthstoneDeckTracker.Log.Error(ex, "Failed to load settings"))
                    // For now, just print to console for debugging
                    Console.WriteLine($"Error loading settings: {ex.Message}");
                    return new AppSettings(); // Return new settings on error
                }
            }
            return new AppSettings(); // Return new settings if file doesn't exist
        }

        // --- INotifyPropertyChanged Implementation ---
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
