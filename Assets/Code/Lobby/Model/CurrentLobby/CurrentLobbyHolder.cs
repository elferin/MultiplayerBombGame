using JetBrains.Annotations;

namespace Code.Lobby {

    // The lobby shown on the lobby screen.
    [UsedImplicitly]
    public class CurrentLobbyHolder : ICurrentLobby {

        private LobbyState _lobby;

        public CurrentLobbyHolder() {
            _lobby = LobbyState.None;
        }

        public void SetCurrentLobby(LobbyState lobby) {
            _lobby = lobby ?? LobbyState.None;
        }

        public LobbyState GetCurrentLobby() {
            return _lobby;
        }
    }
}
