namespace Code.Match {

    public class MatchViewDataGatherer : IViewDataGatherer<MatchViewData> {

        private const string MATCH_STARTING = "Match is starting...";

        private readonly IMatchStatusGetter _statusGetter;

        public MatchViewDataGatherer(IMatchStatusGetter statusGetter) {
            _statusGetter = statusGetter;
        }

        public MatchViewData Get() {
            var isInProgress = _statusGetter.GetStatus() == MatchStatus.InProgress;
            // Only the host ends the match. Placeholder until the game has rules that end it.
            return new MatchViewData(GetStatusText(isInProgress), isInProgress && _statusGetter.IsHost());
        }

        private string GetStatusText(bool isInProgress) {
            return isInProgress ? string.Empty : MATCH_STARTING;
        }
    }
}
