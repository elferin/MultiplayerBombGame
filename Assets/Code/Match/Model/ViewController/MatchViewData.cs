namespace Code.Match {

    // Everything MatchView displays, already formatted.
    public class MatchViewData {

        public string StatusText { get; }

        public MatchViewData(string statusText) {
            StatusText = statusText;
        }
    }
}
