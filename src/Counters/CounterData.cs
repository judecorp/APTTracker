using HearthDb.Enums;
using System.Collections.Generic;
using System.Drawing;

namespace APTTracker.Counters
{
    public enum CounterKey
    {
        UNITS_PLAYED,
        TAVERN_SPELLS,
        NON_TAVERN_SPELLS,
        BUY_MINION_COUNT,
        BUY_SPELL_COUNT,
        BUY_CARD_COUNT,
        SELL_MINION_COUNT,
        REFRESH_COUNT,
        FREEZE_COUNT,
        LEVELED_UP,
        CHOOSE_ONE_COUNT,
        DISCOVER_COUNT,
        ACTIVATE_COUNT,
        HERO_POWER,
        CHOOSE_ONE_MANUAL_COUNT,
        DARK_GIFT_COUNT,
        DISCARD_COUNT,
        DISCARD_AUTO_COUNT,
        DISCARD_MANUAL_COUNT,
    }

    public enum MatchStrategy
    {
        EXACT,
        STARTS_WITH,
        IGNORE
    }

    public class CounterData
    {
        public string DisplayName { get; set; }
        public int DefaultVisibility { get; set; } = 2; // 0 = hidden, 1 = hover, 2 = always
        public Color Color { get; set; }
        public bool IncludeInSummary { get; set; } = true;
        // Rules are alternatives; each rule combines its conditions with AND.
        public List<CounterRule> Rules { get; set; } = new List<CounterRule>();
        // Derived counters have no event rules and are evaluated when read.
        public List<CounterKey> CountersToSum { get; set; } = new List<CounterKey>();
        public List<CounterKey> CountersToSubtract { get; set; } = new List<CounterKey>();
        public bool IsDerived => CountersToSum.Count != 0 || CountersToSubtract.Count != 0;
    }

