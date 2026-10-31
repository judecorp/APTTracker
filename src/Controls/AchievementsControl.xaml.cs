using System.Windows.Controls;
using APTTracker.Counters;
using APTTracker.Settings;

namespace APTTracker.Controls
{
    public partial class AchievementsControl : Border
    {
        public AchievementsControl()
        {
            InitializeComponent();
            RefreshHighestApt();
        }

        public void RefreshHighestApt()
        {
            int highestApt = AppSettings.Default.HighestApt;
            HighestAptTextBlock.Text = $"{highestApt}";
            HighestRankImage.Source = RankSystem.GetRankImage(highestApt);
            HighestRankName.Text = RankSystem.GetRankLabel(highestApt);
        }
    }
}
