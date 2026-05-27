using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using HDTPlugin.Logic;
using HDTPlugin.Data;
using HDTPlugin.Settings;
using System.Linq;

namespace HDTPlugin.Controls
{
    public partial class PlugInDisplayControl : StackPanel
    {
        private readonly TextBlock _summaryCounter;
        private readonly TextBlock _apmText;
        
        private readonly TextBlock _combatTimeText;
        private readonly TextBlock _shopTimeText;
        private readonly Image _rankImage;
        private readonly TextBlock _rankLabel;
        private readonly Border _summaryBorder;
        private readonly Border _countersBorder;
        private readonly Border _counterBorder;
        private readonly TextBlock _apmLabel;
        private readonly TextBlock _combatShopTimeLabel;
        private readonly TextBlock _combatShopSlash;
        private StackPanel _combatShopTimePanel;
        private StackPanel _apmPanel;
        private StackPanel _countersPanel;

        private readonly Dictionary<CounterKey, TextBlock> _textBlocks = new Dictionary<CounterKey, TextBlock>();

        private const double PulseIncrease = 6;
        private int _previousTotalActions;
        private int _previousRankIndex = -1;
        private CounterState _lastState;
        private bool _isHovering;

        public PlugInDisplayControl()
        {
            InitializeComponent();
            
            _summaryCounter = SummaryCounter;
            _apmText = ApmText;
            _combatTimeText = CombatTimeText;
            _shopTimeText = ShopTimeText;
            _rankImage = RankImage;
            _rankLabel = RankLabel;
            _summaryBorder = SummaryBorder;
            _countersBorder = CountersBorder;
            _counterBorder = CounterBorder;
            _apmLabel = ApmLabel;
            _combatShopTimeLabel = CombatShopTimeLabel;
            _combatShopSlash = CombatShopSlash;
            _combatShopTimePanel = CombatShopTimePanel;
            _apmPanel = ApmPanel;
            _countersPanel = CountersPanel;

            CreateCounterControls();
            
            ApplyScaling();
            ApplyVisibilitySettings();
        }

        private void CreateCounterControls()
        {
            foreach (var def in CounterDataProvider.Counters)
            {
                var key = def.Key;
                var data = def.Value;
                
                var textBlock = new TextBlock
                {
                    FontSize = UIScaler.GetBodyFontSize(),
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0),
                    Foreground = ToWpfBrush(data.Color)
                };
                
                _textBlocks[key] = textBlock;
                _countersPanel.Children.Add(textBlock);
            }
        }

        private static SolidColorBrush ToWpfBrush(System.Drawing.Color color)
        {
            return new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
        }

        public void ApplyVisibilitySettings()
        {
            var settings = AppSettings.Default;
            _combatShopTimePanel.Visibility = settings.ShowCombatTime || settings.ShowShopTime ? Visibility.Visible : Visibility.Collapsed;
            _combatTimeText.Visibility = settings.ShowCombatTime ? Visibility.Visible : Visibility.Collapsed;
            _combatShopSlash.Visibility = settings.ShowCombatTime && settings.ShowShopTime ? Visibility.Visible : Visibility.Collapsed;
            _shopTimeText.Visibility = settings.ShowShopTime ? Visibility.Visible : Visibility.Collapsed;
            _apmPanel.Visibility = settings.ShowAPM ? Visibility.Visible : Visibility.Collapsed;
            _rankImage.Visibility = settings.ShowRank ? Visibility.Visible : Visibility.Collapsed;
            _rankLabel.Visibility = settings.ShowRank ? Visibility.Visible : Visibility.Collapsed;
            ApplyCounterVisibility();
        }

        public void RecalculateScaling()
        {
            ApplyScaling();
        }

