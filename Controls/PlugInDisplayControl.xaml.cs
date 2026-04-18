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

        public PlugInDisplayControl()
        {
            MinWidth = 120;

            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0x1d, 0x36, 0x57)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(0x14, 0x16, 0x17)),
                BorderThickness = new Thickness(2),
                Padding = new Thickness(8, 4, 8, 6)
            };

            var mainPanel = new StackPanel();

            _totalText = new TextBlock
            {
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0xC8, 0xE5)),
                Text = "Actions: 0",
                TextAlignment = TextAlignment.Center
            };

            var detailsPanel = new StackPanel
            {
                Margin = new Thickness(0, 4, 0, 0)
            };

            _unitsText = new TextBlock
            {
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC)),
                Text = "Minions Played: 0",
                TextAlignment = TextAlignment.Center
            };

            _tavernSpellsText = new TextBlock
            {
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC)),
                Text = "Tavern Spells: 0",
                TextAlignment = TextAlignment.Center
            };

            _nonTavernSpellsText = new TextBlock
            {
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC)),
                Text = "Non-Tavern Spells: 0",
                TextAlignment = TextAlignment.Center
            };

            detailsPanel.Children.Add(_unitsText);
            detailsPanel.Children.Add(_tavernSpellsText);
            detailsPanel.Children.Add(_nonTavernSpellsText);

            mainPanel.Children.Add(_totalText);
            mainPanel.Children.Add(detailsPanel);

            border.Child = mainPanel;
            Children.Add(border);
        }

        public void UpdateCounters(int turn, int unitsPlayed, int tavernSpells, int nonTavernSpells)
        {
            _totalText.Text = $"Actions: {unitsPlayed + tavernSpells + nonTavernSpells}";
            _unitsText.Text = $"Minions Played: {unitsPlayed}";
            _tavernSpellsText.Text = $"Tavern Spells: {tavernSpells}";
            _nonTavernSpellsText.Text = $"Non-Tavern Spells: {nonTavernSpells}";
        }
    }
}
