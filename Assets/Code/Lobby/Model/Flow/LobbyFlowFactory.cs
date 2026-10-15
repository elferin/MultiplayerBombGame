using JetBrains.Annotations;

using Code.Util;

namespace Code.Lobby {

    [UsedImplicitly]
    public class LobbyFlowFactory : ILobbyFlowFactory {

        private readonly ILobbyViewControllerFactory _viewControllerFactory;
        private readonly ILobbyService _lobbyService;
        private readonly LobbyCreator _lobbyCreator;
        private readonly LobbyJoiner _lobbyJoiner;
        private readonly LobbyStartRules _startRules;
        private readonly MissingPlayerRemover _missingPlayerRemover;
        private readonly IClipboard _clipboard;
        private readonly ILobbyStatus _status;
        private readonly ICurrentLobby _currentLobby;

        public LobbyFlowFactory(ILobbyViewControllerFactory viewControllerFactory, ILobbyService lobbyService,
            LobbyCreator lobbyCreator, LobbyJoiner lobbyJoiner, LobbyStartRules startRules, MissingPlayerRemover missingPlayerRemover,
            IClipboard clipboard, ILobbyStatus status, ICurrentLobby currentLobby) {
            _viewControllerFactory = viewControllerFactory;
            _lobbyService = lobbyService;
            _lobbyCreator = lobbyCreator;
            _lobbyJoiner = lobbyJoiner;
            _startRules = startRules;
            _missingPlayerRemover = missingPlayerRemover;
            _clipboard = clipboard;
            _status = status;
            _currentLobby = currentLobby;
        }

        public ILobbyFlow Create(LobbyFlowData data) {
            return new LobbyFlow(data, _viewControllerFactory, _lobbyService, _lobbyCreator, _lobbyJoiner, _startRules, _missingPlayerRemover,
                _clipboard, _status, _currentLobby);
        }
    }
}
