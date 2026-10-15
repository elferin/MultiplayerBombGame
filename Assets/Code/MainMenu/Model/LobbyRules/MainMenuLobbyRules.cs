using Code.Lobby;
using Code.Profile;
using JetBrains.Annotations;

namespace Code.MainMenu {

    [UsedImplicitly]
    public class MainMenuLobbyRules : IMainMenuLobbyRules {

        private readonly IPlayerNameGetter _playerNameGetter;
        private readonly IHostLobbySettingsGetter _hostLobbySettingsGetter;
        private readonly IJoinLobbyGetter _joinLobbyGetter;

        public MainMenuLobbyRules(IPlayerNameGetter playerNameGetter,
            IHostLobbySettingsGetter hostLobbySettingsGetter,
            IJoinLobbyGetter joinLobbyGetter) {
            _playerNameGetter = playerNameGetter;
            _hostLobbySettingsGetter = hostLobbySettingsGetter;
            _joinLobbyGetter = joinLobbyGetter;
        }

        public bool CanCreateLobby() {
            return _playerNameGetter.HasPlayerName() && _hostLobbySettingsGetter.IsPlayerCountValid();
        }

        public bool CanJoinLobby() {
            return _playerNameGetter.HasPlayerName() && !string.IsNullOrWhiteSpace(_joinLobbyGetter.GetLobbyCode());
        }
    }
}
