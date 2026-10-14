namespace Code.Lobby {

    // A lobby member as the rest of the game sees it, independent of the lobby provider.
    public class LobbyPlayer {

        public string Id { get; }
        public string Name { get; }
        // On the lobby screen; false while the player is still in a match.
        public bool IsInLobby { get; }
        // The player on this device.
        public bool IsLocal { get; }

        public LobbyPlayer(string id, string name, bool isInLobby, bool isLocal) {
            Id = id;
            Name = name;
            IsInLobby = isInLobby;
            IsLocal = isLocal;
        }
    }
}
