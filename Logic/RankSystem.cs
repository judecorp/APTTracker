using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace HDTPlugin.Logic
{
    public class RankInfo
    {
        public string Label { get; set; }
        public string ImagePath { get; set; }
    }

    public static class RankSystem
    {
        private static readonly List<RankInfo> Ranks = new List<RankInfo>
        {
            new RankInfo { Label = "Asleep", ImagePath = "pack://application:,,,/img/rank_00.png" },
            new RankInfo { Label = "Barely Awake", ImagePath = "pack://application:,,,/img/rank_01.png" },
            new RankInfo { Label = "Waking Up", ImagePath = "pack://application:,,,/img/rank_02.png" },
            new RankInfo { Label = "Morning Coffee", ImagePath = "pack://application:,,,/img/rank_03.png" },
            new RankInfo { Label = "Awake", ImagePath = "pack://application:,,,/img/rank_04.png" },
            new RankInfo { Label = "Alert", ImagePath = "pack://application:,,,/img/rank_05.png" },
            new RankInfo { Label = "Focused", ImagePath = "pack://application:,,,/img/rank_06.png" },
            new RankInfo { Label = "Sharp", ImagePath = "pack://application:,,,/img/rank_07.png" },
            new RankInfo { Label = "In the Zone", ImagePath = "pack://application:,,,/img/rank_08.png" },
            new RankInfo { Label = "Peak Performance", ImagePath = "pack://application:,,,/img/rank_09.png" },
            new RankInfo { Label = "Unstoppable", ImagePath = "pack://application:,,,/img/rank_10.png" },
            new RankInfo { Label = "Mastermind", ImagePath = "pack://application:,,,/img/rank_11.png" },
            new RankInfo { Label = "Tactician", ImagePath = "pack://application:,,,/img/rank_12.png" },
            new RankInfo { Label = "Strategist", ImagePath = "pack://application:,,,/img/rank_13.png" },
            new RankInfo { Label = "Grandmaster", ImagePath = "pack://application:,,,/img/rank_14.png" },
            new RankInfo { Label = "Legendary", ImagePath = "pack://application:,,,/img/rank_15.png" },
        };

        public const int MaxRankIndex = 15;
        public const int ActionsPerRank = 10;
        public const int MaxActions = 150;

        public static int GetRankIndex(int totalActions)
        {
            if (totalActions <= 0) return 0;
            int index = (totalActions - 1) / ActionsPerRank;
            if (index > MaxRankIndex) index = MaxRankIndex;
            return index;
        }

        public static RankInfo GetRank(int totalActions)
        {
            int index = GetRankIndex(totalActions);
            return Ranks[index];
        }

        public static string GetRankLabel(int totalActions)
        {
            return GetRank(totalActions).Label;
        }

        public static BitmapImage GetRankImage(int totalActions)
        {
            try
            {
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new System.Uri(GetRank(totalActions).ImagePath, System.UriKind.Absolute);
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.EndInit();
                bi.Freeze();
                return bi;
            }
            catch
            {
                return null;
            }
        }
    }
}