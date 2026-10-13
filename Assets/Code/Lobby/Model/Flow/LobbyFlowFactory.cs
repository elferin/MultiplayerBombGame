using Code.Match;

namespace Code.Lobby {

    public class LobbyFlowFactory : ILobbyFlowFactory {

        private readonly ILobbyViewControllerFactory _viewControllerFactory;
        private readonly ILobbyService _lobbyService;
        private readonly IMatchNetwork _matchNetwork;
        private readonly LobbyCreator _lobbyCreator;
        private readonly ILobbyStatus _status;
        private readonly ICurrentLobby _currentLobby;

        public LobbyFlowFactory(ILobbyViewControllerFactory viewControllerFactory, ILobbyService lobbyService,
            IMatchNetwork matchNetwork, LobbyCreator lobbyCreator, ILobbyStatus status, ICurrentLobby currentLobby) {
            _viewControllerFactory = viewControllerFactory;
            _lobbyService = lobbyService;
            _matchNetwork = matchNetwork;
            _lobbyCreator = lobbyCreator;
            _status = status;
            _currentLobby = currentLobby;
        }

        public ILobbyFlow Create(LobbyFlowArgs args) {
            return new LobbyFlow(args, _viewControllerFactory, _lobbyService, _matchNetwork, _lobbyCreator, _status, _currentLobby);
        }
    }
}
