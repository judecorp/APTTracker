using HearthDb.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace APTTracker.Counters
{
    public enum CounterEventKind
    {
        CardPlayed,
        LogPlay,
        DiscoverCompleted,
        HeroPowerUsed,
        HandDiscarded,
        ActivateUsed
    }

    /// <summary>Source handlers translate their input into these facts, without choosing counters.</summary>
    public sealed class CounterEvent
    {
        public CounterEventKind Kind { get; }
        public string CardId { get; }
        public string CardType { get; }
        public string SourceCardId { get; }
        public int? SubOption { get; }
        public IReadOnlyCollection<GameTag> Tags { get; }

        public CounterEvent(CounterEventKind kind, string cardId = null,
            string cardType = null, IEnumerable<GameTag> tags = null, int? subOption = null,
            string sourceCardId = null)
        {
            Kind = kind;
            CardId = cardId;
            CardType = cardType;
            SourceCardId = sourceCardId;
            SubOption = subOption;
            Tags = new List<GameTag>(tags ?? new GameTag[0]).AsReadOnly();
        }
    }

    /// <summary>All specified conditions must match. Unspecified conditions impose no restriction.</summary>
    public sealed class CounterRule
    {
        public CounterEventKind EventKind { get; set; }
        public string CardId { get; set; }
        public MatchStrategy CardIdMatch { get; set; } = MatchStrategy.EXACT;
        public string CardType { get; set; }
        public string SourceCardId { get; set; }
        public GameTag? RequiredTag { get; set; }
        public int? SubOption { get; set; }

        public bool Matches(CounterEvent counterEvent)
        {
            if (counterEvent.Kind != EventKind)
                return false;
            if (SourceCardId != null && SourceCardId != counterEvent.SourceCardId)
                return false;
            if (SubOption.HasValue && SubOption != counterEvent.SubOption)
                return false;
            if (CardType != null && CardType != counterEvent.CardType)
                return false;
            if (RequiredTag.HasValue && !counterEvent.Tags.Contains(RequiredTag.Value))
                return false;
            if (CardId == null)
                return true;

            switch (CardIdMatch)
            {
                case MatchStrategy.EXACT:
                    return string.Equals(counterEvent.CardId, CardId, StringComparison.Ordinal);
                case MatchStrategy.STARTS_WITH:
                    return counterEvent.CardId != null && counterEvent.CardId.StartsWith(CardId, StringComparison.Ordinal);
                default:
                    return false;
            }
        }
    }
}
