using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MahApps.Metro.IconPacks;
using HDTPlugin.Logic;
using HDTPlugin.Settings;
using System.Collections.Generic;
using HDTPlugin.Data;

namespace HDTPlugin.Controls
{
    public partial class SettingsControl : Border
    {
        private class CounterTag
        {
            public CounterKey Key { get; }
            public int Value { get; }
            public CounterTag(CounterKey key, int value) { Key = key; Value = value; }
        }

        private ScrollViewer _scrollViewer;
        private PackIconMaterial _iconShowCombatTime;
        private PackIconMaterial _iconShowShopTime;
        private PackIconMaterial _iconShowAPM;
        private PackIconMaterial _iconShowRank;
        private bool _isLoading = true;
        private readonly AppSettings _settings = AppSettings.Default;
        private StackPanel _counterControlsPanel;

        // Track created buttons for each counter
        private readonly Dictionary<CounterKey, Button> _alwaysButtons = new Dictionary<CounterKey, Button>();
        private readonly Dictionary<CounterKey, Button> _hoverButtons = new Dictionary<CounterKey, Button>();
        private readonly Dictionary<CounterKey, Button> _neverButtons = new Dictionary<CounterKey, Button>();

        public SettingsControl()
        {
            InitializeComponent();
            _scrollViewer = SettingsScrollViewer;
            _iconShowCombatTime = IconShowCombatTime;
            _iconShowShopTime = IconShowShopTime;
            _iconShowAPM = IconShowAPM;
            _iconShowRank = IconShowRank;
            _counterControlsPanel = CounterControlsPanel;
            
            CreateCounterControls();
            LoadSettings();
            _isLoading = false;
        }

