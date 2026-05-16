using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
            ScaleFonts();
        }

        private void ScaleFonts()
        {
            double headerFontSize = UIScaler.GetSettingsFontSize();
            double bodyFontSize = headerFontSize * 0.9;
            double buttonSize = UIScaler.GetSettingsButtonSize();

            if (_scrollViewer?.Content is StackPanel stackPanel)
            {
                ScaleFontsRecursive(stackPanel, headerFontSize, bodyFontSize, buttonSize);
            }
        }

        private void ScaleFontsRecursive(DependencyObject parent, double headerSize, double bodySize, double buttonSize)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                
                if (child is TextBlock textBlock)
                {
                    if (textBlock.FontWeight == FontWeights.Bold)
                        textBlock.FontSize = headerSize;
                    else
                        textBlock.FontSize = bodySize;
                }
                else if (child is CheckBox checkBox)
                {
                    checkBox.FontSize = bodySize * 0.9;
                }
                else if (child is Button button)
                {
                    button.FontSize = bodySize * 0.8;
                    button.Width = buttonSize;
                    button.Height = buttonSize;
                }
                else if (child is Separator)
                {
                    // Separator doesn't need scaling
                }
                
                ScaleFontsRecursive(child, headerSize, bodySize, buttonSize);
            }
        }
    }
}