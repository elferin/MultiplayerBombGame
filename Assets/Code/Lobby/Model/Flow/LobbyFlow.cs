using System.Threading;
using Code.Util;
using Cysharp.Threading.Tasks;

namespace Code.Lobby {

    public class LobbyFlow : ILobbyFlow {

        private readonly LobbyFlowData _data;
        private readonly ILobbyViewControllerFactory _viewControllerFactory;
        private readonly ILobbyService _lobbyService;
        private readonly LobbyCreator _lobbyCreator;
        private readonly LobbyStartRules _startRules;
        private readonly MissingPlayerRemover _missingPlayerRemover;
        private readonly IClipboard _clipboard;
        private readonly ILobbyStatus _status;
        private readonly ICurrentLobby _currentLobby;

        private ILobbyViewController _viewController;
        private UniTaskCompletionSource<LobbyFlowExit> _exitRequested;

        public LobbyFlow(LobbyFlowData data, ILobbyViewControllerFactory viewControllerFactory, ILobbyService lobbyService,
            LobbyCreator lobbyCreator, LobbyStartRules startRules, MissingPlayerRemover missingPlayerRemover,
            IClipboard clipboard, ILobbyStatus status, ICurrentLobby currentLobby) {
            _data = data;
            _viewControllerFactory = viewControllerFactory;
            _lobbyService = lobbyService;
            _lobbyCreator = lobbyCreator;
            _startRules = startRules;
            _missingPlayerRemover = missingPlayerRemover;
            _clipboard = clipboard;
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
                isInLobby = await HandleExit(exit, isInLobby, flowResult);

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

        // Leaves the lobby, or gets ready for the match. Returns whether the player is still in the lobby.
        private async UniTask<bool> HandleExit(LobbyFlowExit exit, bool isInLobby, LobbyFlowResult flowResult) {
            if (exit != LobbyFlowExit.Leave) {
                if (exit == LobbyFlowExit.StartAsHost) {
                    // The others switch to the match screen while the host sets up the network.
                    await _lobbyService.AnnounceMatchStart();
                }
                // The match is played with this lobby, so it stays joined.
                // The host waits for everyone to be back on this screen before the next match.
                await _lobbyService.SetInLobby(false);
                SetMatchResult(flowResult, exit);
                return isInLobby;
            }
            if (isInLobby) {
                _status.SetStatus(LobbyStatus.Leaving);
                _viewController.Refresh();
                await _lobbyService.LeaveLobby();
            }
            return false;
        }

        private void SetMatchResult(LobbyFlowResult flowResult, LobbyFlowExit exit) {
            var lobby = _currentLobby.GetCurrentLobby();
            flowResult.ShouldStartMatch = true;
            flowResult.IsHost = exit == LobbyFlowExit.StartAsHost;
            flowResult.MaxPlayers = lobby.MaxPlayers;
            flowResult.PlayerCount = lobby.Players.Count;
            flowResult.MatchJoinCode = flowResult.IsHost ? string.Empty : lobby.MatchJoinCode;
        }

        // Clients go to the match screen as soon as the host taps Start; the join code follows there.
        private void CheckMatchStarted(LobbyState lobby) {
            if (!lobby.IsOwner && IsNewMatch(lobby)) {
                _exitRequested.TrySetResult(LobbyFlowExit.JoinStartedMatch);
            }
        }

        // The code of the match just played, and the starting flag next to it, may still be in the lobby
        // until the host clears them.
        private bool IsNewMatch(LobbyState lobby) {
            var hasNoCode = string.IsNullOrEmpty(lobby.MatchJoinCode);
            var hasNewCode = !hasNoCode && lobby.MatchJoinCode != _data.UsedJoinCode;
            return hasNewCode || (lobby.IsMatchStarting && hasNoCode);
        }

        private void AddListeners() {
            _viewController.LeaveClicked += OnLeaveClicked;
            _viewController.StartClicked += OnStartClicked;
            _viewController.CopyCodeClicked += OnCopyCodeClicked;
            _lobbyService.LobbyUpdated += OnLobbyUpdated;
        }

        private void RemoveListeners() {
            _viewController.LeaveClicked -= OnLeaveClicked;
            _viewController.StartClicked -= OnStartClicked;
            _viewController.CopyCodeClicked -= OnCopyCodeClicked;
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

        private void OnCopyCodeClicked() {
            var lobby = _currentLobby.GetCurrentLobby();
            if (lobby.IsValid) {
                _clipboard.SetText(lobby.Code);
            }
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
