using HDTPlugin.Controls;
using HDTPlugin.Logic;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Card = Hearthstone_Deck_Tracker.Hearthstone.Card;
using Core = Hearthstone_Deck_Tracker.API.Core;

namespace HDTPlugin
{
    public class HDTBootstrap : IPlugin
    {
        private PlugInDisplayControl _displayControl;
        private DebugWindow _debugWindow;
        private bool _isTracking;
        private string _logFilePath;
        private StreamWriter _logWriter;
        private InputMoveManager _inputMoveManager;

        // Shop action counters for verification
        private int _buyMinionCount;
        private int _buySpellCount;
        private int _sellMinionCount;
        private int _refreshCount;
        private int _freezeCount;
        private int _tavernTier;

        private int _turnNumber;
        private int _unitsPlayed;
        private int _tavernSpells;
        private int _nonTavernSpells;

        public string Author => "Author";
        public string ButtonText => "BG Action Tracker";
        public string Description => "Tracks actions per turn in Battlegrounds";
        public string Name => "BG Action Tracker";
        public Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        public MenuItem MenuItem => null;

        public void OnLoad()
        {
            _displayControl = new PlugInDisplayControl();
            _displayControl.Visibility = Visibility.Collapsed;
            Core.OverlayCanvas.Children.Add(_displayControl);

            // Load position from settings
            Canvas.SetTop(_displayControl, global::HDTPlugin.Properties.Settings.Default.Top);
            Canvas.SetLeft(_displayControl, global::HDTPlugin.Properties.Settings.Default.Left);

            // Initialize input move manager for dragging functionality
            _inputMoveManager = new InputMoveManager(_displayControl);

            // Initialize counters
            ResetCounters();

            // Initialize PowerLog line logging
            var logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HDTPlugin");
            Directory.CreateDirectory(logDir);
            _logFilePath = Path.Combine(logDir, "PowerLogLines.log");
            _logWriter = new StreamWriter(_logFilePath, false) { AutoFlush = true };
            _logWriter.WriteLine($"[{DateTime.Now}] HDTPlugin loaded, subscribing to OnPowerLogLine");

            GameEvents.OnGameStart.Add(GameStart);
            GameEvents.OnGameEnd.Add(GameEnd);
            GameEvents.OnTurnStart.Add(TurnStart);
            GameEvents.OnPlayerPlay.Add(OnCardPlayed);

            LogEvents.OnPowerLogLine.Add(OnPowerLogLine);
            _logWriter.WriteLine($"[{DateTime.Now}] Subscribed to OnPowerLogLine");
        }

        public void OnUnload()
        {
            if (_debugWindow != null)
            {
                _debugWindow.Close();
                _debugWindow = null;
            }
            if (_logWriter != null)
            {
                _logWriter.Close();
                _logWriter = null;
            }
            if (_displayControl != null)
            {
                Core.OverlayCanvas.Children.Remove(_displayControl);
                _displayControl = null;
            }
            if (_inputMoveManager != null)
            {
                _inputMoveManager.Dispose();
                _inputMoveManager = null;
            }
        }

