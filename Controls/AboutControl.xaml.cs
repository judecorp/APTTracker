using System.Windows;
using System.Windows.Controls;
using HDTPlugin.Logic;
using System.Diagnostics;
using System.Windows.Navigation;

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

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            }
            catch { }
            e.Handled = true;
        }
    }
}