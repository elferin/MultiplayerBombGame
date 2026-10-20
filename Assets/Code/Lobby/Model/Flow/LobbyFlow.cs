using System.Threading;
using Code.Util;
using Cysharp.Threading.Tasks;

namespace Code.Lobby {

    public class LobbyFlow : ILobbyFlow {

        private readonly LobbyFlowData _data;
        private readonly ILobbyViewControllerFactory _viewControllerFactory;
        private readonly ILobbyService _lobbyService;
        private readonly LobbyCreator _lobbyCreator;
        private readonly LobbyJoiner _lobbyJoiner;
        private readonly LobbyStartRules _startRules;
        private readonly MissingPlayerRemover _missingPlayerRemover;
        private readonly IClipboard _clipboard;
        private readonly ILobbyStatus _status;
        private readonly ICurrentLobby _currentLobby;

        private ILobbyViewController _viewController;
        private UniTaskCompletionSource<LobbyFlowExit> _exitRequested;
        private LobbyJoinFailure _joinFailure;

        public LobbyFlow(LobbyFlowData data, ILobbyViewControllerFactory viewControllerFactory, ILobbyService lobbyService,
            LobbyCreator lobbyCreator, LobbyJoiner lobbyJoiner, LobbyStartRules startRules, MissingPlayerRemover missingPlayerRemover,
            IClipboard clipboard, ILobbyStatus status, ICurrentLobby currentLobby) {
            _data = data;
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

        public async UniTask<LobbyFlowResult> RunAsync(CancellationToken cancellation) {
            var flowResult = new LobbyFlowResult();
            _viewController = _viewControllerFactory.CreateViewController();
            _exitRequested = new UniTaskCompletionSource<LobbyFlowExit>();
            _joinFailure = LobbyJoinFailure.None;
            var isInLobby = false;
            var removalCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellation);

            AddListeners();
            try {
                // Sets the screen's state before the view first shows it; creating continues while the view is open.
                var lobbyRequest = GetLobby(cancellation);
                var viewRun = _viewController.RunAsync(cancellation);

                isInLobby = await lobbyRequest;
                if (_joinFailure != LobbyJoinFailure.None) {
                    // Back to the Join panel, where the player can fix the code.
                    flowResult.JoinFailure = _joinFailure;
                    _viewController.Close();
                    await viewRun;
                    return flowResult;
                }
                _viewController.Refresh();
                if (isInLobby && _data.Entry == LobbyEntry.ReturnFromMatch && _currentLobby.GetCurrentLobby().IsOwner) {
                    // Stops when the flow ends, so nobody is removed once the host has left or started the next match.
                    _missingPlayerRemover.RemoveMissingPlayersAfterTimeout(removalCancellation.Token).Forget();
                }
                // The host may have started the next match while this player was still finishing the last one.
                CheckMatchStarted(_currentLobby.GetCurrentLobby());

                var exit = await _exitRequested.Task.AttachExternalCancellation(cancellation);
                await HandleExit(exit, isInLobby, flowResult, cancellation);

                _viewController.Close();
                await viewRun;
            }
            finally {
                removalCancellation.Cancel();
                removalCancellation.Dispose();
                RemoveListeners();
                _viewController.Close();

                // Ended without starting a match while still in the lobby (cancelled, or a request threw): leave it,
                // so it doesn't stay open on the server. Asks the service, which knows even when a request failed midway.
                if (!flowResult.ShouldStartMatch && _lobbyService.CurrentLobby.IsValid) {
                    _lobbyService.LeaveLobby().Forget();
                }
            }

            return flowResult;
        }

        private UniTask<bool> GetLobby(CancellationToken cancellation) {
            switch (_data.Entry) {
                case LobbyEntry.Create:
                    return _lobbyCreator.TryCreateLobby(_data.MaxPlayers, _data.PlayerName, cancellation);
                case LobbyEntry.Join:
                    return JoinLobby(cancellation);
                default:
                    return GetReturnedLobby(cancellation);
            }
        }

        private async UniTask<bool> JoinLobby(CancellationToken cancellation) {
            _joinFailure = await _lobbyJoiner.TryJoinLobby(_data.LobbyCode, _data.PlayerName, cancellation);
            return _joinFailure == LobbyJoinFailure.None;
        }

        // Back from a match, so the lobby is still joined.
        private async UniTask<bool> GetReturnedLobby(CancellationToken cancellation) {
            var lobby = _lobbyService.CurrentLobby;
            _currentLobby.SetCurrentLobby(lobby);
            _status.SetStatus(GetReturnedStatus(lobby));

            if (!lobby.IsValid) {
                return false;
            }

            // A player who took too long to come back may have been removed by the host.
            if (!await _lobbyService.ReturnToLobby(cancellation)) {
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

        private LobbyStatus GetReturnedStatus(LobbyState lobby) {
            if (!lobby.IsValid) {
                return LobbyStatus.Closed;
            }
            return _data.MatchStartFailed ? LobbyStatus.StartFailed : LobbyStatus.Ready;
        }

        // Leaves the lobby, or gets ready for the match.
        private async UniTask HandleExit(LobbyFlowExit exit, bool isInLobby, LobbyFlowResult flowResult,
            CancellationToken cancellation) {
            if (exit != LobbyFlowExit.Leave) {
                if (exit == LobbyFlowExit.StartAsHost) {
                    // Hides Start and shows the tap worked while the requests below finish.
                    _status.SetStatus(LobbyStatus.Starting);
                    _viewController.Refresh();
                    // The others switch to the match screen while the host sets up the network.
                    await _lobbyService.AnnounceMatchStart();
                }
                // The match is played with this lobby, so it stays joined.
                // The host waits for everyone to be back on this screen before the next match.
                await _lobbyService.SetInLobby(false, cancellation);
                SetMatchResult(flowResult, exit);
                return;
            }
            if (isInLobby) {
                _status.SetStatus(LobbyStatus.Leaving);
                _viewController.Refresh();
                await _lobbyService.LeaveLobby();
            }
        }

        private void SetMatchResult(LobbyFlowResult flowResult, LobbyFlowExit exit) {
            var lobby = _currentLobby.GetCurrentLobby();
            flowResult.ShouldStartMatch = true;
            flowResult.IsHost = exit == LobbyFlowExit.StartAsHost;
            flowResult.MaxPlayers = lobby.MaxPlayers;
            flowResult.PlayerCount = lobby.Players.Count;
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
