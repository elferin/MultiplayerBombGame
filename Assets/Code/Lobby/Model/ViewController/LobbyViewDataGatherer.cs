using System.Text;

namespace Code.Lobby {

    public class LobbyViewDataGatherer : IViewDataGatherer<LobbyViewData> {

        private const string CREATING_LOBBY = "Creating lobby...";
        private const string FAILED_TO_CREATE_LOBBY = "Failed to create lobby";
        private const string LEAVING_LOBBY = "Leaving lobby...";
        private const string FAILED_TO_START_MATCH = "Failed to start match";
        private const string STARTING_MATCH = "Starting match...";
        private const string WAITING_FOR_PLAYERS = "Waiting for players...";
        private const string WAITING_FOR_RETURN = "Waiting for players to return...";
        private const string REMOVED_FROM_LOBBY = "You were removed from the lobby";
        private const string LOBBY_CODE_FORMAT = "Code: {0}";
        private const string PLAYER_COUNT_FORMAT = "Players ({0}/{1})";

        private readonly ILobbyStatusGetter _statusGetter;
        private readonly ICurrentLobbyGetter _currentLobbyGetter;
        private readonly LobbyStartRules _startRules;

        public LobbyViewDataGatherer(ILobbyStatusGetter statusGetter, ICurrentLobbyGetter currentLobbyGetter,
            LobbyStartRules startRules) {
            _statusGetter = statusGetter;
            _currentLobbyGetter = currentLobbyGetter;
            _startRules = startRules;
        }

        public LobbyViewData Get() {
            var lobby = _currentLobbyGetter.GetCurrentLobby();
            if (!lobby.IsValid) {
                return new LobbyViewData(GetStatusText(), string.Empty, string.Empty, false, false, false);
            }

            return new LobbyViewData(GetStatusText(), string.Format(LOBBY_CODE_FORMAT, lobby.Code), GetPlayersText(lobby),
                _startRules.IsStartOffered(), _startRules.CanStart(), !string.IsNullOrEmpty(lobby.Code));
        }

        private string GetStatusText() {
            switch (_statusGetter.GetStatus()) {
                case LobbyStatus.Creating: return CREATING_LOBBY;
                case LobbyStatus.Failed: return FAILED_TO_CREATE_LOBBY;
                case LobbyStatus.Leaving: return LEAVING_LOBBY;
                case LobbyStatus.StartFailed: return FAILED_TO_START_MATCH;
                case LobbyStatus.Removed: return REMOVED_FROM_LOBBY;
                case LobbyStatus.Starting: return STARTING_MATCH;
                case LobbyStatus.Ready: return GetReadyText();
                default: return string.Empty;
            }
        }

        // Tells the host why Start is greyed out.
        private string GetReadyText() {
            if (!_startRules.IsStartOffered()) {
                return string.Empty;
            }
            if (!_startRules.HasEnoughPlayers()) {
                return WAITING_FOR_PLAYERS;
            }
            return _startRules.AreAllPlayersBack() ? string.Empty : WAITING_FOR_RETURN;
        }

        private string GetPlayersText(LobbyState lobby) {
            var players = new StringBuilder();
            players.Append(string.Format(PLAYER_COUNT_FORMAT, lobby.Players.Count, lobby.MaxPlayers));
            foreach (var player in lobby.Players) {
                players.Append('\n').Append(player.Name);
            }
            return players.ToString();
        }
    }
}
