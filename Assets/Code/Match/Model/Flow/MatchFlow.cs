using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.Match {

    // Starts the match network, spawns the players on the host, plays the game, and stops the network when it ends.
    public class MatchFlow : IMatchFlow {

        private readonly MatchFlowData _data;
        private readonly IMatchViewControllerFactory _viewControllerFactory;
        private readonly IMatchNetwork _network;
        private readonly MatchPlayerSpawner _playerSpawner;
        private readonly IPlayerSpawning _spawning;
        private readonly IMatchArena _arena;
        private readonly ILocalPlayer _localPlayer;
        private readonly IMatchStatus _status;

        private IMatchViewController _viewController;
        private UniTaskCompletionSource _endRequested;

        public MatchFlow(MatchFlowData data, IMatchViewControllerFactory viewControllerFactory, IMatchNetwork network,
            MatchPlayerSpawner playerSpawner, IPlayerSpawning spawning, IMatchArena arena, ILocalPlayer localPlayer,
            IMatchStatus status) {
            _data = data;
            _viewControllerFactory = viewControllerFactory;
            _network = network;
            _playerSpawner = playerSpawner;
            _spawning = spawning;
            _arena = arena;
            _localPlayer = localPlayer;
            _status = status;
        }

        public async UniTask<MatchFlowResult> RunAsync(CancellationToken cancellation) {
            var flowResult = new MatchFlowResult();
            _viewController = _viewControllerFactory.CreateViewController();
            _endRequested = new UniTaskCompletionSource();
            _status.SetStatus(MatchStatus.Starting);
            var isConnected = false;
            // Stops the game and the name setting when the match ends.
            var matchCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellation);

            AddListeners();
            try {
                var viewRun = _viewController.RunAsync(cancellation);

                // Everyone loads the arena first: the players spawn on its spawn points, before the game's countdown.
                await _arena.Create(cancellation);

                // Clients show "Match is starting..." while the host sets up the network and shares the code.
                if (_data.IsHost) {
                    // An empty code means the network didn't start.
                    flowResult.UsedJoinCode = await _network.StartHost(_data.MaxPlayers, cancellation);
                    // Set before sharing, so the network is stopped even if sharing fails.
                    isConnected = !string.IsNullOrEmpty(flowResult.UsedJoinCode);
                    if (isConnected) {
                        // The other players start their clients when the code reaches them.
                        await _data.ShareJoinCode(flowResult.UsedJoinCode);
                    }
                }
                else {
                    flowResult.UsedJoinCode = await _data.GetJoinCode(cancellation);
                    isConnected = !string.IsNullOrEmpty(flowResult.UsedJoinCode)
                                  && await _network.StartClient(flowResult.UsedJoinCode, cancellation);
                }

                if (isConnected) {
                    _localPlayer.SetName(_data.PlayerName, matchCancellation.Token).SuppressCancellationThrow().Forget();
                    if (_data.IsHost) {
                        await _playerSpawner.SpawnWhenAllJoined(_data.PlayerCount, cancellation);
                    }
                    _status.SetStatus(MatchStatus.InProgress);
                    _viewController.Refresh();

                    await PlayGame(matchCancellation.Token, cancellation);
                }
                else {
                    flowResult.StartFailed = true;
                }

                _viewController.Close();
                await viewRun;
            }
            finally {
                matchCancellation.Cancel();
                matchCancellation.Dispose();
                RemoveListeners();
                _viewController.Close();

                if (isConnected) {
                    _network.Stop();
                    _spawning.RemoveSpawnedObjects();
                    if (_data.IsHost) {
                        // The next match waits for everyone again before spawning.
                        _spawning.StopSpawning();
                    }
                }
                _arena.Remove();
            }

            return flowResult;
        }

        // The game ends the match for everyone; a lost connection ends it early, e.g. when the host leaves.
        // Clients don't wait for the host to stop the network: through Relay they only notice that after a timeout.
        private async UniTask PlayGame(CancellationToken gameCancellation, CancellationToken cancellation) {
            var playerCount = _data.IsHost ? _network.GetConnectedPlayerCount() : 0;
            var game = RunGame(playerCount, gameCancellation);
            await UniTask.WhenAny(game, _endRequested.Task.AttachExternalCancellation(cancellation));
        }

        // A game that fails ends the match like one that finished, so everyone gets back to the lobby
        // instead of the error ending the whole game loop.
        private async UniTask RunGame(int playerCount, CancellationToken gameCancellation) {
            try {
                await _data.PlayGame(playerCount, gameCancellation);
            }
            catch (OperationCanceledException) {
                // The match ended some other way.
            }
            catch (Exception exception) {
                Debug.LogException(exception);
            }
        }

        private void AddListeners() {
            _network.Disconnected += OnEndRequested;
        }

        private void RemoveListeners() {
            _network.Disconnected -= OnEndRequested;
        }

        private void OnEndRequested() {
            _endRequested.TrySetResult();
        }
    }
}