        private void CreateCounterControls()
        {
            foreach (var kvp in CounterDataProvider.Counters)
            {
                CounterKey key = kvp.Key;
                string displayName = kvp.Value.DisplayName;

                var grid = new Grid { Margin = new Thickness(0, 2, 0, 0) };
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Auto) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Auto) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Auto) });

                var textBlock = new TextBlock
                {
                    Text = displayName,
                    Foreground = Brushes.White,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(textBlock, 0);
                grid.Children.Add(textBlock);

                var alwaysBtn = CreateCounterButton("Eye", key, 2, 0);
                var hoverBtn = CreateCounterButton("CursorDefault", key, 1, 1);
                var neverBtn = CreateCounterButton("Cancel", key, 0, 2);

                _alwaysButtons[key] = alwaysBtn;
                _hoverButtons[key] = hoverBtn;
                _neverButtons[key] = neverBtn;

                grid.Children.Add(alwaysBtn);
                grid.Children.Add(hoverBtn);
                grid.Children.Add(neverBtn);

                _counterControlsPanel.Children.Add(grid);
            }
        }

        private Button CreateCounterButton(string iconKind, CounterKey key, int value, int column)
        {
            var btn = new Button
            {
                Width = 24,
                Height = 24,
                Margin = new Thickness(2),
                Style = (Style)FindResource("IconButtonStyle"),
                Tag = new { Key = key, Value = value }
            };
            
            var icon = new PackIconMaterial
            {
                Kind = (PackIconMaterialKind)System.Enum.Parse(typeof(PackIconMaterialKind), iconKind),
                Width = 12,
                Height = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88))
            };
            btn.Content = icon;

            switch (value)
            {
                case 2:
                    btn.Click += BtnCounterAlways_Click;
                    break;
                case 1:
                    btn.Click += BtnCounterHover_Click;
                    break;
                case 0:
                    btn.Click += BtnCounterNever_Click;
                    break;
            }

            Grid.SetColumn(btn, column + 1);
            return btn;
        }

        private void LoadSettings()
        {
            UpdateShowToggle(_iconShowCombatTime, _settings.ShowCombatTime);
            UpdateShowToggle(_iconShowShopTime, _settings.ShowShopTime);
            UpdateShowToggle(_iconShowAPM, _settings.ShowAPM);
            UpdateShowToggle(_iconShowRank, _settings.ShowRank);
            UpdateCounterButtons();
        }

        private void UpdateShowToggle(PackIconMaterial icon, bool isChecked)
        {
            if (isChecked)
            {
                icon.Visibility = Visibility.Visible;
                icon.Kind = PackIconMaterialKind.Check;
            }
            else
            {
                icon.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowToggle_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_isLoading) return;

            if (sender is StackPanel panel && panel.Tag is string tag)
            {
                bool newValue = true;
                PackIconMaterial icon = null;

                switch (tag)
                {
                    case "ShowCombatTime":
                        _settings.ShowCombatTime = !_settings.ShowCombatTime;
                        newValue = _settings.ShowCombatTime;
                        icon = _iconShowCombatTime;
                        break;
                    case "ShowShopTime":
                        _settings.ShowShopTime = !_settings.ShowShopTime;
                        newValue = _settings.ShowShopTime;
                        icon = _iconShowShopTime;
                        break;
                    case "ShowAPM":
                        _settings.ShowAPM = !_settings.ShowAPM;
                        newValue = _settings.ShowAPM;
                        icon = _iconShowAPM;
                        break;
                    case "ShowRank":
                        _settings.ShowRank = !_settings.ShowRank;
                        newValue = _settings.ShowRank;
                        icon = _iconShowRank;
                        break;
                }

                UpdateShowToggle(icon, newValue);
                _settings.Save();
            }
        }

        private void ShowToggle_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is StackPanel panel && panel.Tag is string tag)
            {
                bool isChecked = false;
                PackIconMaterial icon = null;

                switch (tag)
                {
                    case "ShowCombatTime":
                        isChecked = _settings.ShowCombatTime;
                        icon = _iconShowCombatTime;
                        break;
                    case "ShowShopTime":
                        isChecked = _settings.ShowShopTime;
                        icon = _iconShowShopTime;
                        break;
                    case "ShowAPM":
                        isChecked = _settings.ShowAPM;
                        icon = _iconShowAPM;
                        break;
                    case "ShowRank":
                        isChecked = _settings.ShowRank;
                        icon = _iconShowRank;
                        break;
                }

                UpdateShowToggleHover(icon, isChecked, true);
            }
        }

        private void ShowToggle_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is StackPanel panel && panel.Tag is string tag)
            {
                bool isChecked = false;
                PackIconMaterial icon = null;

                switch (tag)
                {
                    case "ShowCombatTime":
                        isChecked = _settings.ShowCombatTime;
                        icon = _iconShowCombatTime;
                        break;
                    case "ShowShopTime":
                        isChecked = _settings.ShowShopTime;
                        icon = _iconShowShopTime;
                        break;
                    case "ShowAPM":
                        isChecked = _settings.ShowAPM;
                        icon = _iconShowAPM;
                        break;
                    case "ShowRank":
                        isChecked = _settings.ShowRank;
                        icon = _iconShowRank;
                        break;
                }

                UpdateShowToggleHover(icon, isChecked, false);
            }
        }

        private void UpdateShowToggleHover(PackIconMaterial icon, bool isChecked, bool isHover)
        {
            if (isChecked)
            {
                icon.Visibility = Visibility.Visible;
                icon.Kind = PackIconMaterialKind.Check;
            }
            else
            {
                if (isHover)
                {
                    icon.Visibility = Visibility.Visible;
                    icon.Kind = PackIconMaterialKind.CheckOutline;
                }
                else
                {
                    icon.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void UpdateCounterButtons()
        {
            foreach (var kvp in CounterDataProvider.Counters)
            {
                int value = _settings.CounterVisibility[kvp.Key];
                SetCounterButtonColors(kvp.Key, value);
            }
        }

        private void SetCounterValue(CounterKey key, int value)
        {
            _settings.CounterVisibility[key] = value;
        }

        private void SetCounterButtonColors(CounterKey key, int value)
        {
            if (_alwaysButtons.TryGetValue(key, out Button alwaysBtn))
                alwaysBtn.Background = value == 2 ? new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50)) : new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33));
            if (_hoverButtons.TryGetValue(key, out Button hoverBtn))
                hoverBtn.Background = value == 1 ? new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00)) : new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33));
            if (_neverButtons.TryGetValue(key, out Button neverBtn))
                neverBtn.Background = value == 0 ? new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36)) : new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33));
        }

        private void BtnCounterAlways_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is CounterTag tag)
            {
                SetCounterValue(tag.Key, 2);
                SetCounterButtonColors(tag.Key, 2);
                SaveSettings();
            }
        }

        private void BtnCounterHover_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is CounterTag tag)
            {
                SetCounterValue(tag.Key, 1);
                SetCounterButtonColors(tag.Key, 1);
                SaveSettings();
            }
        }

        private void BtnCounterNever_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is CounterTag tag)
            {
                SetCounterValue(tag.Key, 0);
                SetCounterButtonColors(tag.Key, 0);
                SaveSettings();
            }
        }

        private void BtnAllAlways_Click(object sender, RoutedEventArgs e)
        {
            foreach (var key in CounterDataProvider.Counters.Keys)
            {
                SetCounterValue(key, 2);
                SetCounterButtonColors(key, 2);
            }
            SaveSettings();
        }

        private void BtnAllHover_Click(object sender, RoutedEventArgs e)
        {
            foreach (var key in CounterDataProvider.Counters.Keys)
            {
                SetCounterValue(key, 1);
                SetCounterButtonColors(key, 1);
            }
            SaveSettings();
        }

        private void BtnAllNever_Click(object sender, RoutedEventArgs e)
        {
            foreach (var key in CounterDataProvider.Counters.Keys)
            {
                SetCounterValue(key, 0);
                SetCounterButtonColors(key, 0);
            }
            SaveSettings();
        }

        private void SaveSettings()
        {
            _settings.Save();
        }

        public void RecalculateScaling()
        {
            Width = UIScaler.GetMinWidth();
            MaxHeight = UIScaler.GetSettingsMaxHeight();
            if (_scrollViewer != null)
            {
                _scrollViewer.MaxHeight = UIScaler.GetSettingsMaxHeight();
            }
            ScaleFonts();
        }

        private void SetAllButtonHover(Button button, bool isHover)
        {
            if (button?.Content is StackPanel panel)
            {
                foreach (var child in panel.Children)
                {
                    if (child is TextBlock textBlock)
                        textBlock.Foreground = isHover ? Brushes.White : new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));
                    else if (child is PackIconMaterial icon)
                        icon.Foreground = isHover ? Brushes.White : new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x88));
                }
            }
        }

        private void BtnAllAlways_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is Button btn) SetAllButtonHover(btn, true);
        }

        private void BtnAllAlways_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is Button btn) SetAllButtonHover(btn, false);
        }

        private void BtnAllHover_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is Button btn) SetAllButtonHover(btn, true);
        }

        private void BtnAllHover_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is Button btn) SetAllButtonHover(btn, false);
        }

        private void BtnAllNever_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is Button btn) SetAllButtonHover(btn, true);
        }

        private void BtnAllNever_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is Button btn) SetAllButtonHover(btn, false);
        }

        private void ScaleFonts()
        {
            double headerFontSize = UIScaler.GetSettingsFontSize();
            double bodyFontSize = headerFontSize * 0.9;
            double buttonSize = UIScaler.GetSettingsButtonSize();

            if (_scrollViewer?.Content is StackPanel stackPanel)
            {
                ScaleFontsRecursive(stackPanel, headerFontSize, bodyFontSize, buttonSize);
            }
        }

        private void ScaleFontsRecursive(DependencyObject parent, double headerSize, double bodySize, double buttonSize)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is TextBlock textBlock)
                {
                    if (textBlock.FontWeight == FontWeights.Bold)
                        textBlock.FontSize = headerSize;
                    else
                        textBlock.FontSize = bodySize;
                }
                else if (child is CheckBox checkBox)
                {
                    checkBox.FontSize = bodySize * 0.9;
                }
                else if (child is Button button)
                {
                    button.FontSize = bodySize * 0.8;
                    button.Width = buttonSize;
                    button.Height = buttonSize;
                }

                ScaleFontsRecursive(child, headerSize, bodySize, buttonSize);
            }
        }
    }
}