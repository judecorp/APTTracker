using APTTracker.Data;

namespace APTTracker.Logic
{
    /// <summary>One event can increment multiple counters, each at most once.</summary>
    public sealed class CounterEngine
    {
        private readonly CounterState _state;

        public CounterEngine(CounterState state)
        {
            _state = state;
        }

        public bool Process(CounterEvent counterEvent)
        {
            bool changed = false;
            foreach (var entry in CounterDataProvider.Counters)
            {
                var definition = entry.Value;
                if (definition.CountersToSum.Count != 0)
                    continue;

                foreach (var rule in definition.Rules)
                {
                    if (!rule.Matches(counterEvent))
                        continue;

                    _state[entry.Key]++;
                    changed = true;
                    break;
                }
            }
            return changed;
        }
    }
}
