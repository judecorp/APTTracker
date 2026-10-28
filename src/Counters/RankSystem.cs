using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using APTTracker.Controls;

namespace APTTracker.Counters
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
            new RankInfo { Label = "Sleeping Skeleton", ImagePath = "sleeping_skeleton.png" },
            new RankInfo { Label = "Newborn Baby", ImagePath = "newborn_baby.png" },
            new RankInfo { Label = "Special Friend", ImagePath = "special_friend.png" },
            new RankInfo { Label = "Bunny", ImagePath = "bunny.png" },
            new RankInfo { Label = "Tortoise", ImagePath = "tortoise.png" },
            new RankInfo { Label = "Eraserhead Baby", ImagePath = "eraserhead_baby.png" },
            new RankInfo { Label = "Garry Kasparov", ImagePath = "garry_kasparov.png" },
            new RankInfo { Label = "Dog", ImagePath = "dog.png" },
            new RankInfo { Label = "Blizzard Employee", ImagePath = "blizzard_employee.png" },
            new RankInfo { Label = "Jeefs", ImagePath = "jeefs.png" },
            new RankInfo { Label = "Bob", ImagePath = "bob.png" },
            new RankInfo { Label = "Giga Chad", ImagePath = "giga_chad.png" },
            new RankInfo { Label = "Hacker", ImagePath = "hacker.png" },
            new RankInfo { Label = "Jesus H. Christ", ImagePath = "jesus_h_christ.png" },

        };

        public const int MaxRankIndex = 13;
        public const int ActionsPerRank = 10;
        public const int MaxActions = 150;

        public static int GetRankIndex(int totalActions)
        {
            if (totalActions <= 0) return 0;
            int index = totalActions / ActionsPerRank;
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
                return new BitmapImage(new Uri($"pack://application:,,,/APTTracker;component/assets/images/{GetRank(totalActions).ImagePath}"));
            }
            catch(Exception)
            {
                return null;
            }
        }
    }
}