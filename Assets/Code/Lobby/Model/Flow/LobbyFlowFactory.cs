namespace Code.Lobby {

    public class LobbyFlowFactory : ILobbyFlowFactory {

        private readonly ILobbyViewControllerFactory _viewControllerFactory;
        private readonly ILobbyService _lobbyService;
        private readonly LobbyCreator _lobbyCreator;
        private readonly ILobbyStatus _status;
        private readonly ICurrentLobby _currentLobby;

        public LobbyFlowFactory(ILobbyViewControllerFactory viewControllerFactory, ILobbyService lobbyService,
            LobbyCreator lobbyCreator, ILobbyStatus status, ICurrentLobby currentLobby) {
            _viewControllerFactory = viewControllerFactory;
            _lobbyService = lobbyService;
            _lobbyCreator = lobbyCreator;
            _status = status;
            _currentLobby = currentLobby;
        }

        public ILobbyFlow Create(LobbyFlowArgs args) {
            return new LobbyFlow(args, _viewControllerFactory, _lobbyService, _lobbyCreator, _status, _currentLobby);
        }
    }
}