    public static class CounterDataProvider
    {
            public static readonly Dictionary<CounterKey, CounterData> Counters = new Dictionary<CounterKey, CounterData>
            {
                { 
                    CounterKey.UNITS_PLAYED,
                    new CounterData {
                        DisplayName = "Minions Played",
                        Color = Color.LightGreen,
                        Rules = { new CounterRule { EventKind = CounterEventKind.CardPlayed, CardType = "Minion" } }
                    } 
                },
                {
                    CounterKey.TAVERN_SPELLS,
                    new CounterData {
                        DisplayName = "Tavern Spells",
                        Color = Color.LightBlue,
                        Rules = { new CounterRule { EventKind = CounterEventKind.CardPlayed, CardType = "Battleground_Spell" } }
                    }
                },
                {
                    CounterKey.NON_TAVERN_SPELLS,
                    new CounterData {
                        DisplayName = "Non-Tavern Spells",
                        Color = Color.LightYellow,
                        Rules = { new CounterRule { EventKind = CounterEventKind.CardPlayed, CardType = "Spell" } }
                    }
                },
                {
                    CounterKey.BUY_MINION_COUNT,
                    new CounterData {
                        DisplayName = "Buy Minion",
                        Color = Color.LightCoral,
                        DefaultVisibility = 0,
                        IncludeInSummary = false,
                        Rules = { new CounterRule { EventKind = CounterEventKind.LogPlay, CardId = "TB_BaconShop_DragBuy" } }
                    }
                },
                {
                    CounterKey.BUY_SPELL_COUNT,
                    new CounterData {
                        DisplayName = "Buy Spell",
                        Color = Color.LightPink,
                        DefaultVisibility = 0,
                        IncludeInSummary = false,
                        Rules = { new CounterRule { EventKind = CounterEventKind.LogPlay, CardId = "TB_BaconShop_DragBuy_Spell" } }
                    }
                },
                {
                    CounterKey.SELL_MINION_COUNT,
                    new CounterData {
                        DisplayName = "Sell Minion",
                        Color = Color.LightSalmon,
                        Rules = { new CounterRule { EventKind = CounterEventKind.LogPlay, CardId = "TB_BaconShop_DragSell" } }
                    }
                },
                {
                    CounterKey.REFRESH_COUNT,
                    new CounterData {
                        DisplayName = "Refresh",
                        Color = Color.LightSkyBlue,
                        Rules = { new CounterRule { EventKind = CounterEventKind.LogPlay, CardId = "TB_BaconShop_8p_Reroll_Button" } }
                    }
                },
                {
                    CounterKey.FREEZE_COUNT,
                    new CounterData {
                        DisplayName = "Freeze",
                        Color = Color.LightSteelBlue,
                        Rules = { new CounterRule { EventKind = CounterEventKind.LogPlay, CardId = "TB_BaconShopLockAll_Button" } }
                    }
                },
                {
                    CounterKey.LEVELED_UP,
                    new CounterData {
                        DisplayName = "Leveled Up",
                        Color = Color.LightGoldenrodYellow,
                        Rules = { new CounterRule { EventKind = CounterEventKind.LogPlay, CardId = "TB_BaconShopTechUp", CardIdMatch = MatchStrategy.STARTS_WITH } }
                    }
                },
                {
                    CounterKey.BUY_CARD_COUNT,
                    new CounterData {
                        DisplayName = "Cards Bought",
                        Color = Color.LightGray,
                        CountersToSum = { CounterKey.BUY_MINION_COUNT, CounterKey.BUY_SPELL_COUNT }
                    }
                },
                {
                    CounterKey.CHOOSE_ONE_COUNT,
                    new CounterData {
                        DisplayName = "Choose One",
                        Color = Color.DarkSeaGreen,
                        DefaultVisibility = 0,
                        IncludeInSummary = false,
                        Rules = { new CounterRule { EventKind = CounterEventKind.CardPlayed, RequiredTag = GameTag.CHOOSE_ONE } }
                    }
                },
                {
                    CounterKey.CHOOSE_ONE_MANUAL_COUNT,
                    new CounterData {
                        DisplayName = "Choose One (Manual)",
                        Color = Color.DarkSeaGreen,
                        DefaultVisibility = 0,
                        Rules = {
                            new CounterRule { EventKind = CounterEventKind.LogPlay, SubOption = 0 },
                            new CounterRule { EventKind = CounterEventKind.LogPlay, SubOption = 1 }
                        }
                    }
                },
                {
                    CounterKey.DISCOVER_COUNT,
                    new CounterData {
                        DisplayName = "Discover",
                        Color = Color.Bisque,
                        DefaultVisibility = 0,
                        Rules = { new CounterRule { EventKind = CounterEventKind.DiscoverCompleted } }
                    }
                },
                {
                    CounterKey.DISCARD_COUNT,
                    new CounterData {
                        DisplayName = "Discard (All)",
                        Color = Color.LightSalmon,
                        DefaultVisibility = 0,
                        IncludeInSummary = false,
                        Rules = { new CounterRule { EventKind = CounterEventKind.HandDiscarded } }
                    }
                },
                {
                    CounterKey.DISCARD_AUTO_COUNT,
                    new CounterData {
                        DisplayName = "Discard (Auto)",
                        Color = Color.LightSalmon,
                        DefaultVisibility = 0,
                        IncludeInSummary = false,
                        Rules = {
                            // Willbreaker's paired spells discard through this enchantment.
                            new CounterRule { EventKind = CounterEventKind.HandDiscarded, SourceCardId = "BG36_307pe" },
                            new CounterRule { EventKind = CounterEventKind.HandDiscarded, SourceCardId = "BG36_320" },
                            new CounterRule { EventKind = CounterEventKind.HandDiscarded, SourceCardId = "BG36_320_G" }
                        }
                    }
                },
                {
                    CounterKey.DISCARD_MANUAL_COUNT,
                    new CounterData {
                        DisplayName = "Discard (Manual)",
                        Color = Color.LightSalmon,
                        DefaultVisibility = 0,
                        CountersToSum = { CounterKey.DISCARD_COUNT },
                        CountersToSubtract = { CounterKey.DISCARD_AUTO_COUNT }
                    }
                },
                {
                    CounterKey.ACTIVATE_COUNT,
                    new CounterData {
                        DisplayName = "Activate",
                        Color = Color.Cornsilk,
                        DefaultVisibility = 0,
                        // Activate is WIP: no event rules yet.
                    }
                },
                {
                    CounterKey.DARK_GIFT_COUNT,
                    new CounterData {
                        DisplayName = "Dark Gift",
                        Color = Color.MediumPurple,
                        DefaultVisibility = 0,
                        Rules = { new CounterRule { EventKind = CounterEventKind.LogPlay, CardId = "BG36_Button_DarkGift" } }
                    }
                },
                {
                    CounterKey.HERO_POWER,
                    new CounterData {
                        DisplayName = "Hero Power",
                        Color = Color.Gainsboro,
                        Rules = { new CounterRule { EventKind = CounterEventKind.HeroPowerUsed } }
                    }
                }
            };
    }
}
