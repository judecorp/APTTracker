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
using System.Windows.Threading;
using Card = Hearthstone_Deck_Tracker.Hearthstone.Card;
using Core = Hearthstone_Deck_Tracker.API.Core;

namespace HDTPlugin
{
    public class HDTBootstrap : IPlugin
    {
        private PluginContainer _pluginContainer;
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
        private DateTime _turnStartTime;
        private int _lastShopLength;
        private int _lastCombatLength;
        private int _currentTimeout;
        private DispatcherTimer _apmTimer;

        public string Author => "Author";
        public string ButtonText => "BG Action Tracker";
        public string Description => "Tracks actions per turn in Battlegrounds";
        public string Name => "BG Action Tracker";
        public Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        public MenuItem MenuItem => null;

        public void OnLoad()
        {
            _pluginContainer = new PluginContainer();
            _pluginContainer.Visibility = Visibility.Collapsed;
            Core.OverlayCanvas.Children.Add(_pluginContainer);

            Point position;
            if (PluginPosition.HasSavedPosition && PluginPosition.IsOverlayAvailable)
                position = PluginPosition.GetSavedPosition();
            else
                position = PluginPosition.GetDefaultPosition();

            Canvas.SetTop(_pluginContainer, position.Y);
            Canvas.SetLeft(_pluginContainer, position.X);

            // Initialize input move manager for dragging functionality
            _inputMoveManager = new InputMoveManager(_pluginContainer);
            _inputMoveManager.Enable();

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

            _apmTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _apmTimer.Tick += ApmTimer_Tick;
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
            if (_pluginContainer != null)
            {
                Core.OverlayCanvas.Children.Remove(_pluginContainer);
                _pluginContainer = null;
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

            // Track TIMEOUT value (allocated shop time)
            if (line.Contains("TAG_CHANGE Entity=SLOSK#1451 tag=TIMEOUT value="))
            {
                var timeoutMatch = Regex.Match(line, @"tag=TIMEOUT value=(\d+)");
                if (timeoutMatch.Success)
                {
                    _currentTimeout = int.Parse(timeoutMatch.Groups[1].Value);
                }
                return;
            }

            // Track turn changes - calculate length when new turn starts
            if (line.Contains("TAG_CHANGE Entity=GameEntity tag=TURN value="))
            {
                var turnMatch = Regex.Match(line, @"tag=TURN value=(\d+)");
                if (turnMatch.Success)
                {
                    int newTurn = int.Parse(turnMatch.Groups[1].Value);
                    if (_turnStartTime != DateTime.MinValue && newTurn > 1)
                    {
                        var duration = DateTime.Now - _turnStartTime;
                        int elapsedSeconds = (int)duration.TotalSeconds;
                        
                        // Even turn = shop phase, odd turn = combat phase
                        if (newTurn % 2 == 0)
                        {
                            // Starting shop phase - reset shop length, keep combat length
                            _lastShopLength = 0;
                            _lastCombatLength = Math.Max(0, _currentTimeout - elapsedSeconds);
                            _logWriter.WriteLine($"[{DateTime.Now}] SHOP TURN {newTurn} STARTED - Combat from turn {newTurn - 1}: {_lastCombatLength}s (timeout: {_currentTimeout}s)");
                        }
                        else
                        {
                            // Starting combat phase - show previous shop length, calculate combat
                            _lastShopLength = elapsedSeconds;
                            _lastCombatLength = Math.Max(0, _currentTimeout - _lastShopLength);
                            _logWriter.WriteLine($"[{DateTime.Now}] COMBAT TURN {newTurn} STARTED - Shop from turn {newTurn - 1}: {_lastShopLength}s, Combat: {_lastCombatLength}s (timeout: {_currentTimeout}s)");
                        }
                    }
                    _turnStartTime = DateTime.Now;
                    if (_isTracking)
                        UpdateDisplay();
                }
                return;
            }

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

        private ResolutionTier _lastTier;
        
        public void OnUpdate()
        {
            if (_pluginContainer == null)
                return;

            if (_debugWindow != null && _debugWindow.IsLoaded)
                return;

            UIScaler.DetectResolutionTier();
            if (UIScaler.CurrentTier != _lastTier)
            {
                _lastTier = UIScaler.CurrentTier;
                _pluginContainer.DisplayControl.RecalculateScaling();
                if (!PluginPosition.HasSavedPosition)
                {
                    var position = PluginPosition.GetDefaultPosition();
                    Canvas.SetTop(_pluginContainer, position.Y);
                    Canvas.SetLeft(_pluginContainer, position.X);
                }
                else if (PluginPosition.IsOverlayAvailable)
                {
                    var position = PluginPosition.GetSavedPosition();
                    Canvas.SetTop(_pluginContainer, position.Y);
                    Canvas.SetLeft(_pluginContainer, position.X);
                }
            }

            // Update APM display during active turn
            if (_isTracking)
                UpdateDisplay();
        }
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
            _turnNumber = 0;
            _turnStartTime = DateTime.MinValue;
            _lastShopLength = 0;
            _lastCombatLength = 0;
            _currentTimeout = 0;
            ResetCounters();
            _pluginContainer.DisplayControl.Visibility = Visibility.Visible;
            _apmTimer.Start();
            Point position;
            if (PluginPosition.HasSavedPosition && PluginPosition.IsOverlayAvailable)
                position = PluginPosition.GetSavedPosition();
            else
                position = PluginPosition.GetDefaultPosition();

            Canvas.SetTop(_pluginContainer, position.Y);
            Canvas.SetLeft(_pluginContainer, position.X);
            UpdateDisplay();
        }

        private void GameEnd()
        {
            _isTracking = false;
            _apmTimer.Stop();
            _pluginContainer.DisplayControl.Visibility = Visibility.Collapsed;
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
            double apm = CalculateApm();
            _pluginContainer.DisplayControl.UpdateCounters(_turnNumber, _unitsPlayed, _tavernSpells, _nonTavernSpells,
                _buyMinionCount, _buySpellCount, _sellMinionCount, _refreshCount, _freezeCount, _tavernTier, 
                _lastShopLength, _lastCombatLength, apm);
        }

        private double CalculateApm()
        {
            if (_turnStartTime == DateTime.MinValue || !_isTracking)
                return 0;

            var elapsed = DateTime.Now - _turnStartTime;
            if (elapsed.TotalSeconds < 1)
                return 0;

            int totalActions = _unitsPlayed + _tavernSpells + _nonTavernSpells +
                _buyMinionCount + _buySpellCount + _sellMinionCount + _refreshCount + _freezeCount + _tavernTier;

            return Math.Round(totalActions * 60.0 / elapsed.TotalSeconds, 2);
        }

        private void ApmTimer_Tick(object sender, EventArgs e)
        {
            if (_isTracking)
                UpdateDisplay();
        }
    }
}
