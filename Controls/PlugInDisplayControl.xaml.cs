using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using HDTPlugin.Logic;

namespace HDTPlugin.Controls
{
    public partial class PlugInDisplayControl : StackPanel
    {
        private readonly TextBlock _summaryCounter;
        private readonly TextBlock _apmText;
        
        private readonly TextBlock _combatTimeText;
        private readonly TextBlock _shopTimeText;
        private readonly TextBlock _unitsText;
        private readonly TextBlock _tavernSpellsText;
        private readonly TextBlock _nonTavernSpellsText;
        private readonly TextBlock _cardsBoughtText;
        private readonly TextBlock _sellMinionText;
        private readonly TextBlock _refreshText;
        private readonly TextBlock _freezeText;
        private readonly TextBlock _tavernTierText;
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

        private readonly Dictionary<CounterKey, TextBlock> _textBlocks = new Dictionary<CounterKey, TextBlock>();

        public PlugInDisplayControl()
        {
            InitializeComponent();
            
            _summaryCounter = SummaryCounter;
            _apmText = ApmText;
            _combatTimeText = CombatTimeText;
            _shopTimeText = ShopTimeText;
            _unitsText = UnitsText;
            _tavernSpellsText = TavernSpellsText;
            _nonTavernSpellsText = NonTavernSpellsText;
            _cardsBoughtText = CardsBoughtText;
            _sellMinionText = SellMinionText;
            _refreshText = RefreshText;
            _freezeText = FreezeText;
            _tavernTierText = TavernTierText;
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

            _textBlocks[CounterKey.UNITS_PLAYED] = _unitsText;
            _textBlocks[CounterKey.TAVERN_SPELLS] = _tavernSpellsText;
            _textBlocks[CounterKey.NON_TAVERN_SPELLS] = _nonTavernSpellsText;
            _textBlocks[CounterKey.BUY_MINION_COUNT] = _cardsBoughtText; // Will be handled specially
            _textBlocks[CounterKey.BUY_SPELL_COUNT] = _cardsBoughtText; // Will be handled specially
            _textBlocks[CounterKey.SELL_MINION_COUNT] = _sellMinionText;
            _textBlocks[CounterKey.REFRESH_COUNT] = _refreshText;
            _textBlocks[CounterKey.FREEZE_COUNT] = _freezeText;
            _textBlocks[CounterKey.LEVELED_UP] = _tavernTierText;

            ApplyScaling();
            ApplyVisibilitySettings();
        }

        public void ApplyVisibilitySettings()
        {
            var settings = Properties.Settings.Default;
            _combatShopTimePanel.Visibility = settings.ShowCombatTime || settings.ShowShopTime ? Visibility.Visible : Visibility.Collapsed;
            _combatTimeText.Visibility = settings.ShowCombatTime ? Visibility.Visible : Visibility.Collapsed;
            _combatShopSlash.Visibility = settings.ShowCombatTime && settings.ShowShopTime ? Visibility.Visible : Visibility.Collapsed;
            _shopTimeText.Visibility = settings.ShowShopTime ? Visibility.Visible : Visibility.Collapsed;
            _apmPanel.Visibility = settings.ShowAPM ? Visibility.Visible : Visibility.Collapsed;
            _rankImage.Visibility = settings.ShowRank ? Visibility.Visible : Visibility.Collapsed;
            _rankLabel.Visibility = settings.ShowRank ? Visibility.Visible : Visibility.Collapsed;
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
            _unitsText.FontSize = UIScaler.GetBodyFontSize();
            _tavernSpellsText.FontSize = UIScaler.GetBodyFontSize();
            _nonTavernSpellsText.FontSize = UIScaler.GetBodyFontSize();
            _cardsBoughtText.FontSize = UIScaler.GetBodyFontSize();
            _sellMinionText.FontSize = UIScaler.GetBodyFontSize();
            _refreshText.FontSize = UIScaler.GetBodyFontSize();
            _freezeText.FontSize = UIScaler.GetBodyFontSize();
            _tavernTierText.FontSize = UIScaler.GetBodyFontSize();

            _rankImage.Width = UIScaler.GetRankBorderSize();
            _rankImage.Height = UIScaler.GetRankBorderSize();

            _counterBorder.Height = UIScaler.GetCounterHeight();

            MinWidth = UIScaler.GetMinWidth();
        }

        private const double PulseIncrease = 6;
        private int _previousTotalActions;
        private int _previousRankIndex = -1;
        private GameCounters _lastCounters;
        private bool _isHovering;

        public void UpdateCounters(GameCounters counters)
        {
            _lastCounters = counters;
            int totalBought = counters.TotalBought;
            int totalActions = counters.TotalActions;

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
            _apmText.Text = counters.Apm > 0 ? $"{counters.Apm:F2}" : "0.00";
            _combatTimeText.Text = counters.CombatLength > 0 ? $"{counters.CombatLength}s" : "0s";
            _shopTimeText.Text = counters.ShopLength > 0 ? $"{counters.ShopLength}s" : "???";

            foreach (var kvp in _textBlocks)
            {
                CounterKey key = kvp.Key;
                TextBlock textBlock = kvp.Value;
                int value = counters[key];
                
                // Special handling for Cards Bought (combines BuyMinionCount + BuySpellCount)
                if (key == CounterKey.BUY_MINION_COUNT || key == CounterKey.BUY_SPELL_COUNT)
                {
                    // Skip individual handling, will be handled together below
                    continue;
                }
                
                UpdateCounterText(textBlock, key.ToString(), value);
            }
            
            // Handle Cards Bought separately (combines BuyMinionCount + BuySpellCount)
            if (_textBlocks.ContainsKey(CounterKey.BUY_MINION_COUNT))
            {
                int cardsBought = counters[CounterKey.BUY_MINION_COUNT] + counters[CounterKey.BUY_SPELL_COUNT];
                UpdateCounterText(_textBlocks[CounterKey.BUY_MINION_COUNT], "Cards Bought", cardsBought);
            }

            UpdateRank(totalActions);
        }