        private void OnPowerLogLine(string line)
        {
            if (_logWriter == null) return;

            // Only process BLOCK_START BlockType=PLAY lines
            if (!line.Contains("BLOCK_START BlockType=PLAY"))
                return;

            // Extract cardId using regex
            var cardIdMatch = Regex.Match(line, @"cardId=(\S+)");
            if (!cardIdMatch.Success) return;

            string cardId = cardIdMatch.Groups[1].Value;
            string entityName = "";
            var nameMatch = Regex.Match(line, @"entityName=(\S+)");
            if (nameMatch.Success) entityName = nameMatch.Groups[1].Value;

            // Extract player ID
            int playerId = 0;
            var playerMatch = Regex.Match(line, @"player=(\d+)");
            if (playerMatch.Success) playerId = int.Parse(playerMatch.Groups[1].Value);

            // Identify shop action based on cardId
            if (cardId == "TB_BaconShop_DragBuy")
            {
                _buyMinionCount++;
                _logWriter.WriteLine($"[{DateTime.Now}] BUY MINION: {entityName} (Player: {playerId})");
                UpdateDisplay();
            }
            else if (cardId == "TB_BaconShop_DragBuy_Spell")
            {
                _buySpellCount++;
                _logWriter.WriteLine($"[{DateTime.Now}] BUY SPELL: {entityName} (Player: {playerId})");
                UpdateDisplay();
            }
            else if (cardId == "TB_BaconShop_DragSell")
            {
                _sellMinionCount++;
                _logWriter.WriteLine($"[{DateTime.Now}] SELL MINION: {entityName} (Player: {playerId})");
                UpdateDisplay();
            }
            else if (cardId == "TB_BaconShop_8p_Reroll_Button")
            {
                _refreshCount++;
                _logWriter.WriteLine($"[{DateTime.Now}] REFRESH: {entityName} (Player: {playerId})");
                UpdateDisplay();
            }
            else if (cardId == "TB_BaconShopLockAll_Button")
            {
                _freezeCount++;
                _logWriter.WriteLine($"[{DateTime.Now}] FREEZE: {entityName} (Player: {playerId})");
                UpdateDisplay();
            }
            // Check for Tavern Tier upgrade (TB_BaconShopTechUp0X_Button)
            else if (cardId.StartsWith("TB_BaconShopTechUp"))
            {
                // Increment level up counter (player pressed tavern up button)
                _tavernTier++;
                _logWriter.WriteLine($"[{DateTime.Now}] LEVELED UP: {entityName} (Count: {_tavernTier})");
                UpdateDisplay();
            }
        }

        public void OnUpdate() { }
        public void OnButtonPress()
        {
            if (_debugWindow == null || !_debugWindow.IsLoaded)
            {
                _debugWindow = new DebugWindow();
                _debugWindow.Closed += (s, e) => _debugWindow = null;
                _debugWindow.Show();
            }
            else
            {
                _debugWindow.Activate();
            }
        }

        private void GameStart()
        {
            if (Core.Game.CurrentGameMode != GameMode.Battlegrounds)
                return;

            _isTracking = true;
            _turnNumber =0;
            ResetCounters();
            _displayControl.Visibility = Visibility.Visible;
            UpdateDisplay();
        }

        private void GameEnd()
        {
            _isTracking = false;
            _displayControl.Visibility = Visibility.Collapsed;
        }

        private void TurnStart(ActivePlayer player)
        {
            if (!_isTracking || player != ActivePlayer.Player)
                return;

            ResetCounters(); // Resets ALL counters for the turn
            _turnNumber++;
            UpdateDisplay();
        }

        private void OnCardPlayed(Card card)
        {
            if (!_isTracking)
                return;

            switch (card.Type)
            {
                case "Battleground_Spell":
                    _tavernSpells++;
                    break;
                case "Spell":
                    _nonTavernSpells++;
                    break;
                case "Minion":
                    _unitsPlayed++;
                    break;
            }

            UpdateDisplay();
        }

        private void ResetCounters()
        {
            _unitsPlayed = 0;
            _tavernSpells = 0;
            _nonTavernSpells = 0;
            _buyMinionCount = 0;
            _buySpellCount = 0;
            _sellMinionCount = 0;
            _refreshCount = 0;
            _freezeCount = 0;
            _tavernTier = 0; // Start at 0 (no level ups yet this turn)
        }

        private void UpdateDisplay()
        {
            _displayControl.UpdateCounters(_turnNumber, _unitsPlayed, _tavernSpells, _nonTavernSpells,
                _buyMinionCount, _buySpellCount, _sellMinionCount, _refreshCount, _freezeCount, _tavernTier);
        }
    }
}
