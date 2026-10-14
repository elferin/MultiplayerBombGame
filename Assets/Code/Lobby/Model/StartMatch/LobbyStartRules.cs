using JetBrains.Annotations;

namespace Code.Lobby {

    // When the host may start the match from the lobby.
    [UsedImplicitly]
    public class LobbyStartRules {

        private const int MIN_PLAYERS_TO_START = 2;

        private readonly ILobbyStatusGetter _statusGetter;
        private readonly ICurrentLobbyGetter _currentLobbyGetter;

        public LobbyStartRules(ILobbyStatusGetter statusGetter, ICurrentLobbyGetter currentLobbyGetter) {
            _statusGetter = statusGetter;
            _currentLobbyGetter = currentLobbyGetter;
        }

        // Only the host starts the match, and only while nothing else is in progress; a failed start can be retried.
        public bool IsStartOffered() {
            var lobby = _currentLobbyGetter.GetCurrentLobby();
            var status = _statusGetter.GetStatus();
            return lobby.IsValid && lobby.IsOwner && status is LobbyStatus.Ready or LobbyStatus.StartFailed;
        }

        public bool CanStart() {
            return IsStartOffered() && HasEnoughPlayers();
        }

        public bool HasEnoughPlayers() {
            return _currentLobbyGetter.GetCurrentLobby().Players.Count >= MIN_PLAYERS_TO_START;
        }
    }
}
