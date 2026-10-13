namespace Code.Lobby {

    // How LobbyFlow gets its lobby: create a new one, or show the one the player has already joined.
    public class LobbyFlowArgs {

        public bool CreatesLobby { get; }
        public int MaxPlayers { get; }
        public string PlayerName { get; }

        private LobbyFlowArgs(bool createsLobby, int maxPlayers, string playerName) {
            CreatesLobby = createsLobby;
            MaxPlayers = maxPlayers;
            PlayerName = playerName;
        }

        public static LobbyFlowArgs CreateLobby(int maxPlayers, string playerName) {
            return new LobbyFlowArgs(true, maxPlayers, playerName);
        }

        public static LobbyFlowArgs JoinedLobby() {
            return new LobbyFlowArgs(false, 0, string.Empty);
        }
    }
}
