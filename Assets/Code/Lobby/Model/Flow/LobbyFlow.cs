using System.Threading;
using Cysharp.Threading.Tasks;

namespace Code.Lobby {

    public class LobbyFlow : ILobbyFlow {

        private readonly LobbyFlowData _data;
        private readonly ILobbyViewControllerFactory _viewControllerFactory;
        private readonly ILobbyService _lobbyService;
        private readonly LobbyCreator _lobbyCreator;
        private readonly LobbyStartRules _startRules;
        private readonly MissingPlayerRemover _missingPlayerRemover;
        private readonly ILobbyStatus _status;
        private readonly ICurrentLobby _currentLobby;

        private ILobbyViewController _viewController;
        private UniTaskCompletionSource<LobbyFlowExit> _exitRequested;

        public LobbyFlow(LobbyFlowData data, ILobbyViewControllerFactory viewControllerFactory, ILobbyService lobbyService,
            LobbyCreator lobbyCreator, LobbyStartRules startRules, MissingPlayerRemover missingPlayerRemover, ILobbyStatus status, ICurrentLobby currentLobby) {
            _data = data;
            _viewControllerFactory = viewControllerFactory;
            _lobbyService = lobbyService;
            _lobbyCreator = lobbyCreator;
            _startRules = startRules;
            _missingPlayerRemover = missingPlayerRemover;
            _status = status;
            _currentLobby = currentLobby;
        }

        public async UniTask<LobbyFlowResult> RunAsync(CancellationToken cancellation) {
            var flowResult = new LobbyFlowResult();
            _viewController = _viewControllerFactory.CreateViewController();
            _exitRequested = new UniTaskCompletionSource<LobbyFlowExit>();
            var isInLobby = false;
            var removalCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellation);

            AddListeners();
            try {
                // Sets the screen's state before the view first shows it; creating continues while the view is open.
                var lobbyRequest = GetLobby(cancellation);
                var viewRun = _viewController.RunAsync(cancellation);

                isInLobby = await lobbyRequest;
                _viewController.Refresh();
                if (isInLobby && _data.ReturnsFromMatch && _currentLobby.GetCurrentLobby().IsOwner) {
                    // Stops when the flow ends, so nobody is removed once the host has left or started the next match.
                    _missingPlayerRemover.RemoveMissingPlayersAfterTimeout(removalCancellation.Token).Forget();
                }
                // The host may have started the next match while this player was still finishing the last one.
                CheckMatchStarted(_currentLobby.GetCurrentLobby());

                var exit = await _exitRequested.Task.AttachExternalCancellation(cancellation);
                if (exit == LobbyFlowExit.Leave) {
                    if (isInLobby) {
                        _status.SetStatus(LobbyStatus.Leaving);
                        _viewController.Refresh();
                        await _lobbyService.LeaveLobby();
                        isInLobby = false;
                    }
                }
                else {
                    // The match is played with this lobby, so it stays joined.
                    // The host waits for everyone to be back on this screen before the next match.
                    await _lobbyService.SetInLobby(false);
                    SetMatchResult(flowResult, exit);
                }

                _viewController.Close();
                await viewRun;
            }
            finally {
                removalCancellation.Cancel();
                removalCancellation.Dispose();
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
            if (_data.CreatesLobby) {
                return _lobbyCreator.TryCreateLobby(_data.MaxPlayers, _data.PlayerName, cancellation);
            }
            return GetJoinedLobby();
        }

        // Joined from the main menu or back from a match, so the lobby already exists.
        private async UniTask<bool> GetJoinedLobby() {
            var lobby = _lobbyService.CurrentLobby;
            _currentLobby.SetCurrentLobby(lobby);
            _status.SetStatus(GetJoinedStatus(lobby));

            if (!_data.ReturnsFromMatch || !lobby.IsValid) {
                return lobby.IsValid;
            }

            // A player who took too long to come back may have been removed by the host.
            if (!await _lobbyService.ReturnToLobby()) {
                _currentLobby.SetCurrentLobby(LobbyState.None);
                _status.SetStatus(LobbyStatus.Removed);
                return false;
            }
            // The host reopens the lobby so the next match can start.
            if (lobby.IsOwner) {
                await _lobbyService.EndMatch();
            }
            return true;
        }

        private LobbyStatus GetJoinedStatus(LobbyState lobby) {
            if (!lobby.IsValid) {
                return LobbyStatus.Failed;
            }
            return _data.MatchStartFailed ? LobbyStatus.StartFailed : LobbyStatus.Ready;
        }

        private void SetMatchResult(LobbyFlowResult flowResult, LobbyFlowExit exit) {
            var lobby = _currentLobby.GetCurrentLobby();
            flowResult.ShouldStartMatch = true;
            flowResult.IsHost = exit == LobbyFlowExit.StartAsHost;
            flowResult.MaxPlayers = lobby.MaxPlayers;
            flowResult.PlayerCount = lobby.Players.Count;
            flowResult.MatchJoinCode = flowResult.IsHost ? string.Empty : lobby.MatchJoinCode;
        }

        private void CheckMatchStarted(LobbyState lobby) {
            // The code of the match just played may still be in the lobby until the host clears it.
            if (!lobby.IsOwner && !string.IsNullOrEmpty(lobby.MatchJoinCode) && lobby.MatchJoinCode != _data.UsedJoinCode) {
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
            // The button can still be clicked in the moment a player leaves and the view hasn't refreshed yet.
            if (!_startRules.CanStart()) {
                return;
            }
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
