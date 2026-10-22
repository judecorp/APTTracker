using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MahApps.Metro.IconPacks;
using APTTracker.Logic;
using APTTracker.Settings;
using System.Collections.Generic;
using APTTracker.Data;

namespace APTTracker.Controls
{
public partial class SettingsControl : Border
        {
        // ── colours shared across all rows ──────────────────────────────────
        private static readonly Color ColorAlways = Color.FromRgb(0x4C, 0xAF, 0x50); // green
        private static readonly Color ColorHover  = Color.FromRgb(0xFF, 0x98, 0x00); // orange
        private static readonly Color ColorNever  = Color.FromRgb(0xF4, 0x43, 0x36); // red
        private static readonly Color ColorGrey   = Color.FromRgb(0x88, 0x88, 0x88);

        // ── per-row button tracking ─────────────────────────────────────────
        // Key: arbitrary string tag (CounterKey.ToString() for counters, "ShowXxx" for show rows)
        private readonly Dictionary<string, Button[]> _rowButtons = new Dictionary<string, Button[]>();

        private readonly AppSettings _settings = AppSettings.Default;
        private ScrollViewer _scrollViewer;
        private StackPanel _showControlsPanel;
        private StackPanel _counterControlsPanel;
        private StackPanel _allButtonsPanel;
        private Slider _scaleSlider;
        private TextBlock _scaleValueText;

        // ── show-row definitions ────────────────────────────────────────────
        private static readonly (string Tag, string Label)[] ShowRows =
        {
            ("ShowCombatTime", "Combat Time"),
            ("ShowShopTime",   "Shop Time"),
            ("ShowAPM",        "APM"),
            ("ShowRank",       "Rank"),
        };

        public SettingsControl()
        {
            InitializeComponent();
            _scrollViewer        = SettingsScrollViewer;
            _showControlsPanel   = ShowControlsPanel;
            _counterControlsPanel = CounterControlsPanel;
            _allButtonsPanel     = AllButtonsPanel;
            _scaleSlider         = ScaleSlider;
            _scaleValueText      = ScaleValueText;

            // Setup scaling slider
            _scaleSlider.Value = UIStateManager.Instance.ManualScale;
            _scaleSlider.ValueChanged += (s, e) => {
                UIStateManager.Instance.ManualScale = _scaleSlider.Value;
                _scaleValueText.Text = $"{UIStateManager.Instance.ManualScale:F1}";
            };
            _scaleValueText.Text = $"{UIStateManager.Instance.ManualScale:F1}";

            CreateAllButtons();
            CreateShowRows();
            CreateCounterRows();
            LoadSettings();
        }

        // ════════════════════════════════════════════════════════════════════
        // SET-ALL BUTTONS
        // ════════════════════════════════════════════════════════════════════

        private void CreateAllButtons()
        {
            _allButtonsPanel.Children.Add(CreateAllButton("Set All to Always Show", "Eye",           2));
            _allButtonsPanel.Children.Add(CreateAllButton("Set All to Show on Hover", "CursorDefault", 1));
            _allButtonsPanel.Children.Add(CreateAllButton("Set All to Never Show",   "Cancel",        0));
            // Add Set All to Defaults button
            var text = new TextBlock
            {
                Text                = "Set All to Defaults",
                Foreground          = new SolidColorBrush(ColorGrey),
                VerticalAlignment   = VerticalAlignment.Center,
                Margin              = new Thickness(0, 0, 4, 0)
            };
            
            // Invisible icon placeholder
            var iconBorder = new Border
            {
                BorderThickness = new Thickness(0),
                Width           = 16, 
                Height          = 16
            };

            var panel = new StackPanel { Orientation = Orientation.Horizontal };
            panel.Children.Add(text);
            panel.Children.Add(iconBorder);

            var defaultsBtn = new Button
            {
                Content                 = panel,
                Style                   = (Style)FindResource("IconButtonStyle"),
                HorizontalContentAlignment = HorizontalAlignment.Left,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width                   = Double.NaN,
                Margin                  = new Thickness(0, 0, 0, 8),
                Padding                 = new Thickness(4, 2, 4, 2)
            };
            defaultsBtn.Click += SetAllDefaults_Click;
            defaultsBtn.MouseEnter += (s, e) => text.Foreground = Brushes.White;
            defaultsBtn.MouseLeave += (s, e) => text.Foreground = new SolidColorBrush(ColorGrey);
            _allButtonsPanel.Children.Add(defaultsBtn);
        }

