using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using APTTracker.Logic;
using APTTracker.Data;
using APTTracker.Controls;

namespace APTTracker
{
    public partial class DebugWindow : Window
    {
        private readonly CounterState _counterState = new CounterState();
        private readonly Dictionary<CounterKey, TextBlock> _counterLabels = new Dictionary<CounterKey, TextBlock>();
        private InputMoveManager _inputMoveManager;
        private PluginContainer _pluginContainer;

        public DebugWindow()
        {
            InitializeComponent();
            
            _pluginContainer = new PluginContainer(_counterState);
            PluginCanvas.Children.Add(_pluginContainer);

            // No need for GameCounters initialization – CounterState starts at zero for all counters
            CreateCounterControls();
            
            _inputMoveManager = new InputMoveManager(_pluginContainer);
            _inputMoveManager.Enable();
            
            SetPluginPosition();
            RefreshUI(1.0);
            UpdateDisplay();
        }

        private void SetPluginPosition()
        {
            var position = PluginPosition.GetDefaultPosition();
            Canvas.SetLeft(_pluginContainer, position.X);
            Canvas.SetTop(_pluginContainer, position.Y);
        }

        private void Btn1080p_Click(object sender, RoutedEventArgs e)
        {
            UIScaler.SetResolutionTier(Logic.ResolutionTier.HD1080p);
            TxtCurrentTier.Text = "Current: 1080p";
            RefreshUI(1.0);
        }

        private void Btn1440p_Click(object sender, RoutedEventArgs e)
        {
            UIScaler.SetResolutionTier(Logic.ResolutionTier.QHD1440p);
            TxtCurrentTier.Text = "Current: 1440p";
            RefreshUI(1.33);
        }

        private void Btn4K_Click(object sender, RoutedEventArgs e)
        {
            UIScaler.SetResolutionTier(Logic.ResolutionTier.UHD4K);
            TxtCurrentTier.Text = "Current: 4K";
            RefreshUI(2.0);
        }

        private void RefreshUI(double scaleFactor)
        {
            _pluginContainer.RecalculateScaling();
            
            // Scale background from top-right corner
            BackgroundScale.ScaleX = scaleFactor;
            BackgroundScale.ScaleY = scaleFactor;
            
            // In debug window, shift left by offscreen pixels, then scale
            // 4K: screenWidth=3840, visible=1920, offscreen=1920
            // 1440p: screenWidth=2560, visible=1920, offscreen=640
            // 1080p: screenWidth=1920, visible=1920, offscreen=0
            var basePosition = Logic.PluginPosition.GetDefaultPosition();
            double debugWindowWidth = 1920.0;
            double screenWidth;
            if (scaleFactor > 1.5)
                screenWidth = 3840;  // 4K
            else if (scaleFactor > 1.0)
                screenWidth = 2560;  // 1440p
            else
                screenWidth = 1920;  // 1080p
            double offscreen = screenWidth - debugWindowWidth;
            double scaledLeft = (basePosition.X - offscreen);
            Canvas.SetLeft(_pluginContainer, scaledLeft);
            Canvas.SetTop(_pluginContainer, basePosition.Y);
        }

        private void CreateCounterControls()
        {
            foreach (var def in CounterDataProvider.Counters)
            {
                var key = def.Key;
                var displayName = def.Value.DisplayName;
                var grid = new Grid { Margin = new Thickness(0, 5, 0, 0) };
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var label = new TextBlock { Text = displayName + ":", VerticalAlignment = VerticalAlignment.Center, Foreground = System.Windows.Media.Brushes.White };
                Grid.SetColumn(label, 0);
                grid.Children.Add(label);

                var stackPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
                Grid.SetColumn(stackPanel, 1);

                var valueLabel = new TextBlock { Text = _counterState[key].ToString(), Width = 30, TextAlignment = TextAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Foreground = System.Windows.Media.Brushes.White };
                _counterLabels[key] = valueLabel;

                stackPanel.Children.Add(CreateButton("-10", -10, key));
                stackPanel.Children.Add(CreateButton("-1", -1, key));
                stackPanel.Children.Add(valueLabel);
                stackPanel.Children.Add(CreateButton("+1", 1, key));
                stackPanel.Children.Add(CreateButton("+10", 10, key));

                grid.Children.Add(stackPanel);
                DynamicControlsContainer.Children.Add(grid);
            }
        }

        private Button CreateButton(string content, int delta, CounterKey key)
        {
            var btn = new Button { Content = content, Width = content.Contains("10") ? 35 : 30, Margin = new Thickness(content.StartsWith("-") ? 0 : 2, 0, 0, 0) };
            btn.Click += (s, e) => AdjustCounter(key, delta);
            return btn;
        }

        private void AdjustCounter(CounterKey key, int delta)
        {
            int newValue = Math.Max(0, _counterState[key] + delta);
            _counterState[key] = newValue;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            foreach (var kvp in _counterStateSnapshot())
            {
                if (_counterLabels.TryGetValue(kvp.Key, out var lbl))
                    lbl.Text = kvp.Value.ToString();
            }

            // Directly update the display control with the current CounterState
            _pluginContainer.DisplayControl.UpdateCounters(_counterState);
        }

        // Helper to get a snapshot of the current CounterState values
        private IReadOnlyDictionary<CounterKey, int> _counterStateSnapshot()
        {
            // CounterState already provides Snapshot method
            return _counterState.Snapshot();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            // Reset all counters to zero (default state)
            foreach (var key in Enum.GetValues(typeof(CounterKey)))
            {
                _counterState[(CounterKey)key] = 0;
            }
            UpdateDisplay();
        }
    }
}
