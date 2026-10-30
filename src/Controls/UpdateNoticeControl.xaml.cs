using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace APTTracker.Controls
{
    public partial class UpdateNoticeControl : TextBlock
    {
        // Temporary UI preview state, shared by both update notices and the About icon.
        public static bool IsUpdateAvailable => true;

        public UpdateNoticeControl()
        {
            InitializeComponent();
            Visibility = IsUpdateAvailable ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Update_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[APTTracker] Could not open update link: {ex.Message}");
            }
            e.Handled = true;
        }
    }
}
