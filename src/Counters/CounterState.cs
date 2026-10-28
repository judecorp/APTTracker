// CounterState.cs
// Holds runtime mutable values for each counter defined in CounterData.
// This class is intentionally generic – it does not contain per‑counter code.
// It creates a dictionary keyed by CounterKey and populates it with a default
// value (0) for every entry found in CounterDataProvider.Counters.

using System.Collections.Generic;

namespace APTTracker.Counters
{
    /// <summary>
    /// Runtime state for all counters. Values are mutable and independent of the
    /// static compile‑time definitions in <see cref="CounterDataProvider"/>.
    /// </summary>
    public sealed class CounterState
    {
        // Holds the current integer value for each counter key.
        private readonly Dictionary<CounterKey, int> _values = new Dictionary<CounterKey, int>();

        /// <summary>
        /// Creates a new CounterState and initializes every counter with a value of 0.
        /// </summary>
        public CounterState()
        {
            // Initialise the dictionary using the static CounterData definitions.
            foreach (var kvp in CounterDataProvider.Counters)
            {
                _values[kvp.Key] = 0;
            }
        }

        /// <summary>
        /// Gets or sets the current value for the specified counter.
        /// </summary>
        public int this[CounterKey key]
        {
            get
            {
                if (CounterDataProvider.Counters[key].CountersToSum.Count != 0)
                {
                    int summedValue = 0;

                    foreach (var subKey in CounterDataProvider.Counters[key].CountersToSum)
                    {
                        summedValue += this[subKey];
                    }
                    return summedValue;
                }

                return _values[key];
            }
            set => _values[key] = value;
        }

        /// <summary>
        /// Returns the total of counters included in the action summary.
        /// </summary>
        public int TotalActions
        {
            get
            {
                int total = 0;
                foreach (var entry in CounterDataProvider.Counters)
                    if (entry.Value.IncludeInSummary)
                        total += this[entry.Key];
                return total;
            }
        }

        /// <summary>Returns current values, including calculated counters.</summary>
        public IReadOnlyDictionary<CounterKey, int> Snapshot()
        {
            var snapshot = new Dictionary<CounterKey, int>();
            foreach (var key in _values.Keys)
                snapshot[key] = this[key];
            return snapshot;
        }
    }
}
