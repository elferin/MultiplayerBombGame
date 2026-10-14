using System.Threading;
using Cysharp.Threading.Tasks;

namespace Code.Match {

    // Starts the match network, spawns the players on the host, and stops the network when the match ends.
    public class MatchFlow : IMatchFlow {

        private readonly MatchFlowData _data;
        private readonly IMatchViewControllerFactory _viewControllerFactory;
        private readonly IMatchNetwork _network;
        private readonly MatchPlayerSpawner _playerSpawner;
        private readonly IPlayerSpawning _spawning;
        private readonly IMatchStatus _status;

        private IMatchViewController _viewController;
        private UniTaskCompletionSource _endRequested;

        public MatchFlow(MatchFlowData data, IMatchViewControllerFactory viewControllerFactory, IMatchNetwork network,
            MatchPlayerSpawner playerSpawner, IPlayerSpawning spawning, IMatchStatus status) {
            _data = data;
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
            _status.SetIsHost(_data.IsHost);
            _status.SetStatus(MatchStatus.Starting);
            var isConnected = false;

            AddListeners();
            try {
                var viewRun = _viewController.RunAsync(cancellation);

                // Clients show "Match is starting..." while the host sets up the network and shares the code.
                flowResult.UsedJoinCode = _data.IsHost ? await StartHost(cancellation) : await _data.GetJoinCode(cancellation);
                var hasJoinCode = !string.IsNullOrEmpty(flowResult.UsedJoinCode);
                isConnected = _data.IsHost
                    ? hasJoinCode
                    : hasJoinCode && await _network.StartClient(flowResult.UsedJoinCode, cancellation);

                if (isConnected) {
                    if (_data.IsHost) {
                        await _playerSpawner.SpawnWhenAllJoined(_data.PlayerCount, cancellation);
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
                    if (_data.IsHost) {
                        // The next match waits for everyone again before spawning.
                        _spawning.StopSpawning();
                    }
                }
            }

            return flowResult;
        }

        // Returns the join code, or an empty string if the network didn't start.
        private async UniTask<string> StartHost(CancellationToken cancellation) {
            var joinCode = await _network.StartHost(_data.MaxPlayers, cancellation);
            if (!string.IsNullOrEmpty(joinCode)) {
                // The other players start their clients when the code reaches them.
                await _data.ShareJoinCode(joinCode);
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
