using System;
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

            ApplyScaling();
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

        public void UpdateCounters(int turn, int unitsPlayed, int tavernSpells, int nonTavernSpells,
            int buyMinionCount = 0, int buySpellCount = 0, int sellMinionCount = 0,
            int refreshCount = 0, int freezeCount = 0, int leveledUp = 0, 
            int shopLength = 0, int combatLength = 0, double apm = 0)
        {
            int totalBought = buyMinionCount + buySpellCount;
            int totalActions = unitsPlayed + tavernSpells + nonTavernSpells +
                           totalBought + sellMinionCount + refreshCount + freezeCount + leveledUp;

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
            _apmText.Text = apm > 0 ? $"{apm:F2}" : "0.00";
            _combatTimeText.Text = combatLength > 0 ? $"{combatLength}s" : "0s";
            _shopTimeText.Text = shopLength > 0 ? $"{shopLength}s" : "???";
            UpdateCounterText(_unitsText, "Minions Played", unitsPlayed);
            UpdateCounterText(_tavernSpellsText, "Tavern Spells", tavernSpells);
            UpdateCounterText(_nonTavernSpellsText, "Non-Tavern Spells", nonTavernSpells);
            UpdateCounterText(_cardsBoughtText, "Cards Bought", totalBought);
            UpdateCounterText(_sellMinionText, "Sell Minion", sellMinionCount);
            UpdateCounterText(_refreshText, "Refresh", refreshCount);
            UpdateCounterText(_freezeText, "Freeze", freezeCount);
            UpdateCounterText(_tavernTierText, "Leveled Up", leveledUp);

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
            _countersBorder.Visibility = Visibility.Visible;
        }

        private void CollapseCounters()
        {
            _countersBorder.Visibility = Visibility.Collapsed;
        }

        private void CountersBorder_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
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