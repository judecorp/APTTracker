using APTTracker.Counters;
using System;
using System.Diagnostics;

namespace APTTracker.Tracking
{
    internal sealed class TrackingSession
    {
        private readonly CounterEngine _engine;
        private readonly Stopwatch _phaseClock = new Stopwatch();
        private int _phaseTurn;

        public CounterState Counters { get; } = new CounterState();
        public bool IsInGame { get; private set; }
        public bool IsInCombat { get; private set; }
        public bool IsInShop => IsInGame && !IsInCombat;
        public int ShopSeconds { get; private set; }
        public int CombatSeconds { get; private set; }
        public double Apm { get; private set; }
        public int HighestApt { get; private set; }

        public TrackingSession()
        {
            _engine = new CounterEngine(Counters);
        }

        public void Start(int highestApt)
        {
            IsInGame = true;
            IsInCombat = false;
            HighestApt = highestApt;
            _phaseTurn = 0;
            _phaseClock.Restart();
            ShopSeconds = CombatSeconds = 0;
            Apm = 0;
            ResetCounters();
        }

        public void Stop()
        {
            UpdateTiming();
            IsInGame = false;
            IsInCombat = false;
            _phaseClock.Stop();
        }

        public bool Process(CounterEvent counterEvent)
        {
            return IsInShop && counterEvent != null && _engine.Process(counterEvent);
        }

        public bool ChangePhase(int turn)
        {
            if (!IsInGame || turn <= _phaseTurn)
                return false;

            _phaseTurn = turn;
            // Existing log convention: even GameEntity turns are combat, odd are shop.
            bool enteringCombat = turn % 2 == 0;
            if (enteringCombat == IsInCombat)
                return false;

            UpdateTiming();
            IsInCombat = enteringCombat;
            if (IsInCombat)
                CombatSeconds = 0;
            else
            {
                StartShop();
                ShopSeconds = 0;
                Apm = 0;
            }
            _phaseClock.Restart();
            return true;
        }

        private void StartShop()
        {
            HighestApt = Math.Max(HighestApt, Counters.TotalActions);
            ResetCounters();
        }

        public void UpdateTiming()
        {
            if (!IsInGame || !_phaseClock.IsRunning)
                return;
            double seconds = _phaseClock.Elapsed.TotalSeconds;
            if (IsInCombat)
                CombatSeconds = (int)seconds;
            else
            {
                ShopSeconds = (int)seconds;
                Apm = seconds < 1 ? 0 : Math.Round(Counters.TotalActions * 60.0 / seconds, 2);
            }
        }

        private void ResetCounters()
        {
            foreach (var key in CounterDataProvider.Counters.Keys)
                Counters[key] = 0;
        }
    }
}
