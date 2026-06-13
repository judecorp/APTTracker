// CounterState.cs
// Holds runtime mutable values for each counter defined in CounterData.
// This class is intentionally generic – it does not contain per‑counter code.
// It creates a dictionary keyed by CounterKey and populates it with a default
// value (0) for every entry found in CounterDataProvider.Counters.

using System.Collections.Generic;
using System.Drawing;
using APTTracker.Settings;

namespace APTTracker.Data
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
                if (CounterDataProvider.Counters[key].ParsingDetails is SumCounter)
                {
                    SumCounter sumCounter = (SumCounter)CounterDataProvider.Counters[key].ParsingDetails;
                    int summedValue = 0;

                    foreach (var subKey in sumCounter.CountersToSum)
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
        /// Returns a shallow copy of the internal dictionary for read‑only purposes.
        /// </summary>
        public IReadOnlyDictionary<CounterKey, int> Snapshot()
        {
            return new Dictionary<CounterKey, int>(_values);
        }

        /// <summary>
        /// Highest Actions‑Per‑Turn ever recorded (read‑only shortcut into persisted settings).
        /// </summary>
        public int HighestApt => AppSettings.Default.HighestApt;
    }
}
