namespace Code.Lobby {

    public class LobbyJoinResult {

        // LobbyState.None when joining failed.
        public LobbyState Lobby { get; }
        public LobbyJoinFailure Failure { get; }

        public LobbyJoinResult(LobbyState lobby, LobbyJoinFailure failure) {
            Lobby = lobby;
            Failure = failure;
        }
    }
}
