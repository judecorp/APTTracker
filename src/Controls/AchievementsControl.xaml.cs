using System.Windows;
using System.Windows.Controls;
using APTTracker.Counters;
using APTTracker.Overlay;
using APTTracker.Settings;

namespace APTTracker.Controls
{
    public partial class AchievementsControl : Border
    {
        private ScrollViewer _scrollViewer;
        private TextBlock _highestAptText;

        public AchievementsControl()
        {
            InitializeComponent();
            _scrollViewer = AchievementsScrollViewer;
            _highestAptText = (TextBlock)FindName("HighestAptTextBlock");
        }

        public void RefreshHighestApt()
        {
            if (_highestAptText != null)
            {
                _highestAptText.Text = $"{AppSettings.Default.HighestApt}";
            }
        }
    }
}