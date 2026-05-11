using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace HDTPlugin.Controls
{
    public partial class PlugInDisplayControl : StackPanel
    {
        private readonly TextBlock _summaryCounter;
        private readonly TextBlock _turnLengthText;
        private readonly TextBlock _apmText;
        
        private readonly TextBlock _unitsText;
        private readonly TextBlock _tavernSpellsText;
        private readonly TextBlock _nonTavernSpellsText;
        private readonly TextBlock _cardsBoughtText;
        private readonly TextBlock _sellMinionText;
        private readonly TextBlock _refreshText;
        private readonly TextBlock _freezeText;
        private readonly TextBlock _tavernTierText;

        public PlugInDisplayControl()
        {
            InitializeComponent();
            
            _summaryCounter = SummaryCounter;
            _turnLengthText = TurnLengthText;
            _apmText = ApmText;
            _unitsText = UnitsText;
            _tavernSpellsText = TavernSpellsText;
            _nonTavernSpellsText = NonTavernSpellsText;
            _cardsBoughtText = CardsBoughtText;
            _sellMinionText = SellMinionText;
            _refreshText = RefreshText;
            _freezeText = FreezeText;
            _tavernTierText = TavernTierText;
        }

        private const double MinFontSize = 56;
        private const double MaxFontSize = 78;
        private const int MaxScaleValue = 100;
        private const double PulseIncrease = 8;
        private int _previousTotalActions;

        public void UpdateCounters(int turn, int unitsPlayed, int tavernSpells, int nonTavernSpells,
            int buyMinionCount = 0, int buySpellCount = 0, int sellMinionCount = 0,
            int refreshCount = 0, int freezeCount = 0, int leveledUp = 0)
        {
            int totalBought = buyMinionCount + buySpellCount;
            int totalActions = unitsPlayed + tavernSpells + nonTavernSpells +
                           totalBought + sellMinionCount + refreshCount + freezeCount + leveledUp;

            double scale = Math.Min(totalActions, MaxScaleValue) / (double)MaxScaleValue;
            double targetFontSize = MinFontSize + (MaxFontSize - MinFontSize) * scale;
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
    }
}