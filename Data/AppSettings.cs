using HDTPlugin.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace HDTPlugin.Settings
{
    public class AppSettings : INotifyPropertyChanged
    {
        private static readonly string SettingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "HDTPlugin", // Application-specific folder
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
                    // Map bool DefaultVisibility to int (1 for true, 0 for false)
                    CounterVisibility[kvp.Key] = kvp.Value.DefaultVisibility ? 1 : 0;
                }
            }
        }

        // --- Existing Settings Properties (mirroring Settings.Designer.cs) ---
        private double _scale = 100.0;
        public double Scale
        {
            get => _scale;
            set => SetProperty(ref _scale, value);
        }

        private double _opacity = 100.0;
        public double Opacity
        {
            get => _opacity;
            set => SetProperty(ref _opacity, value);
        }

        private double _top = 100.0;
        public double Top
        {
            get => _top;
            set => SetProperty(ref _top, value);
        }

        private double _left = 100.0;
        public double Left
        {
            get => _left;
            set => SetProperty(ref _left, value);
        }

        private double _positionLeftPercent = -1.0;
        public double PositionLeftPercent
        {
            get => _positionLeftPercent;
            set => SetProperty(ref _positionLeftPercent, value);
        }

        private double _positionTopPercent = -1.0;
        public double PositionTopPercent
        {
            get => _positionTopPercent;
            set => SetProperty(ref _positionTopPercent, value);
        }

        private bool _showCombatTime = true;
        public bool ShowCombatTime
        {
            get => _showCombatTime;
            set => SetProperty(ref _showCombatTime, value);
        }

        private bool _showShopTime = true;
        public bool ShowShopTime
        {
            get => _showShopTime;
            set => SetProperty(ref _showShopTime, value);
        }

        private bool _showAPM = true;
        public bool ShowAPM
        {
            get => _showAPM;
            set => SetProperty(ref _showAPM, value);
        }

        private bool _showRank = true;
        public bool ShowRank
        {
            get => _showRank;
            set => SetProperty(ref _showRank, value);
        }

        // --- Dynamic Counter Visibility Setting ---
        public Dictionary<CounterKey, int> CounterVisibility { get; set; }

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
