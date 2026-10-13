using System.Threading;
using Code.Match;
using Cysharp.Threading.Tasks;

namespace Code.Lobby {

    public class LobbyFlow : ILobbyFlow {

        private readonly LobbyFlowArgs _args;
        private readonly ILobbyViewControllerFactory _viewControllerFactory;
        private readonly ILobbyService _lobbyService;
        private readonly IMatchNetwork _matchNetwork;
        private readonly LobbyCreator _lobbyCreator;
        private readonly ILobbyStatus _status;
        private readonly ICurrentLobby _currentLobby;

        private ILobbyViewController _viewController;
        private UniTaskCompletionSource<LobbyFlowExit> _exitRequested;

        public LobbyFlow(LobbyFlowArgs args, ILobbyViewControllerFactory viewControllerFactory, ILobbyService lobbyService,
            IMatchNetwork matchNetwork, LobbyCreator lobbyCreator, ILobbyStatus status, ICurrentLobby currentLobby) {
            _args = args;
            _viewControllerFactory = viewControllerFactory;
            _lobbyService = lobbyService;
            _matchNetwork = matchNetwork;
            _lobbyCreator = lobbyCreator;
            _status = status;
            _currentLobby = currentLobby;
        }

        public async UniTask<LobbyFlowResult> RunAsync(CancellationToken cancellation) {
            var flowResult = new LobbyFlowResult();
            _viewController = _viewControllerFactory.CreateViewController();
            _exitRequested = new UniTaskCompletionSource<LobbyFlowExit>();
            var isInLobby = false;

            AddListeners();
            try {
                // Sets the screen's state before the view first shows it; creating continues while the view is open.
                var lobbyRequest = GetLobby(cancellation);
                var viewRun = _viewController.RunAsync(cancellation);

                isInLobby = await lobbyRequest;
                _viewController.Refresh();

                while (!flowResult.ShouldStartMatch) {
                    var exit = await _exitRequested.Task.AttachExternalCancellation(cancellation);
                    if (exit == LobbyFlowExit.Leave) {
                        break;
                    }

                    flowResult.ShouldStartMatch = await StartMatch(exit, cancellation);
                    if (flowResult.ShouldStartMatch) {
                        var lobby = _currentLobby.GetCurrentLobby();
                        flowResult.IsHost = lobby.IsOwner;
                        flowResult.PlayerCount = lobby.PlayerNames.Count;
                    }
                    // A failed start keeps the player in the lobby to try again or leave.
                    _exitRequested = new UniTaskCompletionSource<LobbyFlowExit>();
                }

                // The match is played with this lobby, so it stays joined once started.
                if (!flowResult.ShouldStartMatch && isInLobby) {
                    _status.SetStatus(LobbyStatus.Leaving);
                    _viewController.Refresh();
                    await _lobbyService.LeaveLobby();
                    isInLobby = false;
                }

                _viewController.Close();
                await viewRun;
            }
            finally {
                RemoveListeners();
                _viewController.Close();

                // Cancelled while still in the lobby: leave it so it doesn't stay open on the server.
                if (isInLobby && !flowResult.ShouldStartMatch) {
                    _lobbyService.LeaveLobby().Forget();
                }
            }

            return flowResult;
        }

        private UniTask<bool> GetLobby(CancellationToken cancellation) {
            if (_args.CreatesLobby) {
                return _lobbyCreator.TryCreateLobby(_args.MaxPlayers, _args.PlayerName, cancellation);
            }

            // Joined from the main menu, so the lobby already exists.
            var lobby = _lobbyService.CurrentLobby;
            _currentLobby.SetCurrentLobby(lobby);
            _status.SetStatus(lobby.IsValid ? LobbyStatus.Ready : LobbyStatus.Failed);
            return UniTask.FromResult(lobby.IsValid);
        }

        private async UniTask<bool> StartMatch(LobbyFlowExit exit, CancellationToken cancellation) {
            _status.SetStatus(LobbyStatus.Starting);
            _viewController.Refresh();

            var lobby = _currentLobby.GetCurrentLobby();
            var isStarted = exit == LobbyFlowExit.StartAsHost
                ? await StartAsHost(lobby, cancellation)
                : await _matchNetwork.StartClient(lobby.MatchJoinCode, cancellation);

            if (!isStarted) {
                _status.SetStatus(LobbyStatus.StartFailed);
                _viewController.Refresh();
            }
            return isStarted;
        }

        private async UniTask<bool> StartAsHost(LobbyState lobby, CancellationToken cancellation) {
            var joinCode = await _matchNetwork.StartHost(lobby.MaxPlayers, cancellation);
            if (string.IsNullOrEmpty(joinCode)) {
                return false;
            }

            // The other players start their clients when this code reaches them through the lobby.
            await _lobbyService.StartMatch(joinCode);
            return true;
        }

        private void CheckMatchStarted(LobbyState lobby) {
            if (!lobby.IsOwner && !string.IsNullOrEmpty(lobby.MatchJoinCode)) {
                _exitRequested.TrySetResult(LobbyFlowExit.JoinStartedMatch);
            }
        }

        private void AddListeners() {
            _viewController.LeaveClicked += OnLeaveClicked;
            _viewController.StartClicked += OnStartClicked;
            _lobbyService.LobbyUpdated += OnLobbyUpdated;
        }

        private void RemoveListeners() {
            _viewController.LeaveClicked -= OnLeaveClicked;
            _viewController.StartClicked -= OnStartClicked;
            _lobbyService.LobbyUpdated -= OnLobbyUpdated;
        }

        private void OnLeaveClicked() {
            _exitRequested.TrySetResult(LobbyFlowExit.Leave);
        }

        private void OnStartClicked() {
            _exitRequested.TrySetResult(LobbyFlowExit.StartAsHost);
        }

        private void OnLobbyUpdated(LobbyState lobby) {
            if (!lobby.IsValid) {
                return;
            }
            _currentLobby.SetCurrentLobby(lobby);
            _viewController.Refresh();
            CheckMatchStarted(lobby);
        }
    }
}
