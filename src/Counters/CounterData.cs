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
        SPELL_COUNT,
        CHOOSE_ONE_AUTO_COUNT,
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
        public int DefaultVisibility { get; set; } = 1; // 0 = hidden, 1 = hover, 2 = always
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
        // Light foregrounds for the #1A1A1A panels. Related counters share a hue;
        // automatic variants are deeper and manual variants lighter for clear separation.
        public static readonly Dictionary<CounterKey, CounterData> Counters = new Dictionary<CounterKey, CounterData>
            {
                {
                    CounterKey.BUY_CARD_COUNT,
                    new CounterData {
                        DisplayName = "Buy Card",
                        Color = Color.FromArgb(0x93, 0xDB, 0xA5), // #93DBA5 leaf green
                        CountersToSum = { CounterKey.BUY_MINION_COUNT, CounterKey.BUY_SPELL_COUNT }
                    }
                },
                {
                    CounterKey.BUY_MINION_COUNT,
                    new CounterData {
                        DisplayName = "Buy Minion",
                        Color = Color.FromArgb(0x56, 0xBA, 0x78), // #56BA78 deeper green
                        DefaultVisibility = 0,
                        IncludeInSummary = false,
                        Rules = { new CounterRule { EventKind = CounterEventKind.LogPlay, CardId = "TB_BaconShop_DragBuy" } }
                    }
                },
                {
                    CounterKey.BUY_SPELL_COUNT,
                    new CounterData {
                        DisplayName = "Buy Spell",
                        Color = Color.FromArgb(0xC4, 0xF3, 0xD0), // #C4F3D0 light green
                        DefaultVisibility = 0,
                        IncludeInSummary = false,
                        Rules = { new CounterRule { EventKind = CounterEventKind.LogPlay, CardId = "TB_BaconShop_DragBuy_Spell" } }
                    }
                },
                {
                    CounterKey.UNITS_PLAYED,
                    new CounterData {
                        DisplayName = "Play Minion",
                        Color = Color.FromArgb(0xFF, 0xC6, 0x81), // #FFC681 apricot
                        Rules = { new CounterRule { EventKind = CounterEventKind.CardPlayed, CardType = "Minion" } }
                    }
                },
                {
                    CounterKey.SELL_MINION_COUNT,
                    new CounterData {
                        DisplayName = "Sell Minion",
                        Color = Color.FromArgb(0xEE, 0x82, 0x4B), // #EE824B deeper orange
                        Rules = { new CounterRule { EventKind = CounterEventKind.LogPlay, CardId = "TB_BaconShop_DragSell" } }
                    }
                },
                {
                    CounterKey.SPELL_COUNT,
                    new CounterData {
                        DisplayName = "Spell",
                        Color = Color.FromArgb(0x8D, 0xB9, 0xF4), // #8DB9F4 blue
                        DefaultVisibility = 1,
                        IncludeInSummary = false,
                        CountersToSum = { CounterKey.TAVERN_SPELLS, CounterKey.NON_TAVERN_SPELLS }
                    }
                },
                {
                    CounterKey.TAVERN_SPELLS,
                    new CounterData {
                        DisplayName = "Tavern Spell",
                        Color = Color.FromArgb(0x5C, 0x90, 0xE0), // #5C90E0 deeper blue
                        Rules = { new CounterRule { EventKind = CounterEventKind.CardPlayed, CardType = "Battleground_Spell" } }
                    }
                },
                {
                    CounterKey.NON_TAVERN_SPELLS,
                    new CounterData {
                        DisplayName = "Non-Tavern Spell",
                        Color = Color.FromArgb(0xC3, 0xDC, 0xFF), // #C3DCFF light blue
                        Rules = { new CounterRule { EventKind = CounterEventKind.CardPlayed, CardType = "Spell" } }
                    }
                },
                {
                    CounterKey.REFRESH_COUNT,
                    new CounterData {
                        DisplayName = "Refresh",
                        Color = Color.FromArgb(0x83, 0xD5, 0xC9), // #83D5C9 teal
                        Rules = { new CounterRule { EventKind = CounterEventKind.LogPlay, CardId = "TB_BaconShop_8p_Reroll_Button" } }
                    }
                },
                {
                    CounterKey.DISCOVER_COUNT,
                    new CounterData {
                        DisplayName = "Discover",
                        Color = Color.FromArgb(0xF3, 0xF5, 0x9B), // #F3F59B lemon yellow
                        Rules = { new CounterRule { EventKind = CounterEventKind.DiscoverCompleted } }
                    }
                },
                {
                    CounterKey.CHOOSE_ONE_COUNT,
                    new CounterData {
                        DisplayName = "Choose-one",
                        Color = Color.FromArgb(0xD4, 0xAD, 0x88), // #D4AD88 warm brown
                        IncludeInSummary = false,
                        Rules = { new CounterRule { EventKind = CounterEventKind.CardPlayed, RequiredTag = GameTag.CHOOSE_ONE } }
                    }
                },
                {
                    CounterKey.CHOOSE_ONE_AUTO_COUNT,
                    new CounterData {
                        DisplayName = "Choose-one (Auto)",
                        Color = Color.FromArgb(0xB8, 0x8B, 0x62), // #B88B62 deeper brown
                        DefaultVisibility = 0,
                        IncludeInSummary = false,
                        CountersToSum = { CounterKey.CHOOSE_ONE_COUNT },
                        CountersToSubtract = { CounterKey.CHOOSE_ONE_MANUAL_COUNT }
                    }
                },
                {
                    CounterKey.CHOOSE_ONE_MANUAL_COUNT,
                    new CounterData {
                        DisplayName = "Choose-one (Manual)",
                        Color = Color.FromArgb(0xF1, 0xD5, 0xB8), // #F1D5B8 light brown
                        DefaultVisibility = 0,
                        Rules = {
                            new CounterRule { EventKind = CounterEventKind.LogPlay, SubOption = 0 },
                            new CounterRule { EventKind = CounterEventKind.LogPlay, SubOption = 1 }
                        }
                    }
                },
                {
                    CounterKey.DISCARD_COUNT,
                    new CounterData {
                        DisplayName = "Discard",
                        Color = Color.FromArgb(0xC5, 0xA0, 0xEE), // #C5A0EE purple
                        IncludeInSummary = false,
                        Rules = { new CounterRule { EventKind = CounterEventKind.HandDiscarded } }
                    }
                },
                {
                    CounterKey.DISCARD_AUTO_COUNT,
                    new CounterData {
                        DisplayName = "Discard (Auto)",
                        Color = Color.FromArgb(0xA7, 0x78, 0xDF), // #A778DF deeper purple
                        DefaultVisibility = 0,
                        IncludeInSummary = false,
                        Rules = {
                            // Paired spells discard through an enchantment with this ID.
                            new CounterRule { EventKind = CounterEventKind.HandDiscarded, SourceCardId = "BG36_307pe" },
                            // Mysterious K'thir
                            new CounterRule { EventKind = CounterEventKind.HandDiscarded, SourceCardId = "BG36_320" },
                            new CounterRule { EventKind = CounterEventKind.HandDiscarded, SourceCardId = "BG36_320_G" }
                        }
                    }
                },
                {
                    CounterKey.DISCARD_MANUAL_COUNT,
                    new CounterData {
                        DisplayName = "Discard (Manual)",
                        Color = Color.FromArgb(0xE5, 0xD0, 0xFF), // #E5D0FF light purple
                        DefaultVisibility = 0,
                        CountersToSum = { CounterKey.DISCARD_COUNT },
                        CountersToSubtract = { CounterKey.DISCARD_AUTO_COUNT }
                    }
                },
                {
                    CounterKey.ACTIVATE_COUNT,
                    new CounterData {
                        DisplayName = "Activate",
                        Color = Color.FromArgb(0xFF, 0x8F, 0x83), // #FF8F83 bright coral
                        Rules = { new CounterRule { EventKind = CounterEventKind.ActivateUsed } }
                    }
                },
                {
                    CounterKey.LEVELED_UP,
                    new CounterData {
                        DisplayName = "Level Up",
                        Color = Color.FromArgb(0xF2, 0xCF, 0x75), // #F2CF75 gold
                        Rules = { new CounterRule { EventKind = CounterEventKind.LogPlay, CardId = "TB_BaconShopTechUp", CardIdMatch = MatchStrategy.STARTS_WITH } }
                    }
                },
                {
                    CounterKey.DARK_GIFT_COUNT,
                    new CounterData {
                        DisplayName = "Dark Gift",
                        Color = Color.FromArgb(0xE6, 0x53, 0x70), // #85001b deeper crimson
                        Rules = { new CounterRule { EventKind = CounterEventKind.LogPlay, CardId = "BG36_Button_DarkGift" } }
                    }
                },
                {
                    CounterKey.HERO_POWER,
                    new CounterData {
                        DisplayName = "Hero Power",
                        Color = Color.FromArgb(0xF0, 0xB7, 0xD6), // #F0B7D6 rose
                        Rules = { new CounterRule { EventKind = CounterEventKind.HeroPowerUsed } }
                    }
                },
                {
                    CounterKey.FREEZE_COUNT,
                    new CounterData {
                        DisplayName = "Freeze",
                        Color = Color.FromArgb(0xA2, 0xE2, 0xF5), // #A2E2F5 ice blue
                        Rules = { new CounterRule { EventKind = CounterEventKind.LogPlay, CardId = "TB_BaconShopLockAll_Button" } }
                    }
                },
            };
    }
}