        private void SetAllDefaults_Click(object sender, RoutedEventArgs e)
        {
            // Reset counters to their defined default visibility
            foreach (var kvp in CounterDataProvider.Counters)
            {
                var key = kvp.Key;
                var defaultVis = kvp.Value.DefaultVisibility;
                _settings.CounterVisibility[key] = defaultVis;
                ApplyRowButtons(key.ToString(), defaultVis);
            }
            // Reset show rows to always (2) as before
            foreach (var row in ShowRows)
                SetShowSetting(row.Tag, 2, persist: false);
            _settings.Save();
        }


        private Button CreateAllButton(string label, string iconKind, int value)
        {
            var icon = MakeIcon(iconKind, ColorGrey, 12);

            var iconBorder = new Border
            {
                BorderThickness = new Thickness(1),
                BorderBrush     = new SolidColorBrush(StateColor(value)),
                Child           = icon,
                Padding         = new Thickness(2)
            };

            var text = new TextBlock
            {
                Text                = label,
                Foreground          = new SolidColorBrush(ColorGrey),
                VerticalAlignment   = VerticalAlignment.Center,
                Margin              = new Thickness(0, 0, 4, 0)
            };

Action<bool> hover = on =>
            {
                // Text should turn white on hover, regardless of selection state
                text.Foreground = on ? Brushes.White : new SolidColorBrush(ColorGrey);
                // Icon remains grey (handled separately)
                // No change to icon.Foreground here
            };

            var panel = new StackPanel { Orientation = Orientation.Horizontal };
            panel.Children.Add(text);
            panel.Children.Add(iconBorder);

            var btn = new Button
            {
                Content                 = panel,
                Style                   = (Style)FindResource("IconButtonStyle"),
                BorderThickness         = new Thickness(0),
                Tag                     = value,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width                   = Double.NaN, // size to content
                Margin                  = new Thickness(0, 0, 0, 8),
                Padding                 = new Thickness(4, 2, 4, 2)
            };

            btn.Click += (s, e) =>
            {
                // set all counter rows
                foreach (var key in CounterDataProvider.Counters.Keys)
                {
                    _settings.CounterVisibility[key] = value;
                    ApplyRowButtons(key.ToString(), value);
                }
                // set all show rows
                foreach (var row in ShowRows)
                    SetShowSetting(row.Tag, value, persist: false);

                _settings.Save();
            };

            btn.MouseEnter += (s, e) => hover(true);
            btn.MouseLeave += (s, e) => hover(false);
            return btn;
        }

        // ════════════════════════════════════════════════════════════════════
        // SHOW ROWS (Combat Time / Shop Time / APM / Rank)
        // ════════════════════════════════════════════════════════════════════

