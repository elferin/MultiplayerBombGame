namespace Code.Lobby {

    // Everything LobbyView displays, already formatted.
    public class LobbyViewData {

        public string StatusText { get; }
        public string LobbyCodeText { get; }
        public string PlayersText { get; }
        public bool ShowsStartButton { get; }
        // Shown but greyed out while the host waits for more players.
        public bool CanStartMatch { get; }

        public LobbyViewData(string statusText, string lobbyCodeText, string playersText, bool showsStartButton,
            bool canStartMatch) {
            StatusText = statusText;
            LobbyCodeText = lobbyCodeText;
            PlayersText = playersText;
            ShowsStartButton = showsStartButton;
            CanStartMatch = canStartMatch;
        }
    }
}
