using System.Windows;
using System.Windows.Controls;
using HDTPlugin.Logic;

namespace HDTPlugin.Controls
{
    public partial class AboutControl : Border
    {
        private ScrollViewer _scrollViewer;

        public AboutControl()
        {
            InitializeComponent();
            _scrollViewer = AboutScrollViewer;
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