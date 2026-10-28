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
        public bool IsActive { get; private set; }
        public bool IsInCombat { get; private set; }
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
            IsActive = true;
            IsInCombat = false;
            HighestApt = highestApt;
            _phaseTurn = 0;
            _phaseClock.Reset();
            ShopSeconds = CombatSeconds = 0;
            Apm = 0;
            ResetCounters();
        }

        public void Stop()
        {
            UpdateTiming();
            IsActive = false;
            _phaseClock.Stop();
        }

        public bool Process(CounterEvent counterEvent)
        {
            return IsActive && counterEvent != null && _engine.Process(counterEvent);
        }

        public bool ChangePhase(int turn)
        {
            if (!IsActive || turn <= _phaseTurn)
                return false;

            UpdateTiming();
            _phaseTurn = turn;
            // Preserve the existing GameEntity TURN convention: even=combat, odd=shop.
            IsInCombat = turn % 2 == 0;
            if (IsInCombat)
                CombatSeconds = 0;
            else
            {
                ShopSeconds = 0;
                Apm = 0;
            }
            _phaseClock.Restart();
            return true;
        }

        public bool StartPlayerTurn()
        {
            if (!IsActive)
                return false;
            bool newRecord = Counters.TotalActions > HighestApt;
            if (newRecord)
                HighestApt = Counters.TotalActions;
            ResetCounters();
            Apm = 0;
            return newRecord;
        }

        public void UpdateTiming()
        {
            if (!IsActive || !_phaseClock.IsRunning)
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
