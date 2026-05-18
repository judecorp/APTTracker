namespace HDTPlugin.Logic
{
    using System.Linq;
    using System.Collections.Generic;
    using System.Windows.Controls;

    public enum CounterKey
    {
        TURN,
        UNITS_PLAYED,
        TAVERN_SPELLS,
        NON_TAVERN_SPELLS,
        BUY_MINION_COUNT,
        BUY_SPELL_COUNT,
        SELL_MINION_COUNT,
        REFRESH_COUNT,
        FREEZE_COUNT,
        LEVELED_UP,
        SHOP_LENGTH,
        COMBAT_LENGTH
    }

    public class CounterDefinition
    {
        public CounterKey Key { get; set; }
        public string DisplayName { get; set; }
        public int InitialValue { get; set; }
    }

    public class Counter
    {
        public CounterKey Key { get; set; }
        public string DisplayName { get; set; }
        public int Value { get; set; }
        public TextBlock Label { get; set; }
    }

    public class GameCounters
    {
        private readonly Dictionary<CounterKey, Counter> _counters = new Dictionary<CounterKey, Counter>();
        private readonly List<Counter> _orderedCounters = new List<Counter>();
        
        public static readonly List<CounterDefinition> Definitions = new List<CounterDefinition>
        {
            new CounterDefinition { Key = CounterKey.TURN, DisplayName = "Turn", InitialValue = 1 },
            new CounterDefinition { Key = CounterKey.UNITS_PLAYED, DisplayName = "Minions Played" },
            new CounterDefinition { Key = CounterKey.TAVERN_SPELLS, DisplayName = "Tavern Spells" },
            new CounterDefinition { Key = CounterKey.NON_TAVERN_SPELLS, DisplayName = "Non-Tavern Spells" },
            new CounterDefinition { Key = CounterKey.BUY_MINION_COUNT, DisplayName = "Buy Minion" },
            new CounterDefinition { Key = CounterKey.BUY_SPELL_COUNT, DisplayName = "Buy Spell" },
            new CounterDefinition { Key = CounterKey.SELL_MINION_COUNT, DisplayName = "Sell Minion" },
            new CounterDefinition { Key = CounterKey.REFRESH_COUNT, DisplayName = "Refresh" },
            new CounterDefinition { Key = CounterKey.FREEZE_COUNT, DisplayName = "Freeze" },
            new CounterDefinition { Key = CounterKey.LEVELED_UP, DisplayName = "Leveled Up" },
            new CounterDefinition { Key = CounterKey.SHOP_LENGTH, DisplayName = "Shop Length" },
            new CounterDefinition { Key = CounterKey.COMBAT_LENGTH, DisplayName = "Combat Length" }
        };

        public int Turn 
        { 
            get => _counters[CounterKey.TURN].Value; 
            set => _counters[CounterKey.TURN].Value = value; 
        }
        public int UnitsPlayed 
        { 
            get => _counters[CounterKey.UNITS_PLAYED].Value; 
            set => _counters[CounterKey.UNITS_PLAYED].Value = value; 
        }
        public int TavernSpells 
        { 
            get => _counters[CounterKey.TAVERN_SPELLS].Value; 
            set => _counters[CounterKey.TAVERN_SPELLS].Value = value; 
        }
        public int NonTavernSpells 
        { 
            get => _counters[CounterKey.NON_TAVERN_SPELLS].Value; 
            set => _counters[CounterKey.NON_TAVERN_SPELLS].Value = value; 
        }
        public int BuyMinionCount 
        { 
            get => _counters[CounterKey.BUY_MINION_COUNT].Value; 
            set => _counters[CounterKey.BUY_MINION_COUNT].Value = value; 
        }
        public int BuySpellCount 
        { 
            get => _counters[CounterKey.BUY_SPELL_COUNT].Value; 
            set => _counters[CounterKey.BUY_SPELL_COUNT].Value = value; 
        }
        public int SellMinionCount 
        { 
            get => _counters[CounterKey.SELL_MINION_COUNT].Value; 
            set => _counters[CounterKey.SELL_MINION_COUNT].Value = value; 
        }
        public int RefreshCount 
        { 
            get => _counters[CounterKey.REFRESH_COUNT].Value; 
            set => _counters[CounterKey.REFRESH_COUNT].Value = value; 
        }
        public int FreezeCount 
        { 
            get => _counters[CounterKey.FREEZE_COUNT].Value; 
            set => _counters[CounterKey.FREEZE_COUNT].Value = value; 
        }
        public int LeveledUp 
        { 
            get => _counters[CounterKey.LEVELED_UP].Value; 
            set => _counters[CounterKey.LEVELED_UP].Value = value; 
        }
        public int ShopLength 
        { 
            get => _counters[CounterKey.SHOP_LENGTH].Value; 
            set => _counters[CounterKey.SHOP_LENGTH].Value = value; 
        }
        public int CombatLength 
        { 
            get => _counters[CounterKey.COMBAT_LENGTH].Value; 
            set => _counters[CounterKey.COMBAT_LENGTH].Value = value; 
        }
        public double Apm { get; set; }

        public Dictionary<CounterKey, Counter> Counters => _counters;
        public List<Counter> OrderedCounters => _orderedCounters;

        public int this[CounterKey key]
        {
            get => _counters[key].Value;
            set => _counters[key].Value = value;
        }

        public static GameCounters Empty => new GameCounters();

        public void InitializeCounters()
        {
            foreach (var def in Definitions)
            {
                var counter = new Counter { Key = def.Key, Value = def.InitialValue, DisplayName = def.DisplayName };
                _counters[def.Key] = counter;
                _orderedCounters.Add(counter);
            }
        }

        public int TotalBought => BuyMinionCount + BuySpellCount;

        public int TotalActions => UnitsPlayed + TavernSpells + NonTavernSpells +
            TotalBought + SellMinionCount + RefreshCount + FreezeCount + LeveledUp;

        public void Reset()
        {
            foreach (var counter in _counters.Values)
            {
                var def = Definitions.First(d => d.Key == counter.Key);
                counter.Value = def.InitialValue;
            }
        }
    }
}