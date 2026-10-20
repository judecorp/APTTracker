using APTTracker.Controls;
using APTTracker.Data;
using APTTracker.Logic;
using APTTracker.Settings;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Plugins;
using System;
using System.Collections.Generic;
using System.Diagnostics.PerformanceData;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Card = Hearthstone_Deck_Tracker.Hearthstone.Card;
using Core = Hearthstone_Deck_Tracker.API.Core;

namespace APTTracker
{
    public class HDTBootstrap : IPlugin
    {
        private PluginContainer _pluginContainer;
        private readonly CounterState _counterState = new CounterState();
        private DebugWindow _debugWindow;
        private bool _isTracking = false;
        private bool _isInCombat = false;
        private string _logFilePath;
        private StreamWriter _logWriter;
        private InputMoveManager _inputMoveManager;

        // Shop action counters for verification
        private int _turnNumber = 0;
        private DateTime _turnStartTime = DateTime.MinValue;
        private int _lastShopLength = 0;
        private int _lastCombatLength = 0;
        private int _currentTimeout = 0;

        public string Author => "JudeCorp";
        public string ButtonText => "BG Action Tracker";
        public string Description => "Tracks actions per turn in Battlegrounds";
        public string Name => "BG Action Tracker";
        public Version Version => new Version(Assembly.GetExecutingAssembly()
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
        ?.InformationalVersion);
        public MenuItem MenuItem => null;

        public void OnLoad()
        {
            _pluginContainer = new PluginContainer(_counterState);
#if DEBUG
            _pluginContainer.Visibility = Visibility.Visible;
#else
            _pluginContainer.Visibility = Visibility.Collapsed;
#endif
            Core.OverlayCanvas.Children.Add(_pluginContainer);
            Hearthstone_Deck_Tracker.Utility.Extensions.OverlayExtensions.SetIsOverlayHitTestVisible(_pluginContainer, true);

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
            var logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "APTTracker");
            Directory.CreateDirectory(logDir);
            _logFilePath = Path.Combine(logDir, "PowerLogLines.log");
            _logWriter = new StreamWriter(_logFilePath, false) { AutoFlush = true };
            _logWriter.WriteLine($"[{DateTime.Now}] HDTPlugin loaded, subscribing to OnPowerLogLine");

            GameEvents.OnGameStart.Add(GameStart);
            GameEvents.OnGameEnd.Add(GameEnd);
            GameEvents.OnTurnStart.Add(TurnStart);
            GameEvents.OnPlayerPlay.Add(OnCardPlayed);
            GameEvents.OnPlayerHeroPower.Add(OnHeroPower);

            LogEvents.OnPowerLogLine.Add(OnPowerLogLine);
            LogEvents.OnPowerGamestateLogLine.Add(OnPowerGamestateLogLine);

            _pluginContainer.RecalculateScaling();
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

        private CounterKey? GetCounterKeyFromCardId(string cardId)
        {
            foreach (var kvp in CounterDataProvider.Counters)
            {
                if (kvp.Value.ParsingDetails is LogParsingDetails logDetails && logDetails.MatchStrategy == MatchStrategy.STARTS_WITH && cardId.StartsWith(logDetails.CardId))
                {
                    return kvp.Key;
                }
                else if(kvp.Value.ParsingDetails is LogParsingDetails exactLogDetails && exactLogDetails.MatchStrategy == MatchStrategy.EXACT && cardId == exactLogDetails.CardId)
                {
                    return kvp.Key;
                }
            }
            return null;
        }
        private CounterKey? GetCounterKeyFromCardType(string cardType)
        {
            foreach (var kvp in CounterDataProvider.Counters)
            {
                if (kvp.Value.ParsingDetails is APIParsingDetails apiDetails && apiDetails.CardType == cardType)
                {
                    return kvp.Key;
                }
            }
            return null;
        }

        private CounterKey? GetCounterKeyFromCardTag(Card card)
        {
            foreach (var kvp in CounterDataProvider.Counters)
            {
                if (kvp.Value.ParsingDetails is TagParsingDetails tagDetails && card.HasTag(tagDetails.GameTag))
                {
                    return kvp.Key;
                }
            }
            return null;
        }

        private void OnPowerLogLine(string line)
        {
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
                            // Starting combat phase - show previous shop length, calculate combat
                            _isInCombat = true;
                            //_lastShopLength = elapsedSeconds;

                            //_logWriter.WriteLine($"[{DateTime.Now}] SHOP TURN {newTurn} STARTED - Combat from turn {newTurn - 1}: {_lastCombatLength}s (timeout: {_currentTimeout}s)");
                        }
                        else
                        {
                            // Starting shop phase - reset shop length, keep combat length
                            _lastShopLength = 0;
                            if (_turnStartTime != DateTime.MinValue)
                            {
                                _lastCombatLength = elapsedSeconds;
                            }
                            _isInCombat = false;
                            //_logWriter.WriteLine($"[{DateTime.Now}] COMBAT TURN {newTurn} STARTED - Shop from turn {newTurn - 1}: {_lastShopLength}s, Combat: {_lastCombatLength}s (timeout: {_currentTimeout}s)");
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
            var counterKey = GetCounterKeyFromCardId(cardId);
            if (counterKey.HasValue)
            {
                _counterState[counterKey.Value]++;
                //_logWriter.WriteLine($"[{DateTime.Now}] ACTION: {CounterDataProvider.Counters[counterKey.Value].DisplayName} ({entityName}) (Player: {playerId})");
                UpdateDisplay();
            }
        }

