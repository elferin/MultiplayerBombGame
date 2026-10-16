namespace Code.BombTag {

    public class BombTagViewDataGatherer : IViewDataGatherer<BombTagViewData> {

        private const string WINNER_FORMAT = "{0} wins!";
        private const string NO_WINNER = "Nobody wins!";

        private readonly IBombTagStatusGetter _statusGetter;

        public BombTagViewDataGatherer(IBombTagStatusGetter statusGetter) {
            _statusGetter = statusGetter;
        }

        public BombTagViewData Get() {
            return new BombTagViewData(GetCenterText(_statusGetter.GetState()));
        }

        private string GetCenterText(BombTagState state) {
            switch (state.Phase) {
                case BombTagPhase.Countdown:
                    return state.CountdownSeconds.ToString();
                case BombTagPhase.Finished:
                    return string.IsNullOrEmpty(state.WinnerName) ? NO_WINNER : string.Format(WINNER_FORMAT, state.WinnerName);
                default:
                    return string.Empty;
            }
        }
    }
}
