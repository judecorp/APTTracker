using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.IconPacks;
using HDTPlugin.Logic;
using HDTPlugin.Data;

namespace HDTPlugin.Controls
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

        public PluginContainer(CounterState state)
        {
            InitializeComponent();
            _state = state;
            
            _displayControl = DisplayPanel;
            _settingsControl = SettingsPanel;
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

        public void RecalculateScaling()
        {
            _displayControl.RecalculateScaling();
            _settingsControl.RecalculateScaling();
            _achievementsControl.RecalculateScaling();
            _aboutControl.RecalculateScaling();
            ApplyButtonScaling();
            Width = UIScaler.GetMinWidth();
        }

        private void ApplyButtonScaling()
        {
            double iconSize = UIScaler.GetSettingsButtonSize();
            double iconPaddingSize = UIScaler.GetSettingsButtonPaddingSize();


            // Scale icon sizes based on resolution
            _helpIcon.Width = iconSize;
            _helpIcon.Height = iconSize;
            _trophyIcon.Width = iconSize;
            _trophyIcon.Height = iconSize;
            _settingsIcon.Width = iconSize;
            _settingsIcon.Height = iconSize;
            _backIcon.Width = iconSize;
            _backIcon.Height = iconSize;
            
            // Align margins to match XAML styling
            _aboutButton.Margin = new Thickness(0, 0, iconPaddingSize, 0);
            _achievementsButton.Margin = new Thickness(0, 0, iconPaddingSize, 0);
            _settingsButton.Margin = new Thickness(0, 0, 0, 0);
            
            // Back button matches XAML styling
            _backButton.Margin = new Thickness(4, 2, 0, 0);
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