        public void ApplyScaling()
        {
            _summaryCounter.FontSize = UIScaler.GetSummaryFontSize();
            _rankLabel.FontSize = UIScaler.GetHeaderFontSize();
            _apmText.FontSize = UIScaler.GetBodyFontSize();
            _apmLabel.FontSize = UIScaler.GetBodyFontSize();
            _combatTimeText.FontSize = UIScaler.GetBodyFontSize();
            _shopTimeText.FontSize = UIScaler.GetBodyFontSize();
            _combatShopTimeLabel.FontSize = UIScaler.GetBodyFontSize();
            _combatShopSlash.FontSize = UIScaler.GetBodyFontSize();

            foreach(var textBlock in _textBlocks.Values)
            {
                textBlock.FontSize = UIScaler.GetBodyFontSize();
            }

            _rankImage.Width = UIScaler.GetRankBorderSize();
            _rankImage.Height = UIScaler.GetRankBorderSize();

            _counterBorder.Height = UIScaler.GetCounterHeight();

            MinWidth = UIScaler.GetMinWidth();
        }

        public void SetApm(double apm)
        {
            _apmText.Text = $"{apm:F2}";
        }

        public void SetCombatTime(int seconds)
        {
            _combatTimeText.Text = $"{seconds}s";
        }

        public void SetShopTime(int seconds)
        {
            _shopTimeText.Text = $"{seconds}s";
        }

        public void UpdateCounters(CounterState state)
        {
            _lastState = state;

            int totalActions = 0;

            foreach (CounterKey key in Enum.GetValues(typeof(CounterKey)))
            {
                if (CounterDataProvider.Counters[key].IncludeInSummary)
                {
                    totalActions += state[key];
                }
            }

            double baseFontSize = UIScaler.GetSummaryFontSize();
            double targetFontSize = baseFontSize + (totalActions > 0 ? PulseIncrease : 0);
            _summaryCounter.FontSize = targetFontSize;
            _summaryCounter.Text = $"{totalActions}";

            if (totalActions != _previousTotalActions && totalActions > 0)
            {
                var animation = new DoubleAnimation
                {
                    From = targetFontSize,
                    To = targetFontSize + PulseIncrease,
                    Duration = TimeSpan.FromMilliseconds(150),
                    AutoReverse = true,
                    FillBehavior = FillBehavior.Stop
                };
                _summaryCounter.BeginAnimation(TextBlock.FontSizeProperty, animation);
            }

            _previousTotalActions = totalActions;

            foreach (var kvp in _textBlocks)
            {
                CounterKey key = kvp.Key;
                TextBlock textBlock = kvp.Value;
                int value = state[key];
                string label = CounterDataProvider.Counters[key].DisplayName;
                UpdateCounterText(textBlock, label, value);
            }

            UpdateRank(totalActions);
            ApplyCounterVisibility();
        }

        private void UpdateCounterText(TextBlock textBlock, string label, int value)
        {
            textBlock.Text = $"{label}: {value}";
        }

        private void UpdateRank(int totalActions)
        {
            int rankIndex = RankSystem.GetRankIndex(totalActions);
            if (rankIndex != _previousRankIndex)
            {
                _previousRankIndex = rankIndex;
                var rankInfo = RankSystem.GetRank(totalActions);
                _rankLabel.Text = $"Rank: {rankInfo.Label}";

                var image = RankSystem.GetRankImage(totalActions);
                if (image != null)
                {
                    _rankImage.Source = image;
                }
            }
        }

        public void ExpandCounters()
        {
            _isHovering = true;
            ApplyCounterVisibility();
        }

        public void CollapseCounters()
        {
            _isHovering = false;
            ApplyCounterVisibility();
        }

        private void ApplyCounterVisibility()
        {
            if (_lastState == null) return;
            
            var settings = AppSettings.Default;

            bool atleastOneVisible = false;

            foreach (var kvp in CounterDataProvider.Counters)
            {
                CounterKey key = kvp.Key;
                if (_textBlocks.TryGetValue(key, out TextBlock textBlock))
                {
                    int value = _lastState[key];
                    Visibility visibility = GetCounterVisibility(settings.CounterVisibility[key], _isHovering, value);
                    textBlock.Visibility = visibility;

                    if(visibility == Visibility.Visible)
                    {
                        atleastOneVisible = true;
                    }
                }
            }

            _countersBorder.Visibility = atleastOneVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private Visibility GetCounterVisibility(int setting, bool isHover, int value)
        {
            if (value <= 0) return Visibility.Collapsed;
            if (setting == 2) return Visibility.Visible;
            if (setting == 1 && isHover) return Visibility.Visible;
            return Visibility.Collapsed;
        }
    }
}