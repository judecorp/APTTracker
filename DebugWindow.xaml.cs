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
        private int _shopLength = 0;
        private int _combatLength = 0;
        private InputMoveManager _inputMoveManager;

        public DebugWindow()
        {
            InitializeComponent();
            
            // Initialize input move manager for the debug window's preview control
            _inputMoveManager = new InputMoveManager(DisplayControl);
            _inputMoveManager.Enable();
            
            LoadBackgroundImage();
            SetPluginPosition();
            UpdateDisplay();
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
                _buyMinionCount, _buySpellCount, _sellMinionCount, _refreshCount, _freezeCount, _leveledUp, _shopLength, _combatLength, 0);
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

        private void Btn1080p_Click(object sender, RoutedEventArgs e)
        {
            UIScaler.SetResolutionTier(Logic.ResolutionTier.HD1080p);
            TxtCurrentTier.Text = "Current: 1080p";
            RefreshUI(1.0);
        }

        private void Btn1440p_Click(object sender, RoutedEventArgs e)
        {
            UIScaler.SetResolutionTier(Logic.ResolutionTier.QHD1440p);
            TxtCurrentTier.Text = "Current: 1440p";
            RefreshUI(1.33);
        }

        private void Btn4K_Click(object sender, RoutedEventArgs e)
        {
            UIScaler.SetResolutionTier(Logic.ResolutionTier.UHD4K);
            TxtCurrentTier.Text = "Current: 4K";
            RefreshUI(2.0);
        }

        private void RefreshUI(double scaleFactor)
        {
            DisplayControl.RecalculateScaling();
            
            // Scale background from top-right corner
            BackgroundScale.ScaleX = scaleFactor;
            BackgroundScale.ScaleY = scaleFactor;
            
            // In debug window, shift left by offscreen pixels, then scale
            // 4K: screenWidth=3840, visible=1920, offscreen=1920
            // 1440p: screenWidth=2560, visible=1920, offscreen=640
            // 1080p: screenWidth=1920, visible=1920, offscreen=0
            var basePosition = Logic.PluginPosition.GetDefaultPosition();
            double debugWindowWidth = 1920.0;
            double screenWidth;
            if (scaleFactor > 1.5)
                screenWidth = 3840;  // 4K
            else if (scaleFactor > 1.0)
                screenWidth = 2560;  // 1440p
            else
                screenWidth = 1920;  // 1080p
            double offscreen = screenWidth - debugWindowWidth;
            double scaledLeft = (basePosition.X - offscreen);
            Canvas.SetLeft(DisplayControl, scaledLeft);
            Canvas.SetTop(DisplayControl, basePosition.Y);
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

        // Shop Length
        private void BtnShopLengthAdd1_Click(object sender, RoutedEventArgs e) { _shopLength++; UpdateDisplay(); }
        private void BtnShopLengthAdd10_Click(object sender, RoutedEventArgs e) { _shopLength += 10; UpdateDisplay(); }
        private void BtnShopLengthSub1_Click(object sender, RoutedEventArgs e) { _shopLength = System.Math.Max(0, _shopLength - 1); UpdateDisplay(); }
        private void BtnShopLengthSub10_Click(object sender, RoutedEventArgs e) { _shopLength = System.Math.Max(0, _shopLength - 10); UpdateDisplay(); }

        // Combat Length
        private void BtnCombatLengthAdd1_Click(object sender, RoutedEventArgs e) { _combatLength++; UpdateDisplay(); }
        private void BtnCombatLengthAdd10_Click(object sender, RoutedEventArgs e) { _combatLength += 10; UpdateDisplay(); }
        private void BtnCombatLengthSub1_Click(object sender, RoutedEventArgs e) { _combatLength = System.Math.Max(0, _combatLength - 1); UpdateDisplay(); }
        private void BtnCombatLengthSub10_Click(object sender, RoutedEventArgs e) { _combatLength = System.Math.Max(0, _combatLength - 10); UpdateDisplay(); }
    }
}
