using System.Windows;
using System.Windows.Controls;
using HDTPlugin.Logic;

namespace HDTPlugin.Controls
{
    public partial class AchievementsControl : Border
    {
        private ScrollViewer _scrollViewer;

        public AchievementsControl()
        {
            InitializeComponent();
            _scrollViewer = AchievementsScrollViewer;
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
    }
}