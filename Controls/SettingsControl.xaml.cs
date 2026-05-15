using System.Windows;
using System.Windows.Controls;
using HDTPlugin.Logic;

namespace HDTPlugin.Controls
{
    public partial class SettingsControl : Border
    {
        private ScrollViewer _scrollViewer;

        public SettingsControl()
        {
            InitializeComponent();
            _scrollViewer = SettingsScrollViewer;
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