        private void OnPowerGamestateLogLine(string line)
        {
            if (line.Contains("m_chosenEntities"))
            {
                foreach (var kvp in CounterDataProvider.Counters)
                {
                    if (kvp.Value.ParsingDetails is GameStateCounter)
                    {
                        _counterState[kvp.Key]++;
                    }
                }
            }
        }

        private ResolutionTier _lastTier;

        public void OnUpdate()
        {
#if !DEBUG
            if (!_isTracking)
                return;

            if (_pluginContainer == null)
                return;

            if (_debugWindow != null && _debugWindow.IsLoaded)
                return;
#endif

            UIScaler.DetectResolutionTier();
            if (UIScaler.CurrentTier != _lastTier)
            {
                _lastTier = UIScaler.CurrentTier;
                _pluginContainer.RecalculateScaling();
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

            //TEMPORARY:
            if (_isInCombat)
            {
                _lastCombatLength = (int)(DateTime.Now - _turnStartTime).TotalSeconds;
            }
            else
            {
                _lastShopLength = (int)(DateTime.Now - _turnStartTime).TotalSeconds;
            }

            if (_isTracking && !_isInCombat)
            {
                _pluginContainer.DisplayControl.SetApm(CalculateApm());
            }
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
            if (Core.Game.CurrentGameMode != GameMode.Battlegrounds && Core.Game.CurrentGameMode != GameMode.Spectator)
                return;

            _isTracking = true;
            _isInCombat = false;
            _turnNumber = 0;
            _turnStartTime = DateTime.MinValue;
            _lastShopLength = 0;
            _lastCombatLength = 0;
            _currentTimeout = 0;
            ResetCounters();
            _pluginContainer.Visibility = Visibility.Visible;
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
            _pluginContainer.Visibility = Visibility.Collapsed;
        }

        private void TurnStart(ActivePlayer player)
        {
            if (player != ActivePlayer.Player)
            {
                return;
            }

            // Before resetting counters, capture the APT of the just‑finished turn
            int totalActions = 0;
            foreach (CounterKey key in Enum.GetValues(typeof(CounterKey)))
            {
                if (CounterDataProvider.Counters[key].IncludeInSummary)
                {
                    totalActions += _counterState[key];
                }
            }
            // Record highest APT if this turn beats the record
            if (totalActions > AppSettings.Default.HighestApt)
            {
                AppSettings.Default.HighestApt = totalActions;
                AppSettings.Default.Save();
            }

            ResetCounters(); // Resets ALL counters for the new turn
            _turnNumber++;
            UpdateDisplay();
        }

        private void OnCardPlayed(Card card)
        {
            if (!_isTracking)
                return;

            var counterKey = GetCounterKeyFromCardType(card.Type);
            if (counterKey.HasValue)
            {
                _counterState[counterKey.Value]++;
                UpdateDisplay();
            }

            var tagCounterKey = GetCounterKeyFromCardTag(card);
            if (tagCounterKey.HasValue)
            {
                _counterState[tagCounterKey.Value]++;
                UpdateDisplay();
            }
        }

        private void OnHeroPower()
        {
            if (!_isTracking)
                return;

            foreach (var kvp in CounterDataProvider.Counters)
            {
                if (kvp.Value.ParsingDetails is HeroPowerCounter)
                {
                    _counterState[kvp.Key]++;
                }
            }
        }

        private void ResetCounters()
        {
            foreach (var key in CounterDataProvider.Counters.Keys)
            {
                _counterState[key] = 0;
            }
        }

        private void UpdateDisplay()
        {
            _pluginContainer.DisplayControl.SetCombatTime(_lastCombatLength);
            _pluginContainer.DisplayControl.SetShopTime(_lastShopLength);

            _pluginContainer.DisplayControl.UpdateCounters(_counterState);
            _pluginContainer.RefreshAchievements();
        }

        private double CalculateApm()
        {
            if (_turnStartTime == DateTime.MinValue || !_isTracking)
                return 0;

            var elapsed = DateTime.Now - _turnStartTime;
            if (elapsed.TotalSeconds < 1)
                return 0;

            int totalActions = 0;
            foreach (CounterKey key in Enum.GetValues(typeof(CounterKey)))
            {
                if (CounterDataProvider.Counters[key].IncludeInSummary)
                {
                    totalActions += _counterState[key];
                }
            }

            return Math.Round(totalActions * 60.0 / elapsed.TotalSeconds, 2);
        }
    }
}
