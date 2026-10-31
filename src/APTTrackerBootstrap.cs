using APTTracker.Controls;
using APTTracker.Counters;
using APTTracker.Overlay;
using APTTracker.Settings;
using APTTracker.Tracking;
using APTTracker.Updates;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Plugins;
using System;
using System.Reflection;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Card = Hearthstone_Deck_Tracker.Hearthstone.Card;
using Core = Hearthstone_Deck_Tracker.API.Core;

namespace APTTracker
{
    public class APTTrackerBootstrap : IPlugin
    {
        private readonly TrackingSession _session = new TrackingSession();
        private PluginContainer _pluginContainer;
#if DEBUG
        private DebugWindow _debugWindow;
#endif
        private InputMoveManager _inputMoveManager;
        private bool _isLoaded;
        private bool _isShowingGame;
        private CancellationTokenSource _updateCheckCancellation;

        public string Author => "JudeCorp";
#if DEBUG
        public string ButtonText => "Open Debug Window";
#else
        public string ButtonText => null;
#endif
        public string Description => "Tracks actions per turn in Battlegrounds";
        public string Name => "APTTracker";
        public Version Version => new Version(Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion);
        public MenuItem MenuItem => null;

        public void OnLoad()
        {
            if (_isLoaded)
                return;

            CreateOverlay();
            _isLoaded = true;
            RegisterHostCallbacks();
            _pluginContainer.RefreshAchievements();
            RefreshCounters();
            _updateCheckCancellation = new CancellationTokenSource();
            _ = CheckForUpdatesAsync(_pluginContainer, _updateCheckCancellation.Token);
        }

