using System.Threading;
using Cysharp.Threading.Tasks;

namespace Code.Match {

    // Starts the match network, spawns the players on the host, and stops the network when the match ends.
    public class MatchFlow : IMatchFlow {

        private readonly MatchFlowArgs _args;
        private readonly IMatchViewControllerFactory _viewControllerFactory;
        private readonly IMatchNetwork _network;
        private readonly MatchPlayerSpawner _playerSpawner;
        private readonly IPlayerSpawning _spawning;
        private readonly IMatchStatus _status;

        private IMatchViewController _viewController;
        private UniTaskCompletionSource _endRequested;

        public MatchFlow(MatchFlowArgs args, IMatchViewControllerFactory viewControllerFactory, IMatchNetwork network,
            MatchPlayerSpawner playerSpawner, IPlayerSpawning spawning, IMatchStatus status) {
            _args = args;
            _viewControllerFactory = viewControllerFactory;
            _network = network;
            _playerSpawner = playerSpawner;
            _spawning = spawning;
            _status = status;
        }

        public async UniTask<MatchFlowResult> RunAsync(CancellationToken cancellation) {
            var flowResult = new MatchFlowResult();
            _viewController = _viewControllerFactory.CreateViewController();
            _endRequested = new UniTaskCompletionSource();
            _status.SetIsHost(_args.IsHost);
            _status.SetStatus(MatchStatus.Starting);
            var isConnected = false;

            AddListeners();
            try {
                var viewRun = _viewController.RunAsync(cancellation);

                flowResult.UsedJoinCode = _args.IsHost ? await StartHost(cancellation) : _args.JoinCode;
                isConnected = _args.IsHost
                    ? !string.IsNullOrEmpty(flowResult.UsedJoinCode)
                    : await _network.StartClient(_args.JoinCode, cancellation);

                if (isConnected) {
                    if (_args.IsHost) {
                        await _playerSpawner.SpawnWhenAllJoined(_args.PlayerCount, cancellation);
                    }
                    _status.SetStatus(MatchStatus.InProgress);
                    _viewController.Refresh();

                    // The host clicks End; clients lose their connection when the host stops the network.
                    await _endRequested.Task.AttachExternalCancellation(cancellation);
                }
                else {
                    flowResult.StartFailed = true;
                }

                _viewController.Close();
                await viewRun;
            }
            finally {
                RemoveListeners();
                _viewController.Close();

                if (isConnected) {
                    _network.Stop();
                    if (_args.IsHost) {
                        // The next match waits for everyone again before spawning.
                        _spawning.StopSpawning();
                    }
                }
            }

            return flowResult;
        }

        // Returns the join code, or an empty string if the network didn't start.
        private async UniTask<string> StartHost(CancellationToken cancellation) {
            var joinCode = await _network.StartHost(_args.MaxPlayers, cancellation);
            if (!string.IsNullOrEmpty(joinCode)) {
                // The other players start their clients when the code reaches them.
                await _args.ShareJoinCode(joinCode);
            }
            return joinCode;
        }

        private void AddListeners() {
            _viewController.EndClicked += OnEndRequested;
            _network.Disconnected += OnEndRequested;
        }

        private void RemoveListeners() {
            _viewController.EndClicked -= OnEndRequested;
            _network.Disconnected -= OnEndRequested;
        }

        private void OnEndRequested() {
            _endRequested.TrySetResult();
        }
    }
}
