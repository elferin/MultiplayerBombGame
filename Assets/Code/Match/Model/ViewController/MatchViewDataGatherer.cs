namespace Code.Match {

    public class MatchViewDataGatherer : IViewDataGatherer<MatchViewData> {

        private const string MATCH_STARTING = "Match is starting...";

        private readonly IMatchStatusGetter _statusGetter;

        public MatchViewDataGatherer(IMatchStatusGetter statusGetter) {
            _statusGetter = statusGetter;
        }

        public MatchViewData Get() {
            var isInProgress = _statusGetter.GetStatus() == MatchStatus.InProgress;
            return new MatchViewData(GetStatusText(isInProgress));
        }

        private string GetStatusText(bool isInProgress) {
            return isInProgress ? string.Empty : MATCH_STARTING;
        }
    }
}
