using System;
using System.Collections;
using System.Threading;
using Code.BombTag;
using Code.Tests.Powerup;
using Code.Tests.Util;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Code.Tests.BombTag {

    public class BombTagFlowTests {

        private const int TIMEOUT_MS = 5000;

        private BombTagStatusHolder _status;
        private FakeBombTagViewController _view;
        private FakeBombTagSync _sync;
        private FakeBombTagPlayers _players;
        private CancellationTokenSource _cancellation;

        [SetUp]
        public void SetUp() {
            _status = new BombTagStatusHolder();
            _view = new FakeBombTagViewController(_status);
            _sync = new FakeBombTagSync();
            _players = new FakeBombTagPlayers();
            _cancellation = new CancellationTokenSource(TIMEOUT_MS);

            _players.Add(1, "Ana", new Vector3(0f, 0f, 0f));
            _players.Add(2, "Luka", new Vector3(5f, 0f, 0f));
        }

        [TearDown]
        public void TearDown() {
            _cancellation.Cancel();
            _cancellation.Dispose();
        }

        [UnityTest]
        public IEnumerator ClientShowsWhatTheHostSendsAndEndsWithTheWinner() => UniTask.ToCoroutine(async () => {
            var run = StartFlow(BombTagFlowData.Client());
            Assert.AreEqual(1, _sync.StartListeningCount);

            _sync.Receive(new BombTagState(BombTagPhase.Countdown, 2, string.Empty));
            Assert.AreEqual(BombTagPhase.Countdown, _status.GetState().Phase);
            Assert.AreEqual(2, _view.RefreshedStates[^1].CountdownSeconds);
            Assert.AreEqual(0, _players.StopMovementCount);

            _sync.Receive(new BombTagState(BombTagPhase.Finished, 0, "Ana"));
            var result = await run;

            Assert.AreEqual("Ana", result.WinnerName);
            Assert.AreEqual(1, _players.StopMovementCount);
            Assert.IsTrue(_view.IsClosed);
            Assert.AreEqual(1, _sync.StopListeningCount);
            Assert.AreEqual(0, _sync.Sent.Count);
        });

        [UnityTest]
        public IEnumerator ClientKeepsWaitingUntilTheMatchIsFinished() => UniTask.ToCoroutine(async () => {
            var run = StartFlow(BombTagFlowData.Client());

            _sync.Receive(new BombTagState(BombTagPhase.Playing, 0, string.Empty));
            await UniTask.Yield();

            Assert.AreEqual(UniTaskStatus.Pending, run.Status);
            _sync.Receive(new BombTagState(BombTagPhase.Finished, 0, string.Empty));
            await run;
        });

        [UnityTest]
        public IEnumerator HostShowsAndSendsTheCountdownOnceEveryoneHasSpawned() => UniTask.ToCoroutine(async () => {
            var run = StartFlow(BombTagFlowData.Host(2));

            await UniTask.WaitUntil(() => _sync.Sent.Count > 0, cancellationToken: _cancellation.Token);

            Assert.AreEqual(BombTagPhase.Countdown, _sync.Sent[0].Phase);
            Assert.AreEqual(3, _sync.Sent[0].CountdownSeconds);
            // Shown directly too, so the host doesn't depend on receiving its own broadcast.
            Assert.AreEqual(BombTagPhase.Countdown, _status.GetState().Phase);

            _cancellation.Cancel();
            try {
                await run;
            }
            catch (OperationCanceledException) {
                // Expected: the round was still running.
            }
            Assert.AreEqual(1, _sync.StopListeningCount);
        });

        private UniTask<BombTagFlowResult> StartFlow(BombTagFlowData data) {
            var round = new BombTagRound(_players, new FakeRandom(), new FakePowerups());
            var flow = new BombTagFlow(data, _view, _sync, _players, _status, round, TimeSpan.Zero);
            return flow.RunAsync(_cancellation.Token).Preserve();
        }
    }
}
