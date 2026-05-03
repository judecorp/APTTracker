using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HDTPlugin.Controls
{
    public class PlugInDisplayControl : StackPanel
    {
        private readonly TextBlock _totalText;
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
            MinWidth = 180;

            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0x1d, 0x36, 0x57)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x14, 0x16, 0x17)),
                BorderThickness = new Thickness(3),
                Padding = new Thickness(12, 8, 12, 10)
            };

            var mainPanel = new StackPanel();

            _totalText = new TextBlock
            {
                FontSize = 28,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0xC8, 0xE5)),
                Text = "Actions: 0",
                TextAlignment = TextAlignment.Center
            };

            var detailsPanel = new StackPanel
            {
                Margin = new Thickness(0, 8, 0, 0)
            };

            _unitsText = new TextBlock
            {
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC)),
                Text = "Minions Played: 0",
                TextAlignment = TextAlignment.Center
            };

            _tavernSpellsText = new TextBlock
            {
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC)),
                Text = "Tavern Spells: 0",
                TextAlignment = TextAlignment.Center
            };

            _nonTavernSpellsText = new TextBlock
            {
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC)),
                Text = "Non-Tavern Spells: 0",
                TextAlignment = TextAlignment.Center
            };

            _cardsBoughtText = new TextBlock
            {
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50)),
                Text = "Cards Bought: 0",
                TextAlignment = TextAlignment.Center
            };

            _sellMinionText = new TextBlock
            {
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36)),
                Text = "Sell Minion: 0",
                TextAlignment = TextAlignment.Center
            };

            _refreshText = new TextBlock
            {
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(0x21, 0x96, 0xF3)),
                Text = "Refresh: 0",
                TextAlignment = TextAlignment.Center
            };

            _freezeText = new TextBlock
            {
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(0x9C, 0x27, 0xB0)),
                Text = "Freeze: 0",
                TextAlignment = TextAlignment.Center
            };

            _tavernTierText = new TextBlock
            {
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0xD7, 0x00)),
                Text = "Leveled Up: 0",
                TextAlignment = TextAlignment.Center
            };

            detailsPanel.Children.Add(_unitsText);
            detailsPanel.Children.Add(_tavernSpellsText);
            detailsPanel.Children.Add(_nonTavernSpellsText);
            detailsPanel.Children.Add(_cardsBoughtText);
            detailsPanel.Children.Add(_sellMinionText);
            detailsPanel.Children.Add(_refreshText);
            detailsPanel.Children.Add(_freezeText);
            detailsPanel.Children.Add(_tavernTierText);

            mainPanel.Children.Add(_totalText);
            mainPanel.Children.Add(detailsPanel);

            border.Child = mainPanel;
            Children.Add(border);
        }

        public void UpdateCounters(int turn, int unitsPlayed, int tavernSpells, int nonTavernSpells,
            int buyMinionCount = 0, int buySpellCount = 0, int sellMinionCount = 0,
            int refreshCount = 0, int freezeCount = 0, int leveledUp = 0)
        {
            int totalBought = buyMinionCount + buySpellCount;
            int totalActions = unitsPlayed + tavernSpells + nonTavernSpells +
                           totalBought + sellMinionCount + refreshCount + freezeCount + leveledUp;
            _totalText.Text = $"Actions: {totalActions}";
            _unitsText.Text = $"Minions Played: {unitsPlayed}";
            _tavernSpellsText.Text = $"Tavern Spells: {tavernSpells}";
            _nonTavernSpellsText.Text = $"Non-Tavern Spells: {nonTavernSpells}";
            _cardsBoughtText.Text = $"Cards Bought: {totalBought}";
            _sellMinionText.Text = $"Sell Minion: {sellMinionCount}";
            _refreshText.Text = $"Refresh: {refreshCount}";
            _freezeText.Text = $"Freeze: {freezeCount}";
            _tavernTierText.Text = $"Leveled Up: {leveledUp}";
        }
    }
}
