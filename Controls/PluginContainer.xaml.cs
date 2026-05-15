using System.Windows;
using System.Windows.Controls;
using HDTPlugin.Logic;

namespace HDTPlugin.Controls
{
    public partial class PluginContainer : Grid
    {
        private readonly PlugInDisplayControl _displayControl;
        private readonly SettingsControl _settingsControl;
        private readonly Button _backButton;
        private readonly Button _settingsButton;

        public PluginContainer()
        {
            InitializeComponent();
            
            _displayControl = DisplayPanel;
            _settingsControl = SettingsPanel;
            _backButton = BackButton;
            _settingsButton = SettingsButton;
        }

        public PlugInDisplayControl DisplayControl => _displayControl;

        public void RecalculateScaling()
        {
            _displayControl.RecalculateScaling();
            _settingsControl.RecalculateScaling();
            ApplyButtonScaling();
            Width = UIScaler.GetMinWidth();
        }

        private void ApplyButtonScaling()
        {
            double buttonFontSize = UIScaler.GetSettingsFontSize();
            _settingsButton.FontSize = buttonFontSize;
            _backButton.FontSize = buttonFontSize;
        }

        public void ShowSettings()
        {
            _displayControl.Visibility = Visibility.Collapsed;
            _settingsControl.Visibility = Visibility.Visible;
            _backButton.Visibility = Visibility.Visible;
            _settingsButton.Visibility = Visibility.Collapsed;
        }

        public void ShowDisplay()
        {
            _settingsControl.Visibility = Visibility.Collapsed;
            _displayControl.Visibility = Visibility.Visible;
            _backButton.Visibility = Visibility.Collapsed;
            _settingsButton.Visibility = Visibility.Visible;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSettings();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            ShowDisplay();
        }
    }
}