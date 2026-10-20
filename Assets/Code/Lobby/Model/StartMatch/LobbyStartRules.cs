using JetBrains.Annotations;

namespace Code.Lobby {

    // When the host may start the match from the lobby.
    [UsedImplicitly]
    public class LobbyStartRules {

        private readonly ILobbyStatusGetter _statusGetter;
        private readonly ICurrentLobbyGetter _currentLobbyGetter;
        private readonly IHostLobbySettingsGetter _hostLobbySettings;

        public LobbyStartRules(ILobbyStatusGetter statusGetter, ICurrentLobbyGetter currentLobbyGetter,
            IHostLobbySettingsGetter hostLobbySettings) {
            _statusGetter = statusGetter;
            _currentLobbyGetter = currentLobbyGetter;
            _hostLobbySettings = hostLobbySettings;
        }

        // Only the host starts the match, and only while nothing else is in progress; a failed start can be retried.
        public bool IsStartOffered() {
            var lobby = _currentLobbyGetter.GetCurrentLobby();
            var status = _statusGetter.GetStatus();
            return lobby.IsValid && lobby.IsOwner && status is LobbyStatus.Ready or LobbyStatus.StartFailed;
        }

        public bool CanStart() {
            return IsStartOffered() && HasEnoughPlayers() && AreAllPlayersBack();
        }

        public bool HasEnoughPlayers() {
            return _currentLobbyGetter.GetCurrentLobby().Players.Count >= _hostLobbySettings.GetMinPlayers();
        }

        // Everyone has come back from the last match.
        public bool AreAllPlayersBack() {
            foreach (var player in _currentLobbyGetter.GetCurrentLobby().Players) {
                if (player.HasNotReturned) {
                    return false;
                }
            }
            return true;
        }
    }
}
