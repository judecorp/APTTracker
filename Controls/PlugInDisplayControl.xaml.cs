using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using HDTPlugin.Logic;

namespace HDTPlugin.Controls
{
    public partial class PlugInDisplayControl : StackPanel
    {
        private readonly TextBlock _summaryCounter;
        private readonly TextBlock _apmText;
        
        private readonly TextBlock _shopLengthText;
        private readonly TextBlock _combatLengthText;
        private readonly TextBlock _unitsText;
        private readonly TextBlock _tavernSpellsText;
        private readonly TextBlock _nonTavernSpellsText;
        private readonly TextBlock _cardsBoughtText;
        private readonly TextBlock _sellMinionText;
        private readonly TextBlock _refreshText;
        private readonly TextBlock _freezeText;
        private readonly TextBlock _tavernTierText;
        private readonly Border _rankBorder;
        private readonly TextBlock _rankLabel;
        private readonly Border _summaryBorder;
        private readonly Border _countersBorder;
        private readonly Border _counterBorder;
        private readonly TextBlock _apmLabel;

        public PlugInDisplayControl()
        {
            InitializeComponent();
            
            _summaryCounter = SummaryCounter;
            _apmText = ApmText;
            _shopLengthText = ShopLengthText;
            _combatLengthText = CombatLengthText;
            _unitsText = UnitsText;
            _tavernSpellsText = TavernSpellsText;
            _nonTavernSpellsText = NonTavernSpellsText;
            _cardsBoughtText = CardsBoughtText;
            _sellMinionText = SellMinionText;
            _refreshText = RefreshText;
            _freezeText = FreezeText;
            _tavernTierText = TavernTierText;
            _rankBorder = RankBorder;
            _rankLabel = RankLabel;
            _summaryBorder = SummaryBorder;
            _countersBorder = CountersBorder;
            _counterBorder = CounterBorder;
            _apmLabel = ApmLabel;

            _countersBorder.Visibility = Visibility.Collapsed;

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
            _shopLengthText.FontSize = UIScaler.GetBodyFontSize();
            _combatLengthText.FontSize = UIScaler.GetBodyFontSize();
            _unitsText.FontSize = UIScaler.GetBodyFontSize();
            _tavernSpellsText.FontSize = UIScaler.GetBodyFontSize();
            _nonTavernSpellsText.FontSize = UIScaler.GetBodyFontSize();
            _cardsBoughtText.FontSize = UIScaler.GetBodyFontSize();
            _sellMinionText.FontSize = UIScaler.GetBodyFontSize();
            _refreshText.FontSize = UIScaler.GetBodyFontSize();
            _freezeText.FontSize = UIScaler.GetBodyFontSize();
            _tavernTierText.FontSize = UIScaler.GetBodyFontSize();

            _rankBorder.Width = UIScaler.GetRankBorderSize();
            _rankBorder.Height = UIScaler.GetRankBorderSize();

            //_counterBorder.MinWidth = 90;
            _counterBorder.Height = UIScaler.GetCounterHeight();

            MinWidth = UIScaler.GetMinWidth();
        }

        private const double PulseIncrease = 6;
        private int _previousTotalActions;

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
            UpdateCounterText(_shopLengthText, "Shop Length", shopLength, "s");
            UpdateCounterText(_combatLengthText, "Combat Length", combatLength, "s");
            UpdateCounterText(_unitsText, "Minions Played", unitsPlayed);
            UpdateCounterText(_tavernSpellsText, "Tavern Spells", tavernSpells);
            UpdateCounterText(_nonTavernSpellsText, "Non-Tavern Spells", nonTavernSpells);
            UpdateCounterText(_cardsBoughtText, "Cards Bought", totalBought);
            UpdateCounterText(_sellMinionText, "Sell Minion", sellMinionCount);
            UpdateCounterText(_refreshText, "Refresh", refreshCount);
            UpdateCounterText(_freezeText, "Freeze", freezeCount);
            UpdateCounterText(_tavernTierText, "Leveled Up", leveledUp);
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

        private System.Windows.Threading.DispatcherTimer _hideTimer;

        private void SummaryBorder_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _hideTimer?.Stop();
            _countersBorder.Visibility = Visibility.Visible;
        }

        private void SummaryBorder_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _hideTimer?.Stop();
            _hideTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(150)
            };
            _hideTimer.Tick += (s, args) =>
            {
                _hideTimer.Stop();
                if (!IsMouseOverSummaryOrCounters())
                    _countersBorder.Visibility = Visibility.Collapsed;
            };
            _hideTimer.Start();
        }

        private void CountersBorder_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _hideTimer?.Stop();
            _countersBorder.Visibility = Visibility.Visible;
        }

        private void CountersBorder_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _hideTimer?.Stop();
            _hideTimer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(150)
            };
            _hideTimer.Tick += (s, args) =>
            {
                _hideTimer.Stop();
                if (!IsMouseOverSummaryOrCounters())
                    _countersBorder.Visibility = Visibility.Collapsed;
            };
            _hideTimer.Start();
        }

        private bool IsMouseOverSummaryOrCounters()
        {
            return _summaryBorder.IsMouseOver || _countersBorder.IsMouseOver;
        }
    }
}