        private async Task CheckForUpdatesAsync(PluginContainer container, CancellationToken cancellationToken)
        {
            try
            {
                using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) })
                {
                    var update = await new UpdateChecker(client).CheckAsync(Version, cancellationToken).ConfigureAwait(false);
                    if (update == null || cancellationToken.IsCancellationRequested)
                        return;

                    await container.Dispatcher.InvokeAsync(() =>
                    {
                        if (!cancellationToken.IsCancellationRequested)
                            container.SetAvailableUpdate(update);
                    });
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // The plugin was unloaded while the request was in progress.
            }
            catch (Exception ex)
            {
                Hearthstone_Deck_Tracker.Utility.Logging.Log.Warn(
                    $"[APTTracker] Could not check for updates. {ex.Message}");
            }
        }

        public void OnUnload()
        {
            _isLoaded = false;
            _updateCheckCancellation?.Cancel();
            _updateCheckCancellation?.Dispose();
            _updateCheckCancellation = null;
            _session.Stop();
#if DEBUG
            _debugWindow?.Close();
            _debugWindow = null;
#endif
            RemoveOverlay();
        }

        private void CreateOverlay()
        {
            _pluginContainer = new PluginContainer(_session.Counters) { Visibility = Visibility.Collapsed };
            Core.OverlayCanvas.Children.Add(_pluginContainer);
            Hearthstone_Deck_Tracker.Utility.Extensions.OverlayExtensions.SetIsOverlayHitTestVisible(_pluginContainer, true);
            PluginPosition.RestorePosition(_pluginContainer);
            _inputMoveManager = new InputMoveManager(_pluginContainer);
            _isShowingGame = false;
            _pluginContainer.SetMainMenu(true);
            UpdateOverlayState(Core.Game.CurrentMode);
        }

        private void UpdateOverlayState(Mode mode)
        {
            // Keep the final counters until HDT reports that gameplay has been left.
            if (_isShowingGame && !IsInGame && mode != Mode.GAMEPLAY)
            {
                _isShowingGame = false;
                _pluginContainer.SetMainMenu(true);
            }

            bool inBattlegrounds = mode == Mode.BACON
                || (mode == Mode.GAMEPLAY && (IsInGame || _isShowingGame || Core.Game.IsBattlegroundsMatch));
            var visibility = Core.Game.IsRunning && inBattlegrounds ? Visibility.Visible : Visibility.Collapsed;
            if (_pluginContainer.Visibility == visibility)
                return;

            _pluginContainer.Visibility = visibility;
            if (visibility == Visibility.Visible)
                _inputMoveManager.Enable();
            else
                _inputMoveManager.Disable();
        }

        private void RemoveOverlay()
        {
            _inputMoveManager?.Dispose();
            _inputMoveManager = null;
            if (_pluginContainer != null)
                Core.OverlayCanvas.Children.Remove(_pluginContainer);
            _pluginContainer = null;
        }

        // HDT identifies the owning plugin from Add's immediate caller type.
        // Keep registration in this class and preserve the call frame.
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private void RegisterHostCallbacks()
        {
            GameEvents.OnGameStart.Add(GameStart);
            GameEvents.OnGameEnd.Add(GameEnd);
            GameEvents.OnModeChanged.Add(OnModeChanged);
            GameEvents.OnInMenu.Add(OnInMenu);
            GameEvents.OnPlayerPlay.Add(OnCardPlayed);
            GameEvents.OnPlayerHandDiscard.Add(OnHandDiscard);
            GameEvents.OnPlayerHeroPower.Add(OnHeroPower);
            LogEvents.OnPowerLogLine.Add(OnPowerLogLine);
            LogEvents.OnPowerGamestateLogLine.Add(OnPowerGamestateLogLine);
        }

        private static bool IsSupportedGame()
        {
            var mode = Core.Game.CurrentGameMode;
            return mode == GameMode.Battlegrounds || mode == GameMode.Spectator;
        }

        private bool IsInGame => _isLoaded && _session.IsInGame;
        private bool IsInShop => _isLoaded && _session.IsInShop;

        private void ProcessCounterEvent(CounterEvent counterEvent)
        {
            if (IsInShop && _session.Process(counterEvent))
            {
                SaveHighestApt();
                RefreshCounters();
            }
        }

        private void OnPowerLogLine(string line)
        {
            if (!IsInGame || string.IsNullOrEmpty(line))
                return;

            if (PhaseLogParser.IsMainAction(line))
            {
                if (_session.BeginTracking())
                    RefreshTiming();
                return;
            }
            if (PhaseLogParser.TryGetTurn(line, out int turn))
            {
                if (_session.ChangePhase(turn) && _session.IsInShop)
                {
                    RefreshCounters();
                }
                return;
            }
            if (IsInShop)
                ProcessCounterEvent(CounterEventAdapter.FromPowerLog(line, Core.Game.Entities, Core.Game.Player.Id));
        }

        private void OnPowerGamestateLogLine(string line)
        {
            if (IsInShop && !string.IsNullOrEmpty(line))
                ProcessCounterEvent(CounterEventAdapter.FromGamestateLog(line, Core.Game.Player.Id));
        }

        public void OnUpdate()
        {
            if (!IsInGame)
                return;
            _session.UpdateTiming();
            RefreshTiming();
        }

        private void OnModeChanged(Mode mode)
        {
            if (_isLoaded)
                UpdateOverlayState(mode);
        }

        private void OnInMenu()
        {
            if (_isLoaded)
                UpdateOverlayState(Core.Game.CurrentMode);
        }

        public void OnButtonPress()
        {
#if DEBUG
            // The debug window uses mock counters and is available outside a game.
            if (!_isLoaded)
                return;
            if (_debugWindow == null || !_debugWindow.IsLoaded)
            {
                _debugWindow = new DebugWindow();
                _debugWindow.Closed += (s, e) => _debugWindow = null;
                _debugWindow.Show();
            }
            else
                _debugWindow.Activate();
#endif
        }

        private void GameStart()
        {
            if (!_isLoaded)
                return;
            _session.Stop();
            if (!IsSupportedGame())
            {
                UpdateOverlayState(Core.Game.CurrentMode);
                return;
            }
            _pluginContainer.SetMainMenu(false);
            _isShowingGame = true;
            _session.Start(AppSettings.Default.HighestApt);
            UpdateOverlayState(Core.Game.CurrentMode);
            PluginPosition.RestorePosition(_pluginContainer);
            RefreshCounters();
        }

        private void GameEnd()
        {
            if (!_isLoaded)
                return;
            _session.Stop();
            RefreshTiming();
#if DEBUG
            _debugWindow?.Close();
            _debugWindow = null;
#endif
            UpdateOverlayState(Core.Game.CurrentMode);
        }

        private void SaveHighestApt()
        {
            if (_session.HighestApt <= AppSettings.Default.HighestApt)
                return;
            AppSettings.Default.HighestApt = _session.HighestApt;
            AppSettings.Default.Save();
            _pluginContainer.RefreshAchievements();
        }

        private void OnCardPlayed(Card card)
        {
            if (IsInShop && card != null)
                ProcessCounterEvent(CounterEventAdapter.FromCardPlayed(card));
        }

        private void OnHeroPower()
        {
            ProcessCounterEvent(new CounterEvent(CounterEventKind.HeroPowerUsed));
        }

        private void OnHandDiscard(Card card)
        {
            if (!IsInShop || card == null)
                return;

            // HDT appends the entity before invoking OnPlayerHandDiscard(Card).
            var discarded = Core.Game.Player.EntitiesDiscardedFromHand.LastOrDefault();
            if (discarded == null)
                return;
            Core.Game.Entities.TryGetValue(discarded.GetTag(GameTag.LAST_AFFECTED_BY), out var source);
            var counterEvent = CounterEventAdapter.FromHandDiscard(discarded, source);
            if (counterEvent == null)
                return;

            ProcessCounterEvent(counterEvent);
        }

        private void RefreshCounters()
        {
            _pluginContainer.DisplayControl.UpdateCounters(_session.Counters, _session.HasNewAptRecord);
        }

        private void RefreshTiming()
        {
            var display = _pluginContainer.DisplayControl;
            display.SetCombatTime(_session.CombatSeconds);
            display.SetShopTime(_session.ShopSeconds);
            display.SetApm(_session.Apm);
        }

    }
}
