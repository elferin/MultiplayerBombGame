using System;
using System.Threading;
using Code.FlowSystem;
using Code.MatchMenu;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.Match {

    // Starts the match network, spawns the players on the host, plays the game, and stops the network when it ends.
    // Esc opens the match menu over the running game; leaving through it ends the match for this player.
    public class MatchFlow : IMatchFlow {

        // Long enough for a leaving player's message to go out before their network stops.
        private static readonly TimeSpan END_ANNOUNCE_DELAY = TimeSpan.FromSeconds(0.5);

        private readonly MatchFlowData _data;
        private readonly IMatchViewControllerFactory _viewControllerFactory;
        private readonly IMatchNetwork _network;
        private readonly MatchPlayerSpawner _playerSpawner;
        private readonly IPlayerSpawning _spawning;
        private readonly IMatchArena _arena;
        private readonly ILocalPlayer _localPlayer;
        private readonly IMatchStatus _status;
        private readonly IFlowRunner _flowRunner;

        private IMatchViewController _viewController;
        private UniTaskCompletionSource _endRequested;
        private CancellationToken _matchCancellation;
        // Set while the menu is open; cancelling it closes the menu.
        private CancellationTokenSource _menuCancellation;
        private bool _isLeaving;

        public MatchFlow(MatchFlowData data, IMatchViewControllerFactory viewControllerFactory, IMatchNetwork network,
            MatchPlayerSpawner playerSpawner, IPlayerSpawning spawning, IMatchArena arena, ILocalPlayer localPlayer,
            IMatchStatus status, IFlowRunner flowRunner) {
            _data = data;
            _viewControllerFactory = viewControllerFactory;
            _network = network;
            _playerSpawner = playerSpawner;
            _spawning = spawning;
            _arena = arena;
            _localPlayer = localPlayer;
            _status = status;
            _flowRunner = flowRunner;
        }

        public async UniTask<MatchFlowResult> RunAsync(CancellationToken cancellation) {
            var flowResult = new MatchFlowResult();
            _viewController = _viewControllerFactory.CreateViewController();
            _endRequested = new UniTaskCompletionSource();
            _isLeaving = false;
            _status.SetStatus(MatchStatus.Starting);
            var isConnected = false;
            // Stops the game, the name setting and the menu when the match ends.
            var matchCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
            _matchCancellation = matchCancellation.Token;

            AddListeners();
            try {
                var viewRun = RunView(cancellation);

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
                    // On the host, too few players connecting in time ends the match; clients always play.
                    var hasEnoughPlayers = !_data.IsHost
                                           || await _playerSpawner.SpawnWhenAllJoined(_data.PlayerCount, _data.MinPlayers, cancellation);
                    if (hasEnoughPlayers) {
                        _status.SetStatus(MatchStatus.InProgress);
                        _viewController.Refresh();

                        await PlayGame(matchCancellation.Token, cancellation);
                        if (_isLeaving) {
                            await AnnounceLeaving(cancellation);
                        }
                    }
                    else {
                        flowResult.StartFailed = true;
                    }
                }
                else {
                    flowResult.StartFailed = true;
                }

                _viewController.Close();
                await viewRun;
            }
            // A failure (e.g. a Relay request throws) counts as a failed start. Returning the result instead of throwing
            // keeps the join code already received, so back in the lobby this match isn't mistaken for a new one.
            catch (Exception exception) when (!(exception is OperationCanceledException)) {
                Debug.LogException(exception);
                flowResult.StartFailed = true;
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

        // A view that fails to load only loses the status text, so the match goes on without it. Logged now rather than
        // when the match ends, and never thrown, so it can't turn a played match into a failed start.
        private async UniTask RunView(CancellationToken cancellation) {
            try {
                await _viewController.RunAsync(cancellation);
            }
            catch (Exception exception) when (!(exception is OperationCanceledException)) {
                Debug.LogException(exception);
            }
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

        // Through Relay nobody notices a player stopping until a timeout, so the others are told first.
        // The host's network carries the match, so the host leaving ends it for everyone; a client leaving is only removed.
        private async UniTask AnnounceLeaving(CancellationToken cancellation) {
            if (_data.IsHost) {
                _network.AnnounceMatchEnd();
            }
            else {
                _network.AnnounceLeaving();
            }
            await UniTask.Delay(END_ANNOUNCE_DELAY, DelayType.Realtime, cancellationToken: cancellation);
        }

        // Closed by a second Esc, by the match ending, or by a choice. Only Leave ends the match.
        private async UniTaskVoid RunMenu() {
            _menuCancellation = CancellationTokenSource.CreateLinkedTokenSource(_matchCancellation);
            try {
                var (isClosed, result) = await _flowRunner.Run(new MatchMenuFlowData(), _menuCancellation.Token)
                    .SuppressCancellationThrow();
                if (!isClosed && result.Choice == MatchMenuChoice.Leave) {
                    _isLeaving = true;
                    _endRequested.TrySetResult();
                }
            }
            // A menu that fails (e.g. its view doesn't load) only closes; the match goes on and Esc can try again.
            catch (Exception exception) {
                Debug.LogException(exception);
            }
            finally {
                _menuCancellation.Dispose();
                _menuCancellation = null;
            }
        }

        private void AddListeners() {
            _network.Disconnected += OnEndRequested;
            _network.HostEndedMatch += OnEndRequested;
            _viewController.MenuRequested += OnMenuRequested;
        }

        private void RemoveListeners() {
            _network.Disconnected -= OnEndRequested;
            _network.HostEndedMatch -= OnEndRequested;
            _viewController.MenuRequested -= OnMenuRequested;
        }

        private void OnEndRequested() {
            _endRequested.TrySetResult();
        }

        // Only while the game runs: there is nothing to leave while the match is still starting.
        private void OnMenuRequested() {
            if (_status.GetStatus() != MatchStatus.InProgress || _isLeaving) {
                return;
            }
            if (_menuCancellation != null) {
                _menuCancellation.Cancel();
                return;
            }
            RunMenu().Forget();
        }
    }
}
