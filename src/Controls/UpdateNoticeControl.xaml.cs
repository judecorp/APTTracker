using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using APTTracker.Updates;

namespace APTTracker.Controls
{
    public partial class UpdateNoticeControl : TextBlock
    {
        public UpdateNoticeControl()
        {
            InitializeComponent();
            Visibility = Visibility.Collapsed;
        }

        internal void SetAvailableUpdate(AvailableUpdate update)
        {
            UpdateLink.NavigateUri = update.ReleaseUrl;
            UpdateText.Text = $"v{update.Version} is available";
        }

        private void Update_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            }
            catch (System.Exception ex)
            {
                Hearthstone_Deck_Tracker.Utility.Logging.Log.Warn(
                    $"[APTTracker] Could not open update link. {ex}");
            }
            e.Handled = true;
        }
    }
}
