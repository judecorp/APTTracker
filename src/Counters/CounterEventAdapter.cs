using HearthDb.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Card = Hearthstone_Deck_Tracker.Hearthstone.Card;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace APTTracker.Counters
{
    /// <summary>Source-specific detection belongs here; counter selection belongs in the rules.</summary>
    public static class CounterEventAdapter
    {
        public static CounterEvent FromHandDiscard(Entity discarded, Entity source)
        {
            // HDT also reports hand removals to SETASIDE (e.g. triples) as discards.
            // Read HDT's current zone; the entity description in a log line may be stale.
            if (discarded == null || discarded.GetTag(GameTag.ZONE) != (int)Zone.GRAVEYARD)
                return null;

            return new CounterEvent(CounterEventKind.HandDiscarded,
                cardId: discarded.CardId, sourceCardId: source?.CardId);
        }

        public static CounterEvent FromPowerLog(string line)
        {
            if (!line.Contains("BLOCK_START BlockType=PLAY"))
                return null;

            var match = Regex.Match(line, @"cardId=(\S+)");
            var subOptionMatch = Regex.Match(line, @"\bSubOption=(-?\d+)(?:\s|$)");
            int? subOption = subOptionMatch.Success && int.TryParse(subOptionMatch.Groups[1].Value, out int value)
                ? (int?)value : null;
            return match.Success
                ? new CounterEvent(CounterEventKind.LogPlay, cardId: match.Groups[1].Value, subOption: subOption)
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
