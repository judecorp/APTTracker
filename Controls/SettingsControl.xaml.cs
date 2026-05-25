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
        private ScrollViewer _scrollViewer;
        private PackIconMaterial _iconShowCombatTime;
        private PackIconMaterial _iconShowShopTime;
        private PackIconMaterial _iconShowAPM;
        private PackIconMaterial _iconShowRank;
        private bool _isLoading = true;
        private readonly AppSettings _settings = AppSettings.Default;

        // Map UI names to CounterKey
        private static readonly Dictionary<string, CounterKey> CounterMap = new Dictionary<string, CounterKey>
        {
            { "MinionsPlayed", CounterKey.UNITS_PLAYED },
            { "TavernSpells", CounterKey.TAVERN_SPELLS },
            { "NonTavernSpells", CounterKey.NON_TAVERN_SPELLS },
            { "CardsBought", CounterKey.BUY_CARD_COUNT },
            { "SellMinion", CounterKey.SELL_MINION_COUNT },
            { "Refresh", CounterKey.REFRESH_COUNT },
            { "Freeze", CounterKey.FREEZE_COUNT },
            { "LeveledUp", CounterKey.LEVELED_UP }
        };
        
        private readonly string[] _counterNames = CounterMap.Keys.ToArray();

        public SettingsControl()
        {
            InitializeComponent();
            _scrollViewer = SettingsScrollViewer;
            _iconShowCombatTime = IconShowCombatTime;
            _iconShowShopTime = IconShowShopTime;
            _iconShowAPM = IconShowAPM;
            _iconShowRank = IconShowRank;
            LoadSettings();
            _isLoading = false;
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
            foreach (var name in _counterNames)
            {
                int value = GetCounterValue(name);
                SetCounterButtonColors(name, value);
            }
        }

        private int GetCounterValue(string counterName)
        {
            if (CounterMap.TryGetValue(counterName, out CounterKey key))
            {
                return _settings.CounterVisibility[key];
            }
            return 2;
        }

        private void SetCounterValue(string counterName, int value)
        {
            if (CounterMap.TryGetValue(counterName, out CounterKey key))
            {
                _settings.CounterVisibility[key] = value;
            }
        }

        private void SetCounterButtonColors(string counterName, int value)
        {
            var alwaysBtn = FindName($"Btn{counterName}Always") as Button;
            var hoverBtn = FindName($"Btn{counterName}Hover") as Button;
            var neverBtn = FindName($"Btn{counterName}Never") as Button;

            if (alwaysBtn != null) alwaysBtn.Background = value == 2 ? new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50)) : new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33));
            if (hoverBtn != null) hoverBtn.Background = value == 1 ? new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00)) : new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33));
            if (neverBtn != null) neverBtn.Background = value == 0 ? new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36)) : new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33));
        }

        private void BtnCounterAlways_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string counterName)
            {
                SetCounterValue(counterName, 2);
                SetCounterButtonColors(counterName, 2);
                SaveSettings();
            }
        }

        private void BtnCounterHover_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string counterName)
            {
                SetCounterValue(counterName, 1);
                SetCounterButtonColors(counterName, 1);
                SaveSettings();
            }
        }

        private void BtnCounterNever_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string counterName)
            {
                SetCounterValue(counterName, 0);
                SetCounterButtonColors(counterName, 0);
                SaveSettings();
            }
        }

        private void BtnAllAlways_Click(object sender, RoutedEventArgs e)
        {
            foreach (var name in _counterNames)
            {
                SetCounterValue(name, 2);
                SetCounterButtonColors(name, 2);
            }
            SaveSettings();
        }

        private void BtnAllHover_Click(object sender, RoutedEventArgs e)
        {
            foreach (var name in _counterNames)
            {
                SetCounterValue(name, 1);
                SetCounterButtonColors(name, 1);
            }
            SaveSettings();
        }

        private void BtnAllNever_Click(object sender, RoutedEventArgs e)
        {
            foreach (var name in _counterNames)
            {
                SetCounterValue(name, 0);
                SetCounterButtonColors(name, 0);
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
                        textBlock.Foreground = isHover ? System.Windows.Media.Brushes.White : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x88, 0x88, 0x88));
                    else if (child is MahApps.Metro.IconPacks.PackIconMaterial icon)
                        icon.Foreground = isHover ? System.Windows.Media.Brushes.White : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x88, 0x88, 0x88));
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
                else if (child is Separator)
                {
                    // Separator doesn't need scaling
                }
                
                ScaleFontsRecursive(child, headerSize, bodySize, buttonSize);
            }
        }
    }
}