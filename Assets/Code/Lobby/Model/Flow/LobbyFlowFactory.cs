using JetBrains.Annotations;

namespace Code.Lobby {

    [UsedImplicitly]
    public class LobbyFlowFactory : ILobbyFlowFactory {

        private readonly ILobbyViewControllerFactory _viewControllerFactory;
        private readonly ILobbyService _lobbyService;
        private readonly LobbyCreator _lobbyCreator;
        private readonly LobbyStartRules _startRules;
        private readonly ILobbyStatus _status;
        private readonly ICurrentLobby _currentLobby;

        public LobbyFlowFactory(ILobbyViewControllerFactory viewControllerFactory, ILobbyService lobbyService,
            LobbyCreator lobbyCreator, LobbyStartRules startRules, ILobbyStatus status, ICurrentLobby currentLobby) {
            _viewControllerFactory = viewControllerFactory;
            _lobbyService = lobbyService;
            _lobbyCreator = lobbyCreator;
            _startRules = startRules;
            _status = status;
            _currentLobby = currentLobby;
        }

        public ILobbyFlow Create(LobbyFlowArgs args) {
            return new LobbyFlow(args, _viewControllerFactory, _lobbyService, _lobbyCreator, _startRules, _status, _currentLobby);
        }
    }
}
