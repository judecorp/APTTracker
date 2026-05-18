using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using HDTPlugin.Logic;

namespace HDTPlugin
{
    public partial class DebugWindow : Window
    {
        private readonly GameCounters _gameCounters = new GameCounters();
        private InputMoveManager _inputMoveManager;

        public DebugWindow()
        {
            InitializeComponent();

            _gameCounters.InitializeCounters();
            CreateCounterControls();
            
            _inputMoveManager = new InputMoveManager(PluginContainer);
            _inputMoveManager.Enable();
            
            LoadBackgroundImage();
            SetPluginPosition();
            RefreshUI(1.0);
            UpdateDisplay();
        }

        private void SetPluginPosition()
        {
            var position = PluginPosition.GetDefaultPosition();
            Canvas.SetLeft(PluginContainer, position.X);
            Canvas.SetTop(PluginContainer, position.Y);
        }

        private void LoadBackgroundImage()
        {
            try
            {
                var appDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var hdtDir = Path.Combine(appDir, "HearthstoneDeckTracker");
                var imagePath = Path.Combine(hdtDir, "img", "hsplugin.png");
                if (File.Exists(imagePath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    BackgroundImage.Source = bitmap;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Image not found at: " + imagePath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Failed to load background image: " + ex.Message);
            }
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
            PluginContainer.RecalculateScaling();
            
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
            Canvas.SetLeft(PluginContainer, scaledLeft);
            Canvas.SetTop(PluginContainer, basePosition.Y);
        }

        private void CreateCounterControls()
        {
            foreach (var counter in _gameCounters.OrderedCounters)
            {
                var grid = new Grid { Margin = new Thickness(0, 5, 0, 0) };
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var label = new TextBlock { Text = counter.DisplayName + ":", VerticalAlignment = VerticalAlignment.Center, Foreground = System.Windows.Media.Brushes.White };
                Grid.SetColumn(label, 0);
                grid.Children.Add(label);

                var stackPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
                Grid.SetColumn(stackPanel, 1);

                var valueLabel = new TextBlock { Text = counter.Value.ToString(), Width = 30, TextAlignment = TextAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Foreground = System.Windows.Media.Brushes.White };
                counter.Label = valueLabel;

                stackPanel.Children.Add(CreateButton("-10", -10, counter.Key));
                stackPanel.Children.Add(CreateButton("-1", -1, counter.Key));
                stackPanel.Children.Add(valueLabel);
                stackPanel.Children.Add(CreateButton("+1", 1, counter.Key));
                stackPanel.Children.Add(CreateButton("+10", 10, counter.Key));

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
            _gameCounters.Counters[key].Value = Math.Max(0, _gameCounters.Counters[key].Value + delta);
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            foreach (var counter in _gameCounters.OrderedCounters)
            {
                if (counter.Label != null)
                    counter.Label.Text = counter.Value.ToString();
            }

            PluginContainer.DisplayControl.UpdateCounters(_gameCounters);
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            _gameCounters.Reset();
            UpdateDisplay();
        }
    }
}
