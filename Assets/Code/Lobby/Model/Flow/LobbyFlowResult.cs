namespace Code.Lobby {
    public class LobbyFlowResult : UniTaskResult {

        // The host started the match and the lobby is still joined. Otherwise the player left the lobby.
        public bool ShouldStartMatch { get; set; }
        // Set when the match started.
        public bool IsHost { get; set; }
        public int PlayerCount { get; set; }
    }
}
