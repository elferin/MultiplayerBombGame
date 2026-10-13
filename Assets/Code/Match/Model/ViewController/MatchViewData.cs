namespace Code.Match {

    // Everything MatchView displays, already formatted.
    public class MatchViewData {

        public string StatusText { get; }
        public bool ShowsEndButton { get; }

        public MatchViewData(string statusText, bool showsEndButton) {
            StatusText = statusText;
            ShowsEndButton = showsEndButton;
        }
    }
}
