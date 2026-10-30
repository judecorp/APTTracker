using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using APTTracker.Counters;
using APTTracker.Overlay;
using APTTracker.Settings;
using System.Linq;

namespace APTTracker.Controls
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
        private readonly StackPanel _counterBorder;
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
                    FontSize = 14,
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

            var combatTimeVis = GetShowVisibility(settings.ShowCombatTimeVisibility, _isHovering);
            var shopTimeVis   = GetShowVisibility(settings.ShowShopTimeVisibility,   _isHovering);
            var apmVis        = GetShowVisibility(settings.ShowAPMVisibility,        _isHovering);
            var rankVis       = GetShowVisibility(settings.ShowRankVisibility,       _isHovering);

            _combatTimeText.Visibility  = combatTimeVis;
            _shopTimeText.Visibility    = shopTimeVis;
            // The slash is only visible when both times are visible
            _combatShopSlash.Visibility = combatTimeVis == Visibility.Visible && shopTimeVis == Visibility.Visible
                ? Visibility.Visible : Visibility.Collapsed;
            // The container panel is visible when at least one time element is visible
            _combatShopTimePanel.Visibility = combatTimeVis == Visibility.Visible || shopTimeVis == Visibility.Visible
                ? Visibility.Visible : Visibility.Collapsed;

            _apmPanel.Visibility  = apmVis;
            _rankImage.Visibility = rankVis;
            _rankLabel.Visibility = rankVis;

            ApplyCounterVisibility();
        }

        // Mirrors GetCounterVisibility but for the Show* settings (no counter value threshold).
        private static Visibility GetShowVisibility(int setting, bool isHover)
        {
            if (setting == 2) return Visibility.Visible;               // Always
            if (setting == 1 && isHover) return Visibility.Visible;   // Hover
            return Visibility.Collapsed;                               // Never
        }

        public void SetApm(double apm)
        {
            string text = $"{apm:F2}";
            if (_apmText.Text != text)
                _apmText.Text = text;
        }

        public void SetCombatTime(int seconds)
        {
            string text = $"{seconds}s";
            if (_combatTimeText.Text != text)
                _combatTimeText.Text = text;
        }

        public void SetShopTime(int seconds)
        {
            string text = $"{seconds}s";
            if (_shopTimeText.Text != text)
                _shopTimeText.Text = text;
        }

        public void UpdateCounters(CounterState state, bool hasNewAptRecord = false)
        {
            _lastState = state;

            int totalActions = state.TotalActions;

            double targetFontSize = 42 + (totalActions > 0 ? PulseIncrease : 0);
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

            if (hasNewAptRecord || totalActions > AppSettings.Default.HighestApt)
            {
                Trophy1.Visibility = Visibility.Visible;
                Trophy2.Visibility = Visibility.Visible;
            } else
            {
                Trophy1.Visibility = Visibility.Collapsed;
                Trophy2.Visibility = Visibility.Collapsed;
            }

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
                _rankLabel.Text = $"{rankInfo.Label}";

                var imagePath = RankSystem.GetRankImage(totalActions);
                if (imagePath != null)
                {
                    _rankImage.Source = imagePath;
                }
            }
        }

        public void ExpandCounters()
        {
            _isHovering = true;
            ApplyVisibilitySettings();
        }

        public void CollapseCounters()
        {
            _isHovering = false;
            ApplyVisibilitySettings();
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
