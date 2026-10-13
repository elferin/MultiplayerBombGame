namespace Code.Lobby {

    // Everything LobbyView displays, already formatted.
    public class LobbyViewData {

        public string StatusText { get; }
        public string LobbyCodeText { get; }
        public string PlayersText { get; }
        public bool ShowsStartButton { get; }

        public LobbyViewData(string statusText, string lobbyCodeText, string playersText, bool showsStartButton) {
            StatusText = statusText;
            LobbyCodeText = lobbyCodeText;
            PlayersText = playersText;
            ShowsStartButton = showsStartButton;
        }
    }
}
