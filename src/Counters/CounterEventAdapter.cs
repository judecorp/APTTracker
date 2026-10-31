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
        private static readonly Regex PlaySource = new Regex(
            @"BLOCK_START BlockType=PLAY Entity=\[entityName=.*? id=(\d+) zone=(\w+) zonePos=\d+ cardId=(\S+) player=(\d+)\]");
        private static readonly Regex FirstChosenEntity = new Regex(
            @"GameState\.DebugPrintEntitiesChosen\(\) -\s+Entities\[0\]=\[.*\bplayer=(\d+)\](?:\s|$)");

        public static CounterEvent FromHandDiscard(Entity discarded, Entity source)
        {
            // HDT also reports hand removals to SETASIDE (e.g. triples) as discards.
            // Read HDT's current zone; the entity description in a log line may be stale.
            if (discarded == null || discarded.GetTag(GameTag.ZONE) != (int)Zone.GRAVEYARD)
                return null;

            return new CounterEvent(CounterEventKind.HandDiscarded,
                cardId: discarded.CardId, sourceCardId: source?.CardId);
        }

        public static CounterEvent FromPowerLog(string line,
            IReadOnlyDictionary<int, Entity> entities = null, int playerId = 0)
        {
            if (string.IsNullOrEmpty(line))
                return null;

            // Match the source explicitly: buy/sell blocks can target a minion in PLAY.
            var match = PlaySource.Match(line);
            if (!match.Success)
                return null;

            // The block records the origin zone before the action resolves. HDT supplies
            // live type/ownership; HAS_ACTIVATE_POWER also appears on non-Activate entities.
            if (match.Groups[2].Value == "PLAY" && playerId > 0 && entities != null
                && int.TryParse(match.Groups[1].Value, out int entityId)
                && int.TryParse(match.Groups[4].Value, out int controllerId)
                && controllerId == playerId
                && entities.TryGetValue(entityId, out var source)
                && source.IsMinion && source.IsControlledBy(playerId))
                return new CounterEvent(CounterEventKind.ActivateUsed, cardId: match.Groups[3].Value);

            var subOptionMatch = Regex.Match(line, @"\bSubOption=(-?\d+)(?:\s|$)");
            int? subOption = subOptionMatch.Success && int.TryParse(subOptionMatch.Groups[1].Value, out int value)
                ? (int?)value : null;
            return new CounterEvent(CounterEventKind.LogPlay, cardId: match.Groups[3].Value, subOption: subOption);
        }

        public static CounterEvent FromGamestateLog(string line, int playerId)
        {
            if (string.IsNullOrEmpty(line))
                return null;

            var match = FirstChosenEntity.Match(line);
            return playerId > 0 && match.Success
                && int.TryParse(match.Groups[1].Value, out int controllerId) && controllerId == playerId
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
