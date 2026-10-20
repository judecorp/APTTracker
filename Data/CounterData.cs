using HearthDb.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTTracker.Data
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
    }

    public enum MatchStrategy
    {
        EXACT,
        STARTS_WITH,
        IGNORE
    }

    public abstract class ParsingDetails { }

    public class LogParsingDetails : ParsingDetails
    {
        public string CardId { get; set; }
        public MatchStrategy MatchStrategy { get; set; } = MatchStrategy.EXACT;
        public string Zone { get; set; } = Constants.HAND_ZONE;
        public MatchStrategy ZoneMatchStrategy { get; set; } = MatchStrategy.IGNORE;
    }

    public class APIParsingDetails : ParsingDetails
    {
        public string CardType { get; set; }
    }

    public class TagParsingDetails : ParsingDetails
    {
        public GameTag GameTag { get; set; }
    }

    public class SumCounter : ParsingDetails
    {
        public List<CounterKey> CountersToSum { get; set; }
    }

    public class GameStateCounter : ParsingDetails
    {
    }

    public class HeroPowerCounter : ParsingDetails
    {
    }

    public class CounterData
    {
        public string DisplayName { get; set; }
        public int DefaultVisibility { get; set; } = 2; // 0 = hidden, 1 = hover, 2 = always
        public Color Color { get; set; }
        public bool IncludeInSummary { get; set; } = true;
        public ParsingDetails ParsingDetails { get; set; }
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
                        ParsingDetails = new APIParsingDetails { CardType = "Minion" }
                    } 
                },
                {
                    CounterKey.TAVERN_SPELLS,
                    new CounterData {
                        DisplayName = "Tavern Spells",
                        Color = Color.LightBlue,
                        ParsingDetails = new APIParsingDetails { CardType = "Battleground_Spell" }
                    }
                },
                {
                    CounterKey.NON_TAVERN_SPELLS,
                    new CounterData {
                        DisplayName = "Non-Tavern Spells",
                        Color = Color.LightYellow,
                        ParsingDetails = new APIParsingDetails { CardType = "Spell" }
                    }
                },
                {
                    CounterKey.BUY_MINION_COUNT,
                    new CounterData {
                        DisplayName = "Buy Minion",
                        Color = Color.LightCoral,
                        DefaultVisibility = 0,
                        IncludeInSummary = false,
                        ParsingDetails = new LogParsingDetails { CardId = "TB_BaconShop_DragBuy" }
                    }
                },
                {
                    CounterKey.BUY_SPELL_COUNT,
                    new CounterData {
                        DisplayName = "Buy Spell",
                        Color = Color.LightPink,
                        DefaultVisibility = 0,
                        IncludeInSummary = false,
                        ParsingDetails = new LogParsingDetails { CardId = "TB_BaconShop_DragBuy_Spell" }
                    }
                },
                {
                    CounterKey.SELL_MINION_COUNT,
                    new CounterData {
                        DisplayName = "Sell Minion",
                        Color = Color.LightSalmon,
                        ParsingDetails = new LogParsingDetails { CardId = "TB_BaconShop_DragSell" }
                    }
                },
                {
                    CounterKey.REFRESH_COUNT,
                    new CounterData {
                        DisplayName = "Refresh",
                        Color = Color.LightSkyBlue,
                        ParsingDetails = new LogParsingDetails { CardId = "TB_BaconShop_8p_Reroll_Button" }
                    }
                },
                {
                    CounterKey.FREEZE_COUNT,
                    new CounterData {
                        DisplayName = "Freeze",
                        Color = Color.LightSteelBlue,
                        ParsingDetails = new LogParsingDetails { CardId = "TB_BaconShopLockAll_Button" }
                    }
                },
                {
                    CounterKey.LEVELED_UP,
                    new CounterData {
                        DisplayName = "Leveled Up",
                        Color = Color.LightGoldenrodYellow,
                        ParsingDetails = new LogParsingDetails { CardId = "TB_BaconShopTechUp", MatchStrategy = MatchStrategy.STARTS_WITH }
                    }
                },
                {
                    CounterKey.BUY_CARD_COUNT,
                    new CounterData {
                        DisplayName = "Cards Bought",
                        Color = Color.LightGray,
                        ParsingDetails = new SumCounter { CountersToSum = new List<CounterKey> { CounterKey.BUY_MINION_COUNT, CounterKey.BUY_SPELL_COUNT } }
                    }
                },
                {
                    CounterKey.CHOOSE_ONE_COUNT,
                    new CounterData {
                        DisplayName = "Choose One",
                        Color = Color.DarkSeaGreen,
                        DefaultVisibility = 0,
                        ParsingDetails = new TagParsingDetails { GameTag = GameTag.CHOOSE_ONE }
                    }
                },
                {
                    CounterKey.DISCOVER_COUNT,
                    new CounterData {
                        DisplayName = "Discover",
                        Color = Color.Bisque,
                        DefaultVisibility = 0,
                        ParsingDetails = new GameStateCounter{ }
                    }
                },
                {
                    CounterKey.ACTIVATE_COUNT,
                    new CounterData {
                        DisplayName = "Activate",
                        Color = Color.Cornsilk,
                        DefaultVisibility = 0,
                        ParsingDetails = new LogParsingDetails{ CardId = "", MatchStrategy = MatchStrategy.IGNORE, Zone = Constants.PLAY_ZONE }
                    }
                },
                {
                    CounterKey.HERO_POWER,
                    new CounterData {
                        DisplayName = "Hero Power",
                        Color = Color.Gainsboro,
                        ParsingDetails = new HeroPowerCounter { }
                    }
                }
            };
    }
}
