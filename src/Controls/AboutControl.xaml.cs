using APTTracker.Logic;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace APTTracker.Controls
{
    public partial class AboutControl : Border
    {
        private ScrollViewer _scrollViewer;

        public AboutControl()
        {
            InitializeComponent();
            _scrollViewer = AboutScrollViewer;

            var version = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion ?? "Unknown";

            VersionText.Text = $"APTTracker v{version}";
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