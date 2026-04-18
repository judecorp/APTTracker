using HDTPlugin.Controls;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Plugins;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Card = Hearthstone_Deck_Tracker.Hearthstone.Card;
using Core = Hearthstone_Deck_Tracker.API.Core;

namespace HDTPlugin
{
    public class HDTBootstrap : IPlugin
    {
        private PlugInDisplayControl _displayControl;
        private bool _isTracking;

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

            Canvas.SetTop(_displayControl, 100);
            Canvas.SetLeft(_displayControl, 100);

            GameEvents.OnGameStart.Add(GameStart);
            GameEvents.OnGameEnd.Add(GameEnd);
            GameEvents.OnTurnStart.Add(TurnStart);
            GameEvents.OnPlayerPlay.Add(OnCardPlayed);
        }

        public void OnUnload()
        {
            if (_displayControl != null)
            {
                Core.OverlayCanvas.Children.Remove(_displayControl);
                _displayControl = null;
            }
        }

        public void OnUpdate() { }
        public void OnButtonPress() { }

        private void GameStart()
        {
            if (Core.Game.CurrentGameMode != GameMode.Battlegrounds)
                return;

            _isTracking = true;
            _turnNumber = 0;
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

            ResetCounters();
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
        }

        private void UpdateDisplay()
        {
            _displayControl.UpdateCounters(_turnNumber, _unitsPlayed, _tavernSpells, _nonTavernSpells);
        }
    }
}
