using HearthDb.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Card = Hearthstone_Deck_Tracker.Hearthstone.Card;

namespace APTTracker.Counters
{
    /// <summary>Source-specific detection belongs here; counter selection belongs in the rules.</summary>
    public static class CounterEventAdapter
    {
        public static CounterEvent FromPowerLog(string line)
        {
            if (!line.Contains("BLOCK_START BlockType=PLAY"))
                return null;

            var match = Regex.Match(line, @"cardId=(\S+)");
            return match.Success
                ? new CounterEvent(CounterEventKind.LogPlay, cardId: match.Groups[1].Value)
                : null;
        }

        public static CounterEvent FromGamestateLog(string line)
        {
            // Preserve the existing discover detection heuristic.
            return line.Contains("m_chosenEntities")
                ? new CounterEvent(CounterEventKind.DiscoverCompleted)
                : null;
        }

        public static CounterEvent FromCardPlayed(Card card)
        {
            // Read only tags used by configured rules, once per distinct tag.
            var requestedTags = CounterDataProvider.Counters.Values
                .SelectMany(counter => counter.Rules)
                .Where(rule => rule.EventKind == CounterEventKind.CardPlayed && rule.RequiredTag.HasValue)
                .Select(rule => rule.RequiredTag.Value)
                .Distinct();
            var tags = new List<GameTag>();
            foreach (var tag in requestedTags)
            {
                if (card.HasTag(tag))
                    tags.Add(tag);
            }

            return new CounterEvent(CounterEventKind.CardPlayed, cardType: card.Type, tags: tags);
        }
    }
}
