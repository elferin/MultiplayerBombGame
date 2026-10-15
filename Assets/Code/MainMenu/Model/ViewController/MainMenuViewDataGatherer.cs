using Code.Lobby;
using JetBrains.Annotations;

namespace Code.MainMenu {

    [UsedImplicitly]
    public class MainMenuViewDataGatherer : IViewDataGatherer<MainMenuViewData> {

        private const string ENTER_NAME = "Enter a name";
        private const string ENTER_PLAYER_COUNT_FORMAT = "Enter a number from {0} to {1}";
        private const string PLAYER_RANGE_FORMAT = "{0}-{1} players";
        private const string JOIN_FAILED = "Couldn't join. Check the code.";
        private const string MATCH_IN_PROGRESS = "Match already in progress";
        private const string LOBBY_FULL = "Lobby is full";

        private readonly IMainMenuScreenLocationGetter _screenLocationGetter;
        private readonly IPlayerNameGetter _playerNameGetter;
        private readonly IHostLobbySettingsGetter _hostLobbySettingsGetter;
        private readonly IJoinLobbyGetter _joinLobbyGetter;
        private readonly IMainMenuLobbyRules _lobbyRules;

        public MainMenuViewDataGatherer(IMainMenuScreenLocationGetter screenLocationGetter,
            IPlayerNameGetter playerNameGetter,
            IHostLobbySettingsGetter hostLobbySettingsGetter,
            IJoinLobbyGetter joinLobbyGetter,
            IMainMenuLobbyRules lobbyRules) {
            _screenLocationGetter = screenLocationGetter;
            _playerNameGetter = playerNameGetter;
            _hostLobbySettingsGetter = hostLobbySettingsGetter;
            _joinLobbyGetter = joinLobbyGetter;
            _lobbyRules = lobbyRules;
        }

        public MainMenuViewData Get() {
            var currentScreen = _screenLocationGetter.GetCurrentScreen();
            return new MainMenuViewData(
                currentScreen == MainMenuScreen.MainMenu,
                currentScreen == MainMenuScreen.HostLobby,
                currentScreen == MainMenuScreen.JoinLobby,
                GetHostLobby(),
                GetJoinLobby());
        }

        private HostLobbyViewData GetHostLobby() {
            var hasName = _playerNameGetter.HasPlayerName();
            var isPlayerCountValid = _hostLobbySettingsGetter.IsPlayerCountValid();
            var minPlayers = _hostLobbySettingsGetter.GetMinPlayers();
            var maxPlayers = _hostLobbySettingsGetter.GetMaxPlayers();

            string hint;
            if (!hasName) {
                hint = ENTER_NAME;
            }
            else if (!isPlayerCountValid) {
                hint = string.Format(ENTER_PLAYER_COUNT_FORMAT, minPlayers, maxPlayers);
            }
            else {
                hint = string.Format(PLAYER_RANGE_FORMAT, minPlayers, maxPlayers);
            }

            return new HostLobbyViewData(
                _playerNameGetter.GetPlayerName(),
                _hostLobbySettingsGetter.GetPlayerCountText(),
                hint,
                _lobbyRules.CanCreateLobby());
        }

        private JoinLobbyViewData GetJoinLobby() {
            string status;
            switch (_joinLobbyGetter.GetJoinStatus()) {
                case JoinLobbyStatus.Failed: status = JOIN_FAILED; break;
                case JoinLobbyStatus.MatchInProgress: status = MATCH_IN_PROGRESS; break;
                case JoinLobbyStatus.LobbyFull: status = LOBBY_FULL; break;
                default: status = _playerNameGetter.HasPlayerName() ? string.Empty : ENTER_NAME; break;
            }

            return new JoinLobbyViewData(
                _playerNameGetter.GetPlayerName(),
                _joinLobbyGetter.GetLobbyCode(),
                status,
                _lobbyRules.CanJoinLobby());
        }
    }
}
