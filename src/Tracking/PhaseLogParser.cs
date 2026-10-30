using System.Text.RegularExpressions;

namespace APTTracker.Tracking
{
    internal static class PhaseLogParser
    {
        private static readonly Regex TurnChange = new Regex(
            @"TAG_CHANGE Entity=GameEntity tag=TURN value=(\d+)(?:\s|$)");
        private static readonly Regex MainAction = new Regex(
            @"TAG_CHANGE Entity=GameEntity tag=STEP value=MAIN_ACTION(?:\s|$)");

        public static bool IsMainAction(string line)
        {
            return !string.IsNullOrEmpty(line) && MainAction.IsMatch(line);
        }

        public static bool TryGetTurn(string line, out int turn)
        {
            turn = 0;
            if (string.IsNullOrEmpty(line))
                return false;
            var match = TurnChange.Match(line);
            return match.Success && int.TryParse(match.Groups[1].Value, out turn) && turn > 0;
        }
    }
}
