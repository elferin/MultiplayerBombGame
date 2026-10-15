namespace Code.Lobby {
    public class LobbyFlowResult : UniTaskResult {

        // A match should start and the lobby stays joined. Otherwise the player left the lobby.
        public bool ShouldStartMatch { get; set; }
        // Set when the match should start.
        public bool IsHost { get; set; }
        public int MaxPlayers { get; set; }
        public int PlayerCount { get; set; }
        // Set when joining by code failed; the player goes back to the main menu's Join panel.
        public LobbyJoinFailure JoinFailure { get; set; }
    }
}
