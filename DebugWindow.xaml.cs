using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using HDTPlugin.Logic;

namespace HDTPlugin
{
    public partial class DebugWindow : Window
    {
        private int _turn = 1;
        private int _unitsPlayed = 0;
        private int _tavernSpells = 0;
        private int _nonTavernSpells = 0;
        private int _buyMinionCount = 0;
        private int _buySpellCount = 0;
        private int _sellMinionCount = 0;
        private int _refreshCount = 0;
        private int _freezeCount = 0;
        private int _leveledUp = 0;
        private InputMoveManager _inputMoveManager;

        public DebugWindow()
        {
            InitializeComponent();
            LoadBackgroundImage();
            SetPluginPosition();
            UpdateDisplay();
            
            // Initialize input move manager for the debug window's preview control
            _inputMoveManager = new InputMoveManager(DisplayControl);
        }

        private void SetPluginPosition()
        {
            var position = PluginPosition.GetDefaultPosition();
            Canvas.SetLeft(DisplayControl, position.X);
            Canvas.SetTop(DisplayControl, position.Y);
        }

        private void LoadBackgroundImage()
        {
            try
            {
                var appDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var hdtDir = Path.Combine(appDir, "HearthstoneDeckTracker");
                var imagePath = Path.Combine(hdtDir, "img", "hsplugin.png");
                if (File.Exists(imagePath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    BackgroundImage.Source = bitmap;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Image not found at: " + imagePath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Failed to load background image: " + ex.Message);
            }
        }

        private void UpdateDisplay()
        {
            TxtMinions.Text = _unitsPlayed.ToString();
            TxtTavernSpells.Text = _tavernSpells.ToString();
            TxtNonTavernSpells.Text = _nonTavernSpells.ToString();
            TxtBuyMinion.Text = _buyMinionCount.ToString();
            TxtBuySpell.Text = _buySpellCount.ToString();
            TxtSellMinion.Text = _sellMinionCount.ToString();
            TxtRefresh.Text = _refreshCount.ToString();
            TxtFreeze.Text = _freezeCount.ToString();
            TxtLeveledUp.Text = _leveledUp.ToString();
            
            DisplayControl.UpdateCounters(_turn, _unitsPlayed, _tavernSpells, _nonTavernSpells,
                _buyMinionCount, _buySpellCount, _sellMinionCount, _refreshCount, _freezeCount, _leveledUp);
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            _turn = 1;
            _unitsPlayed = 0;
            _tavernSpells = 0;
            _nonTavernSpells = 0;
            _buyMinionCount = 0;
            _buySpellCount = 0;
            _sellMinionCount = 0;
            _refreshCount = 0;
            _freezeCount = 0;
            _leveledUp = 0;
            UpdateDisplay();
        }

        private void BtnToggleDrag_Click(object sender, RoutedEventArgs e)
        {
            if (_inputMoveManager != null)
            {
                bool isEnabled = _inputMoveManager.Toggle();
                BtnToggleDrag.Content = isEnabled ? "Lock Position" : "Toggle Dragging";
            }
        }

        // Minions Played
        private void BtnMinionsAdd1_Click(object sender, RoutedEventArgs e) { _unitsPlayed++; UpdateDisplay(); }
        private void BtnMinionsAdd10_Click(object sender, RoutedEventArgs e) { _unitsPlayed += 10; UpdateDisplay(); }
        private void BtnMinionsSub1_Click(object sender, RoutedEventArgs e) { _unitsPlayed = System.Math.Max(0, _unitsPlayed - 1); UpdateDisplay(); }
        private void BtnMinionsSub10_Click(object sender, RoutedEventArgs e) { _unitsPlayed = System.Math.Max(0, _unitsPlayed - 10); UpdateDisplay(); }

        // Tavern Spells
        private void BtnTavernSpellsAdd1_Click(object sender, RoutedEventArgs e) { _tavernSpells++; UpdateDisplay(); }
        private void BtnTavernSpellsAdd10_Click(object sender, RoutedEventArgs e) { _tavernSpells += 10; UpdateDisplay(); }
        private void BtnTavernSpellsSub1_Click(object sender, RoutedEventArgs e) { _tavernSpells = System.Math.Max(0, _tavernSpells - 1); UpdateDisplay(); }
        private void BtnTavernSpellsSub10_Click(object sender, RoutedEventArgs e) { _tavernSpells = System.Math.Max(0, _tavernSpells - 10); UpdateDisplay(); }

        // Non-Tavern Spells
        private void BtnNonTavernSpellsAdd1_Click(object sender, RoutedEventArgs e) { _nonTavernSpells++; UpdateDisplay(); }
        private void BtnNonTavernSpellsAdd10_Click(object sender, RoutedEventArgs e) { _nonTavernSpells += 10; UpdateDisplay(); }
        private void BtnNonTavernSpellsSub1_Click(object sender, RoutedEventArgs e) { _nonTavernSpells = System.Math.Max(0, _nonTavernSpells - 1); UpdateDisplay(); }
        private void BtnNonTavernSpellsSub10_Click(object sender, RoutedEventArgs e) { _nonTavernSpells = System.Math.Max(0, _nonTavernSpells - 10); UpdateDisplay(); }

        // Buy Minion
        private void BtnBuyMinionAdd1_Click(object sender, RoutedEventArgs e) { _buyMinionCount++; UpdateDisplay(); }
        private void BtnBuyMinionAdd10_Click(object sender, RoutedEventArgs e) { _buyMinionCount += 10; UpdateDisplay(); }
        private void BtnBuyMinionSub1_Click(object sender, RoutedEventArgs e) { _buyMinionCount = System.Math.Max(0, _buyMinionCount - 1); UpdateDisplay(); }
        private void BtnBuyMinionSub10_Click(object sender, RoutedEventArgs e) { _buyMinionCount = System.Math.Max(0, _buyMinionCount - 10); UpdateDisplay(); }

        // Buy Spell
        private void BtnBuySpellAdd1_Click(object sender, RoutedEventArgs e) { _buySpellCount++; UpdateDisplay(); }
        private void BtnBuySpellAdd10_Click(object sender, RoutedEventArgs e) { _buySpellCount += 10; UpdateDisplay(); }
        private void BtnBuySpellSub1_Click(object sender, RoutedEventArgs e) { _buySpellCount = System.Math.Max(0, _buySpellCount - 1); UpdateDisplay(); }
        private void BtnBuySpellSub10_Click(object sender, RoutedEventArgs e) { _buySpellCount = System.Math.Max(0, _buySpellCount - 10); UpdateDisplay(); }

        // Sell Minion
        private void BtnSellMinionAdd1_Click(object sender, RoutedEventArgs e) { _sellMinionCount++; UpdateDisplay(); }
        private void BtnSellMinionAdd10_Click(object sender, RoutedEventArgs e) { _sellMinionCount += 10; UpdateDisplay(); }
        private void BtnSellMinionSub1_Click(object sender, RoutedEventArgs e) { _sellMinionCount = System.Math.Max(0, _sellMinionCount - 1); UpdateDisplay(); }
        private void BtnSellMinionSub10_Click(object sender, RoutedEventArgs e) { _sellMinionCount = System.Math.Max(0, _sellMinionCount - 10); UpdateDisplay(); }

        // Refresh
        private void BtnRefreshAdd1_Click(object sender, RoutedEventArgs e) { _refreshCount++; UpdateDisplay(); }
        private void BtnRefreshAdd10_Click(object sender, RoutedEventArgs e) { _refreshCount += 10; UpdateDisplay(); }
        private void BtnRefreshSub1_Click(object sender, RoutedEventArgs e) { _refreshCount = System.Math.Max(0, _refreshCount - 1); UpdateDisplay(); }
        private void BtnRefreshSub10_Click(object sender, RoutedEventArgs e) { _refreshCount = System.Math.Max(0, _refreshCount - 10); UpdateDisplay(); }

        // Freeze
        private void BtnFreezeAdd1_Click(object sender, RoutedEventArgs e) { _freezeCount++; UpdateDisplay(); }
        private void BtnFreezeAdd10_Click(object sender, RoutedEventArgs e) { _freezeCount += 10; UpdateDisplay(); }
        private void BtnFreezeSub1_Click(object sender, RoutedEventArgs e) { _freezeCount = System.Math.Max(0, _freezeCount - 1); UpdateDisplay(); }
        private void BtnFreezeSub10_Click(object sender, RoutedEventArgs e) { _freezeCount = System.Math.Max(0, _freezeCount - 10); UpdateDisplay(); }

        // Leveled Up
        private void BtnLeveledUpAdd1_Click(object sender, RoutedEventArgs e) { _leveledUp++; UpdateDisplay(); }
        private void BtnLeveledUpAdd10_Click(object sender, RoutedEventArgs e) { _leveledUp += 10; UpdateDisplay(); }
        private void BtnLeveledUpSub1_Click(object sender, RoutedEventArgs e) { _leveledUp = System.Math.Max(0, _leveledUp - 1); UpdateDisplay(); }
        private void BtnLeveledUpSub10_Click(object sender, RoutedEventArgs e) { _leveledUp = System.Math.Max(0, _leveledUp - 10); UpdateDisplay(); }
    }
}
