using JetBrains.Annotations;

namespace Code.Lobby {

    [UsedImplicitly]
    public class LobbyFlowFactory : ILobbyFlowFactory {

        private readonly ILobbyViewControllerFactory _viewControllerFactory;
        private readonly ILobbyService _lobbyService;
        private readonly LobbyCreator _lobbyCreator;
        private readonly LobbyStartRules _startRules;
        private readonly MissingPlayerRemover _missingPlayerRemover;
        private readonly ILobbyStatus _status;
        private readonly ICurrentLobby _currentLobby;

        public LobbyFlowFactory(ILobbyViewControllerFactory viewControllerFactory, ILobbyService lobbyService,
            LobbyCreator lobbyCreator, LobbyStartRules startRules, MissingPlayerRemover missingPlayerRemover, ILobbyStatus status, ICurrentLobby currentLobby) {
            _viewControllerFactory = viewControllerFactory;
            _lobbyService = lobbyService;
            _lobbyCreator = lobbyCreator;
            _startRules = startRules;
            _missingPlayerRemover = missingPlayerRemover;
            _status = status;
            _currentLobby = currentLobby;
        }

        public ILobbyFlow Create(LobbyFlowData data) {
            return new LobbyFlow(data, _viewControllerFactory, _lobbyService, _lobbyCreator, _startRules, _missingPlayerRemover, _status, _currentLobby);
        }
    }
}
