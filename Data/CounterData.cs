using HearthDb.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDTPlugin.Data
{
    public static enum CounterKey
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
    }

    public abstract class ParsingDetails { }

    public class LogParsingDetails : ParsingDetails
    {
        public string CardId { get; set; }
    }

    public class APIParsingDetails : ParsingDetails
    {
        public string CardType { get; set; }
    }

    public class SumCounter : ParsingDetails
    {
        public List<CounterKey> CountersToSum { get; set; }
    }

    public class CounterData
    {
        public string DisplayName { get; set; }
        public bool DefaultVisibility { get; set; } = true;
        public Color Color { get; set; }
        public ParsingDetails ParsingDetails { get; set; }
    }

    public static class CounterDataProvider
    {
            public static readonly Dictionary<CounterKey, CounterData> Counters = new Dictionary<CounterKey, CounterData>
            {
                { 
                    CounterKey.UNITS_PLAYED,
                    new CounterData { DisplayName = "Minions Played",
                        Color = Color.LightGreen,
                        ParsingDetails = new HDTAPIDetails { CardType = "Minion" }
                    } 
                },
                {
                    CounterKey.TAVERN_SPELLS,
                    new CounterData {
                        DisplayName = "Tavern Spells",
                        Color = Color.LightBlue,
                        ParsingDetails = new HDTAPIDetails { CardType = "Battleground_Spell" }
                    }
                },
                {
                    CounterKey.NON_TAVERN_SPELLS,
                    new CounterData {
                        DisplayName = "Non-Tavern Spells",
                        Color = Color.LightYellow,
                        ParsingDetails = new HDTAPIDetails { CardType = "Spell" }
                    }
                },
                {
                    CounterKey.BUY_MINION_COUNT,
                    new CounterData {
                        DisplayName = "Buy Minion",
                        Color = Color.LightCoral,
                        DefaultVisibility = false,
                        ParsingDetails = new LogParsingDetails { CardId = "TB_BaconShop_DragBuy" }
                    }
                },
                {
                    CounterKey.BUY_SPELL_COUNT,
                    new CounterData {
                        DisplayName = "Buy Spell",
                        Color = Color.LightPink,
                        DefaultVisibility = false,
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
                        ParsingDetails = new LogParsingDetails { CardId = "TB_BaconShopTechUp" }
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
            };
    }
}
