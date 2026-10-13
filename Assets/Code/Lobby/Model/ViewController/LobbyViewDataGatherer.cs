using System.Text;

namespace Code.Lobby {

    public class LobbyViewDataGatherer : IViewDataGatherer<LobbyViewData> {

        private const string CREATING_LOBBY = "Creating lobby...";
        private const string FAILED_TO_CREATE_LOBBY = "Failed to create lobby";
        private const string LEAVING_LOBBY = "Leaving lobby...";
        private const string STARTING_MATCH = "Starting match...";
        private const string FAILED_TO_START_MATCH = "Failed to start match";
        private const string LOBBY_CODE_FORMAT = "Code: {0}";
        private const string PLAYER_COUNT_FORMAT = "Players ({0}/{1})";

        private readonly ILobbyStatusGetter _statusGetter;
        private readonly ICurrentLobbyGetter _currentLobbyGetter;

        public LobbyViewDataGatherer(ILobbyStatusGetter statusGetter, ICurrentLobbyGetter currentLobbyGetter) {
            _statusGetter = statusGetter;
            _currentLobbyGetter = currentLobbyGetter;
        }

        public LobbyViewData Get() {
            var lobby = _currentLobbyGetter.GetCurrentLobby();
            if (!lobby.IsValid) {
                return new LobbyViewData(GetStatusText(), string.Empty, string.Empty, false);
            }

            return new LobbyViewData(GetStatusText(), string.Format(LOBBY_CODE_FORMAT, lobby.Code), GetPlayersText(lobby),
                ShowsStartButton(lobby));
        }

        private string GetStatusText() {
            switch (_statusGetter.GetStatus()) {
                case LobbyStatus.Creating: return CREATING_LOBBY;
                case LobbyStatus.Failed: return FAILED_TO_CREATE_LOBBY;
                case LobbyStatus.Leaving: return LEAVING_LOBBY;
                case LobbyStatus.Starting: return STARTING_MATCH;
                case LobbyStatus.StartFailed: return FAILED_TO_START_MATCH;
                default: return string.Empty;
            }
        }

        // Only the host starts the match, and only while nothing else is in progress; a failed start can be retried.
        private bool ShowsStartButton(LobbyState lobby) {
            var status = _statusGetter.GetStatus();
            return lobby.IsOwner && (status == LobbyStatus.Ready || status == LobbyStatus.StartFailed);
        }

        private string GetPlayersText(LobbyState lobby) {
            var players = new StringBuilder();
            players.Append(string.Format(PLAYER_COUNT_FORMAT, lobby.PlayerNames.Count, lobby.MaxPlayers));
            foreach (var playerName in lobby.PlayerNames) {
                players.Append('\n').Append(playerName);
            }
            return players.ToString();
        }
    }
}
