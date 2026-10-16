using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.BombTag {

    // One round of bomb tag. The host runs the rules and broadcasts the state; everyone shows it.
    public class BombTagFlow : IBombTagFlow {

        // A player object that never spawns mustn't hold up the others.
        private static readonly TimeSpan SPAWN_TIMEOUT = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan WINNER_SCREEN_TIME = TimeSpan.FromSeconds(3);

        private readonly BombTagFlowData _data;
        private readonly IBombTagViewControllerFactory _viewControllerFactory;
        private readonly IBombTagSync _sync;
        private readonly IBombTagPlayers _players;
        private readonly IBombTagStatus _status;
        private readonly BombTagRules _rules;

        private IBombTagViewController _viewController;
        private UniTaskCompletionSource _finished;

        public BombTagFlow(BombTagFlowData data, IBombTagViewControllerFactory viewControllerFactory, IBombTagSync sync,
            IBombTagPlayers players, IBombTagStatus status, BombTagRules rules) {
            _data = data;
            _viewControllerFactory = viewControllerFactory;
            _sync = sync;
            _players = players;
            _status = status;
            _rules = rules;
        }

        public async UniTask<BombTagFlowResult> RunAsync(CancellationToken cancellation) {
            var flowResult = new BombTagFlowResult();
            _viewController = _viewControllerFactory.CreateViewController();
            _finished = new UniTaskCompletionSource();
            _status.SetState(BombTagState.Waiting);

            AddListeners();
            try {
                var viewRun = _viewController.RunAsync(cancellation);

                if (_data.IsHost) {
                    await RunRules(cancellation);
                }
                else {
                    await _finished.Task.AttachExternalCancellation(cancellation);
                }
                flowResult.WinnerName = _status.GetState().WinnerName;

                await UniTask.Delay(WINNER_SCREEN_TIME, cancellationToken: cancellation);
                _viewController.Close();
                await viewRun;
            }
            finally {
                RemoveListeners();
                _viewController.Close();
            }

            return flowResult;
        }

        private async UniTask RunRules(CancellationToken cancellation) {
            await WaitForPlayers(cancellation);

            _rules.Begin();
            Publish(_rules.GetState());
            while (!_rules.IsFinished()) {
                await UniTask.Yield(PlayerLoopTiming.Update, cancellation);
                if (_rules.Tick(Time.deltaTime)) {
                    Publish(_rules.GetState());
                }
            }
        }

        private async UniTask WaitForPlayers(CancellationToken cancellation) {
            using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellation)) {
                timeout.CancelAfter(SPAWN_TIMEOUT);
                await UniTask.WaitUntil(() => _players.GetPlayers().Count >= _data.PlayerCount, cancellationToken: timeout.Token)
                    .SuppressCancellationThrow();
            }
            cancellation.ThrowIfCancellationRequested();
        }

        // The host's own client gets the broadcast too; showing it here as well means the host doesn't rely on that.
        private void Publish(BombTagState state) {
            ShowState(state);
            _sync.Send(state);
        }

        private void AddListeners() {
            _sync.StateReceived += ShowState;
            _sync.StartListening();
        }

        private void RemoveListeners() {
            _sync.StopListening();
            _sync.StateReceived -= ShowState;
        }

        private void ShowState(BombTagState state) {
            _status.SetState(state);
            _viewController.Refresh();
            if (state.Phase == BombTagPhase.Finished) {
                // Everyone stands still on the winner screen.
                _players.StopMovement();
                _finished.TrySetResult();
            }
        }
    }
}
