using System.Windows;
using System.Windows.Controls;
using APTTracker.Logic;
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

        public void RecalculateScaling()
        {
            Width = UIScaler.GetMinWidth();
            MaxHeight = UIScaler.GetSettingsMaxHeight();
            if (_scrollViewer != null)
            {
                _scrollViewer.MaxHeight = UIScaler.GetSettingsMaxHeight();
            }
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