        private void UpdateCounterText(TextBlock textBlock, string label, int value)
        {
            textBlock.Text = $"{label}: {value}";
            textBlock.Visibility = value > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateCounterText(TextBlock textBlock, string label, int value, string suffix)
        {
            textBlock.Text = $"{label}: {value}{suffix}";
            textBlock.Visibility = value > 0 ? Visibility.Visible : Visibility.Collapsed;
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

        private void SummaryBorder_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ExpandCounters();
        }

        private void SummaryBorder_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_countersBorder.IsMouseOver)
                CollapseCounters();
        }

        private void ExpandCounters()
        {
            _isHovering = true;
            var settings = Properties.Settings.Default;
            bool hasAlways = settings.CounterMinionsPlayed == 2 || settings.CounterTavernSpells == 2 ||
                settings.CounterNonTavernSpells == 2 || settings.CounterCardsBought == 2 ||
                settings.CounterSellMinion == 2 || settings.CounterRefresh == 2 ||
                settings.CounterFreeze == 2 || settings.CounterLeveledUp == 2;

            bool hasHover = settings.CounterMinionsPlayed == 1 || settings.CounterTavernSpells == 1 ||
                settings.CounterNonTavernSpells == 1 || settings.CounterCardsBought == 1 ||
                settings.CounterSellMinion == 1 || settings.CounterRefresh == 1 ||
                settings.CounterFreeze == 1 || settings.CounterLeveledUp == 1;

            if (hasAlways)
            {
                _countersBorder.Visibility = Visibility.Visible;
                ApplyCounterVisibility(hasHover);
            }
            else
            {
                ApplyCounterVisibility(true);
                _countersBorder.Visibility = hasHover ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void CollapseCounters()
        {
            _isHovering = false;
            var settings = Properties.Settings.Default;
            bool hasAlways = settings.CounterMinionsPlayed == 2 || settings.CounterTavernSpells == 2 ||
                settings.CounterNonTavernSpells == 2 || settings.CounterCardsBought == 2 ||
                settings.CounterSellMinion == 2 || settings.CounterRefresh == 2 ||
                settings.CounterFreeze == 2 || settings.CounterLeveledUp == 2;

            if (hasAlways)
            {
                ApplyCounterVisibility(false);
            }
            else
            {
                _countersBorder.Visibility = Visibility.Collapsed;
            }
        }

        private void ApplyCounterVisibility(bool isHover)
        {
            if (_lastCounters == null) return;
            
            var settings = Properties.Settings.Default;
            _unitsText.Visibility = GetCounterVisibility(settings.CounterMinionsPlayed, isHover, _lastCounters[CounterKey.UNITS_PLAYED]);
            _tavernSpellsText.Visibility = GetCounterVisibility(settings.CounterTavernSpells, isHover, _lastCounters[CounterKey.TAVERN_SPELLS]);
            _nonTavernSpellsText.Visibility = GetCounterVisibility(settings.CounterNonTavernSpells, isHover, _lastCounters[CounterKey.NON_TAVERN_SPELLS]);
            _cardsBoughtText.Visibility = GetCounterVisibility(settings.CounterCardsBought, isHover, _lastCounters[CounterKey.BUY_MINION_COUNT] + _lastCounters[CounterKey.BUY_SPELL_COUNT]);
            _sellMinionText.Visibility = GetCounterVisibility(settings.CounterSellMinion, isHover, _lastCounters[CounterKey.SELL_MINION_COUNT]);
            _refreshText.Visibility = GetCounterVisibility(settings.CounterRefresh, isHover, _lastCounters[CounterKey.REFRESH_COUNT]);
            _freezeText.Visibility = GetCounterVisibility(settings.CounterFreeze, isHover, _lastCounters[CounterKey.FREEZE_COUNT]);
            _tavernTierText.Visibility = GetCounterVisibility(settings.CounterLeveledUp, isHover, _lastCounters[CounterKey.LEVELED_UP]);
        }

        private Visibility GetCounterVisibility(int setting, bool isHover, int value)
        {
            if (value <= 0) return Visibility.Collapsed;
            if (setting == 2) return Visibility.Visible;
            if (setting == 1 && isHover) return Visibility.Visible;
            return Visibility.Collapsed;
        }

        private void CountersBorder_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _isHovering = true;
            var settings = Properties.Settings.Default;
            bool hasHover = settings.CounterMinionsPlayed == 1 || settings.CounterTavernSpells == 1 ||
                settings.CounterNonTavernSpells == 1 || settings.CounterCardsBought == 1 ||
                settings.CounterSellMinion == 1 || settings.CounterRefresh == 1 ||
                settings.CounterFreeze == 1 || settings.CounterLeveledUp == 1;
            if (hasHover)
            {
                ApplyCounterVisibility(true);
            }
        }

        private void CountersBorder_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_summaryBorder.IsMouseOver)
                CollapseCounters();
        }

        private void Plugin_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            CollapseCounters();
        }
    }
}