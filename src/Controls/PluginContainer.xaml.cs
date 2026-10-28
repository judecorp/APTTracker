using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.IconPacks;
using APTTracker.Counters;
using APTTracker.Overlay;
using System.Windows.Threading;

namespace APTTracker.Controls
{
    public partial class PluginContainer : Grid
    {
        private readonly PlugInDisplayControl _displayControl;
        private readonly SettingsControl _settingsControl;
        private readonly AchievementsControl _achievementsControl;
        private readonly AboutControl _aboutControl;
        private readonly Button _backButton;
        private readonly Button _settingsButton;
        private readonly Button _achievementsButton;
        private readonly Button _aboutButton;
        private readonly PackIconMaterial _settingsIcon;
        private readonly PackIconMaterial _trophyIcon;
        private readonly PackIconMaterial _helpIcon;
        private readonly PackIconMaterial _backIcon;
        private readonly CounterState _state;
        private Canvas _positionCanvas;
        private bool _positionUpdatePending;

        private void QueuePositionUpdate()
        {
            if (_positionCanvas == null || _positionUpdatePending)
                return;

            _positionUpdatePending = true;
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new System.Action(() =>
            {
                _positionUpdatePending = false;
                if (_positionCanvas == null || _positionCanvas.ActualWidth <= 0 || _positionCanvas.ActualHeight <= 0)
                    return;

                PluginPosition.RestorePosition(this);
            }));
        }

        private void PositionSizeChanged(object sender, SizeChangedEventArgs e)
        {
            QueuePositionUpdate();
        }

        private void PositionLoaded(object sender, RoutedEventArgs e)
        {
            UIStateManager.Instance.PropertyChanged += ScaleChanged;
            ApplyScale();
            _positionCanvas = Parent as Canvas;
            if (_positionCanvas != null)
                _positionCanvas.SizeChanged += PositionSizeChanged;
            QueuePositionUpdate();
        }

        private void PositionUnloaded(object sender, RoutedEventArgs e)
        {
            UIStateManager.Instance.PropertyChanged -= ScaleChanged;
            if (_positionCanvas != null)
                _positionCanvas.SizeChanged -= PositionSizeChanged;
            _positionCanvas = null;
        }

        private void ScaleChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(UIStateManager.TotalScale))
                ApplyScale();
        }

        private void ApplyScale()
        {
            ContainerScale.ScaleX = UIStateManager.Instance.TotalScale;
            ContainerScale.ScaleY = UIStateManager.Instance.TotalScale;
            QueuePositionUpdate();
        }

        public PluginContainer(CounterState state)
        {
            InitializeComponent();
            _state = state;
            Loaded += PositionLoaded;
            Unloaded += PositionUnloaded;
            SizeChanged += PositionSizeChanged;
            
            // Bind to UIStateManager
            ContainerScale.ScaleX = UIStateManager.Instance.TotalScale;
            ContainerScale.ScaleY = UIStateManager.Instance.TotalScale;
            
            _displayControl = DisplayPanel;
            _settingsControl = SettingsPanel;
            _settingsControl.VisibilitySettingsChanged += (sender, args) =>
                _displayControl.ApplyVisibilitySettings();
            _achievementsControl = AchievementsPanel;
            _aboutControl = AboutPanel;
            _backButton = BackButton;
            _settingsButton = SettingsButton;
            _achievementsButton = AchievementsButton;
            _aboutButton = AboutButton;
            _settingsIcon = SettingsIcon;
            _trophyIcon = TrophyIcon;
            _helpIcon = HelpIcon;
            _backIcon = BackIcon;
        }

        public PlugInDisplayControl DisplayControl => _displayControl;

        // Expose a refresh for achievement stats (e.g., highest APT)
        public void RefreshAchievements()
        {
            _achievementsControl?.RefreshHighestApt();
        }

        public void ShowDisplay()
        {
            _settingsControl.Visibility = Visibility.Collapsed;
            _achievementsControl.Visibility = Visibility.Collapsed;
            _aboutControl.Visibility = Visibility.Collapsed;
            _displayControl.Visibility = Visibility.Visible;
            _displayControl.ApplyVisibilitySettings();
            _backButton.Visibility = Visibility.Collapsed;
        }

        private void ShowSettings()
        {
            _displayControl.Visibility = Visibility.Collapsed;
            _achievementsControl.Visibility = Visibility.Collapsed;
            _aboutControl.Visibility = Visibility.Collapsed;
            _settingsControl.Visibility = Visibility.Visible;
            _backButton.Visibility = Visibility.Visible;
        }

        private void ShowAchievements()
        {
            _displayControl.Visibility = Visibility.Collapsed;
            _settingsControl.Visibility = Visibility.Collapsed;
            _aboutControl.Visibility = Visibility.Collapsed;
            _achievementsControl.Visibility = Visibility.Visible;
            _backButton.Visibility = Visibility.Visible;
        }

        private void ShowAbout()
        {
            _displayControl.Visibility = Visibility.Collapsed;
            _settingsControl.Visibility = Visibility.Collapsed;
            _achievementsControl.Visibility = Visibility.Collapsed;
            _aboutControl.Visibility = Visibility.Visible;
            _backButton.Visibility = Visibility.Visible;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSettings();
        }

        private void AchievementsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowAchievements();
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            ShowAbout();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            ShowDisplay();
        }

        private void HeaderButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string iconName)
            {
                var icon = FindName(iconName) as PackIconMaterial;
                if (icon != null)
                    icon.Foreground = System.Windows.Media.Brushes.White;
            }
        }

        private void HeaderButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string iconName)
            {
                var icon = FindName(iconName) as PackIconMaterial;
                if (icon != null)
                    icon.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0x88, 0x88, 0x88));
            }
        }
        private void Plugin_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _displayControl.ExpandCounters();
        }

        private void Plugin_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _displayControl.CollapseCounters();
        }
    }
}