        private void CreateShowRows()
        {
            foreach (var row in ShowRows)
            {
                var label = new TextBlock
                {
                    Text              = row.Label,
                    Foreground        = Brushes.White,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var grid = CreateVisibilityRow(
                    label, row.Tag,
                    onClick: (tag, v) => SetShowSetting(tag, v, persist: true));

                _showControlsPanel.Children.Add(grid);
            }
        }

        private void SetShowSetting(string tag, int value, bool persist)
        {
            switch (tag)
            {
                case "ShowCombatTime": _settings.ShowCombatTimeVisibility = value; break;
                case "ShowShopTime":   _settings.ShowShopTimeVisibility   = value; break;
                case "ShowAPM":        _settings.ShowAPMVisibility         = value; break;
                case "ShowRank":       _settings.ShowRankVisibility        = value; break;
            }
            ApplyRowButtons(tag, value);
            if (persist) _settings.Save();
        }

        // ════════════════════════════════════════════════════════════════════
        // COUNTER ROWS
        // ════════════════════════════════════════════════════════════════════

        private void CreateCounterRows()
        {
            foreach (var kvp in CounterDataProvider.Counters)
            {
                var key   = kvp.Key;
                var data  = kvp.Value;

                var label = new TextBlock
                {
                    Text              = data.DisplayName,
                    Foreground        = new SolidColorBrush(Color.FromRgb(data.Color.R, data.Color.G, data.Color.B)),
                    VerticalAlignment = VerticalAlignment.Center
                };

                var grid = CreateVisibilityRow(
                    label, key.ToString(),
                    onClick: (tag, v) =>
                    {
                        _settings.CounterVisibility[key] = v;
                        ApplyRowButtons(tag, v);
                        _settings.Save();
                    });

                _counterControlsPanel.Children.Add(grid);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // SHARED ROW FACTORY  (the DRY core)
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Creates a Grid with a label on the left and three state-buttons
        /// (Always / Hover / Never) right-aligned.  Registers buttons in
        /// _rowButtons under <paramref name="tag"/>.
        /// </summary>
        private Grid CreateVisibilityRow(TextBlock label, string tag, Action<string, int> onClick)
        {
            var grid = new Grid { Margin = new Thickness(0, 2, 0, 0) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            Grid.SetColumn(label, 0);
            grid.Children.Add(label);

            var buttons = new Button[3];
            buttons[0] = MakeStateButton("Eye",           2, tag, onClick); // Always
            buttons[1] = MakeStateButton("CursorDefault", 1, tag, onClick); // Hover
            buttons[2] = MakeStateButton("Cancel",        0, tag, onClick); // Never

            for (int i = 0; i < buttons.Length; i++)
            {
                Grid.SetColumn(buttons[i], i + 1);
                grid.Children.Add(buttons[i]);
            }

            _rowButtons[tag] = buttons;
            return grid;
        }

        /// <summary>Creates a single Always/Hover/Never icon button.</summary>
        private Button MakeStateButton(string iconKind, int value, string tag, Action<string, int> onClick)
        {
            var icon = MakeIcon(iconKind, ColorGrey, 12);

            var border = new Border
            {
                Padding         = new Thickness(2),
                BorderThickness = new Thickness(0),
                BorderBrush     = new SolidColorBrush(StateColor(value)),
                Child           = icon
            };

            var btn = new Button
            {
                Content         = border,
                Margin          = new Thickness(2),
                MinWidth        = 16,
                MinHeight       = 16,
                Style           = (Style)FindResource("IconButtonStyle"),
                BorderThickness = new Thickness(0)
            };

            btn.Click      += (s, e) => onClick(tag, value);
            btn.MouseEnter += (s, e) =>
            {
                // Determine if this button is the currently selected one by checking border thickness
                bool isSelected = false;
                if (btn.Content is Border b)
                {
                    isSelected = b.BorderThickness.Left > 0;
                }
                icon.Foreground = isSelected ? new SolidColorBrush(ColorGrey) : Brushes.White;
            };
            btn.MouseLeave += (s, e) => icon.Foreground = new SolidColorBrush(ColorGrey);

            return btn;
        }

        // ════════════════════════════════════════════════════════════════════
        // BUTTON STATE HELPERS
        // ════════════════════════════════════════════════════════════════════

        /// <summary>Highlights the selected button, dims the others.</summary>
        private void ApplyRowButtons(string tag, int value)
        {
            if (!_rowButtons.TryGetValue(tag, out var btns)) return;

            // btns[0]=Always(2), btns[1]=Hover(1), btns[2]=Never(0)
            int[] stateValues = { 2, 1, 0 };
            for (int i = 0; i < btns.Length; i++)
            {
                bool selected = stateValues[i] == value;
                if (btns[i].Content is Border b)
                {
                    b.BorderThickness = selected ? new Thickness(1) : new Thickness(0);
                    b.BorderBrush     = new SolidColorBrush(StateColor(stateValues[i]));
                    if (b.Child is PackIconMaterial ico)
                        ico.Foreground = new SolidColorBrush(ColorGrey);
                }
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // LOAD SETTINGS
        // ════════════════════════════════════════════════════════════════════

        private void LoadSettings()
        {
            // Show rows
            ApplyRowButtons("ShowCombatTime", _settings.ShowCombatTimeVisibility);
            ApplyRowButtons("ShowShopTime",   _settings.ShowShopTimeVisibility);
            ApplyRowButtons("ShowAPM",        _settings.ShowAPMVisibility);
            ApplyRowButtons("ShowRank",       _settings.ShowRankVisibility);

            // Counter rows
            foreach (var kvp in CounterDataProvider.Counters)
                ApplyRowButtons(kvp.Key.ToString(), _settings.CounterVisibility[kvp.Key]);
        }

        // ════════════════════════════════════════════════════════════════════
        // SMALL UTILITIES
        // ════════════════════════════════════════════════════════════════════

        private static Color StateColor(int value) =>
            value == 2 ? ColorAlways :
            value == 1 ? ColorHover  : ColorNever;

        private static PackIconMaterial MakeIcon(string kind, Color fg, double size) =>
            new PackIconMaterial
            {
                Kind              = (PackIconMaterialKind)Enum.Parse(typeof(PackIconMaterialKind), kind),
                Width             = size,
                Height            = size,
                Foreground        = new SolidColorBrush(fg),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment   = VerticalAlignment.Center
            };
    }
}
