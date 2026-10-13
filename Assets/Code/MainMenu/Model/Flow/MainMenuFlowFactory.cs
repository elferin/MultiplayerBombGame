using Code.Lobby;

namespace Code.MainMenu {

    public class MainMenuFlowFactory : IMainMenuFlowFactory {

        private readonly IMainMenuViewControllerFactory _viewControllerFactory;
        private readonly LobbyJoiner _lobbyJoiner;
        private readonly IMainMenuScreenLocation _screenLocation;
        private readonly IPlayerName _playerName;
        private readonly IHostLobbySettings _hostLobbySettings;
        private readonly IJoinLobby _joinLobby;
        private readonly IMainMenuLobbyRules _lobbyRules;

        public MainMenuFlowFactory(IMainMenuViewControllerFactory viewControllerFactory, LobbyJoiner lobbyJoiner,
            IMainMenuScreenLocation screenLocation, IPlayerName playerName,
            IHostLobbySettings hostLobbySettings, IJoinLobby joinLobby, IMainMenuLobbyRules lobbyRules) {
            _viewControllerFactory = viewControllerFactory;
            _lobbyJoiner = lobbyJoiner;
            _screenLocation = screenLocation;
            _playerName = playerName;
            _hostLobbySettings = hostLobbySettings;
            _joinLobby = joinLobby;
            _lobbyRules = lobbyRules;
        }

        public IMainMenuFlow Create() {
            return new MainMenuFlow(_viewControllerFactory, _lobbyJoiner,
                _screenLocation, _playerName, _hostLobbySettings, _joinLobby, _lobbyRules);
        }
    }
}
