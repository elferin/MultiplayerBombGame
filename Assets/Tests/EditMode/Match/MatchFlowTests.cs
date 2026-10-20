using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading;
using Code.FlowSystem;
using Code.Match;
using Code.MatchMenu;
using Code.Tests.FlowSystem;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Code.Tests.Match {

    public class MatchFlowTests {

        private const int TIMEOUT_MS = 5000;

        private MatchStatusHolder _status;
        private FakeMatchViewController _view;
        private FakeMatchNetwork _network;
        private FakePlayerSpawning _spawning;
        private FakeMatchArena _arena;
        private FakeLocalPlayer _localPlayer;
        private UniTaskCompletionSource _game;
        private int _gamePlayerCount;
        private CancellationTokenSource _cancellation;
        private string _sharedJoinCode;
        private FakeFlowInstantiator<MatchMenuFlowResult, MatchMenuFlowData> _menu;
        private FlowManager _flowRunner;

        [SetUp]
        public void SetUp() {
            _status = new MatchStatusHolder();
            _view = new FakeMatchViewController(_status);
            _network = new FakeMatchNetwork();
            _spawning = new FakePlayerSpawning();
            _arena = new FakeMatchArena(_network);
            _localPlayer = new FakeLocalPlayer();
            _game = new UniTaskCompletionSource();
            _gamePlayerCount = -1;
            _cancellation = new CancellationTokenSource(TIMEOUT_MS);
            _sharedJoinCode = string.Empty;
            _menu = new FakeFlowInstantiator<MatchMenuFlowResult, MatchMenuFlowData>();
            _flowRunner = new FlowManager();
            _flowRunner.Register(_menu);
        }

        [TearDown]
        public void TearDown() {
            _cancellation.Cancel();
            _cancellation.Dispose();
        }

        [UnityTest]
        public IEnumerator HostSharesJoinCodeSpawnsPlaysAndStopsWhenGameEnds() => UniTask.ToCoroutine(async () => {
            _network.ConnectedPlayerCount = 2;
            var run = StartFlow(HostData(4, 2));

            await UniTask.WaitUntil(() => _status.GetStatus() == MatchStatus.InProgress, cancellationToken: _cancellation.Token);
            Assert.AreEqual(4, _network.LastHostMaxPlayers);
            Assert.AreEqual("RELAY1", _sharedJoinCode);
            Assert.AreEqual(1, _spawning.StartCount);
            Assert.AreEqual(2, _gamePlayerCount);
            Assert.AreEqual("Ana", _localPlayer.LastName);
            Assert.AreEqual(0, _network.StopCount);

            _game.TrySetResult();
            var result = await run;

            Assert.IsFalse(result.StartFailed);
            Assert.AreEqual("RELAY1", result.UsedJoinCode);
            Assert.AreEqual(1, _network.StopCount);
            Assert.AreEqual(1, _spawning.StopCount);
            Assert.AreEqual(1, _spawning.RemoveCount);
            Assert.IsTrue(_view.IsClosed);
        });

        [UnityTest]
        public IEnumerator ArenaLoadsBeforeTheNetworkAndIsRemovedAtTheEnd() => UniTask.ToCoroutine(async () => {
            _network.ConnectedPlayerCount = 2;
            var run = StartFlow(HostData(4, 2));

            await UniTask.WaitUntil(() => _status.GetStatus() == MatchStatus.InProgress, cancellationToken: _cancellation.Token);
            Assert.AreEqual(1, _arena.CreateCount);
            Assert.IsFalse(_arena.WasNetworkStartedOnCreate);
            Assert.AreEqual(0, _arena.RemoveCount);

            _game.TrySetResult();
            await run;

            Assert.AreEqual(1, _arena.RemoveCount);
        });

        [UnityTest]
        public IEnumerator FailingGameEndsTheMatchInsteadOfThrowing() => UniTask.ToCoroutine(async () => {
            _network.ConnectedPlayerCount = 2;
            var run = StartFlow(HostData(4, 2));
            await UniTask.WaitUntil(() => _status.GetStatus() == MatchStatus.InProgress, cancellationToken: _cancellation.Token);

            LogAssert.Expect(LogType.Exception, new Regex("Game failed"));
            _game.TrySetException(new InvalidOperationException("Game failed"));
            var result = await run;

            Assert.IsFalse(result.StartFailed);
            Assert.AreEqual(1, _network.StopCount);
            Assert.AreEqual(1, _spawning.RemoveCount);
        });

        [UnityTest]
        public IEnumerator ViewThatFailsToLoadDoesNotFailAPlayedMatch() => UniTask.ToCoroutine(async () => {
            _view.FailsToLoad = true;
            _network.ConnectedPlayerCount = 2;

            LogAssert.Expect(LogType.Exception, new Regex("Match view failed to load"));
            var run = StartFlow(HostData(4, 2));
            await UniTask.WaitUntil(() => _status.GetStatus() == MatchStatus.InProgress, cancellationToken: _cancellation.Token);

            _game.TrySetResult();
            var result = await run;

            Assert.IsFalse(result.StartFailed);
            Assert.AreEqual(2, _gamePlayerCount);
            Assert.AreEqual(1, _network.StopCount);
        });

        [UnityTest]
        public IEnumerator FailedHostStartReturnsStartFailed() => UniTask.ToCoroutine(async () => {
            _network.HostJoinCode = string.Empty;

            var result = await StartFlow(HostData(4, 2));

            Assert.IsTrue(result.StartFailed);
            Assert.AreEqual(string.Empty, _sharedJoinCode);
            Assert.AreEqual(0, _spawning.StartCount);
            Assert.AreEqual(0, _network.StopCount);
            Assert.AreEqual(1, _arena.RemoveCount);
            Assert.IsTrue(_view.IsClosed);
        });

        [UnityTest]
        public IEnumerator ClientConnectsAndEndsWhenDisconnected() => UniTask.ToCoroutine(async () => {
            var run = StartFlow(ClientData("RELAY1"));

            Assert.AreEqual("RELAY1", _network.LastClientJoinCode);
            Assert.AreEqual(MatchStatus.InProgress, _status.GetStatus());
            Assert.AreEqual("Ana", _localPlayer.LastName);
            Assert.AreEqual(0, _network.StopCount);

            _network.RaiseDisconnected();
            var result = await run;

            Assert.IsFalse(result.StartFailed);
            Assert.AreEqual("RELAY1", result.UsedJoinCode);
            Assert.AreEqual(1, _network.StopCount);
            Assert.AreEqual(0, _spawning.StopCount);
            Assert.AreEqual(1, _spawning.RemoveCount);
            Assert.AreEqual(0, _network.HostStartCount);
        });

        [UnityTest]
        public IEnumerator ClientEndsWhenItsGameEnds() => UniTask.ToCoroutine(async () => {
            var run = StartFlow(ClientData("RELAY1"));

            _game.TrySetResult();
            var result = await run;

            Assert.IsFalse(result.StartFailed);
            Assert.AreEqual(1, _network.StopCount);
            Assert.AreEqual(1, _spawning.RemoveCount);
            Assert.IsTrue(_view.IsClosed);
        });

        [UnityTest]
        public IEnumerator FailedClientStartReturnsStartFailed() => UniTask.ToCoroutine(async () => {
            _network.ClientSucceeds = false;

            var result = await StartFlow(ClientData("RELAY1"));

            Assert.IsTrue(result.StartFailed);
            Assert.AreEqual("RELAY1", result.UsedJoinCode);
            Assert.AreEqual(0, _network.StopCount);
        });

        // A lobby can't start with one player, so the count of 1 only stands in for nobody else connecting in time.
        [UnityTest]
        public IEnumerator HostAloneEndsTheMatchWithoutPlaying() => UniTask.ToCoroutine(async () => {
            _network.ConnectedPlayerCount = 1;

            var result = await StartFlow(HostData(4, 1));

            Assert.IsTrue(result.StartFailed);
            Assert.AreEqual("RELAY1", result.UsedJoinCode);
            Assert.AreEqual(0, _spawning.StartCount);
            Assert.AreEqual(-1, _gamePlayerCount);
            Assert.AreEqual(MatchStatus.Starting, _status.GetStatus());
            Assert.AreEqual(1, _network.StopCount);
            Assert.AreEqual(1, _arena.RemoveCount);
            Assert.IsTrue(_view.IsClosed);
        });

        [UnityTest]
        public IEnumerator FailingClientStartKeepsTheJoinCode() => UniTask.ToCoroutine(async () => {
            _network.ClientThrows = true;

            LogAssert.Expect(LogType.Exception, new Regex("Client start failed"));
            var result = await StartFlow(ClientData("RELAY1"));

            Assert.IsTrue(result.StartFailed);
            Assert.AreEqual("RELAY1", result.UsedJoinCode);
            Assert.AreEqual(0, _network.StopCount);
            Assert.AreEqual(1, _arena.RemoveCount);
            Assert.IsTrue(_view.IsClosed);
        });

        [UnityTest]
        public IEnumerator ClientWithoutJoinCodeReturnsStartFailed() => UniTask.ToCoroutine(async () => {
            var result = await StartFlow(ClientData(string.Empty));

            Assert.IsTrue(result.StartFailed);
            Assert.AreEqual(0, _network.ClientStartCount);
            Assert.AreEqual(0, _network.StopCount);
            Assert.IsTrue(_view.IsClosed);
        });

        [UnityTest]
        public IEnumerator ClientShowsStartingWhileWaitingForJoinCode() => UniTask.ToCoroutine(async () => {
            var joinCode = new UniTaskCompletionSource<string>();
            var run = StartFlow(MatchFlowData.Client("Ana", _ => joinCode.Task, PlayGame));

            Assert.AreEqual(MatchStatus.Starting, _status.GetStatus());
            Assert.AreEqual(0, _network.ClientStartCount);

            joinCode.TrySetResult("RELAY1");
            Assert.AreEqual("RELAY1", _network.LastClientJoinCode);
            _network.RaiseDisconnected();
            await run;
        });

        [UnityTest]
        public IEnumerator MenuOpensOnlyOnceTheGameRuns() => UniTask.ToCoroutine(async () => {
            var joinCode = new UniTaskCompletionSource<string>();
            var run = StartFlow(MatchFlowData.Client("Ana", _ => joinCode.Task, PlayGame));

            _view.RaiseMenuRequested();
            Assert.AreEqual(0, _menu.StartCount);

            joinCode.TrySetResult("RELAY1");
            _view.RaiseMenuRequested();
            Assert.AreEqual(1, _menu.StartCount);

            _network.RaiseDisconnected();
            await run;
        });

        [UnityTest]
        public IEnumerator SecondMenuRequestClosesTheMenuAndTheGameGoesOn() => UniTask.ToCoroutine(async () => {
            var run = StartFlow(ClientData("RELAY1"));

            _view.RaiseMenuRequested();
            _view.RaiseMenuRequested();
            await UniTask.Yield();

            Assert.IsTrue(_menu.LastCancellation.IsCancellationRequested);
            Assert.AreEqual(UniTaskStatus.Pending, run.Status);

            _view.RaiseMenuRequested();
            Assert.AreEqual(2, _menu.StartCount);

            _network.RaiseDisconnected();
            await run;
        });

        [UnityTest]
        public IEnumerator ContinueClosesTheMenuAndTheGameGoesOn() => UniTask.ToCoroutine(async () => {
            var run = StartFlow(ClientData("RELAY1"));

            _view.RaiseMenuRequested();
            _menu.Complete(new MatchMenuFlowResult { Choice = MatchMenuChoice.Continue });
            await UniTask.Yield();

            Assert.AreEqual(UniTaskStatus.Pending, run.Status);
            Assert.AreEqual(0, _network.StopCount);

            _network.RaiseDisconnected();
            await run;
        });

        [UnityTest]
        public IEnumerator ClientLeavingEndsOnlyItsOwnMatch() => UniTask.ToCoroutine(async () => {
            var run = StartFlow(ClientData("RELAY1"));

            _view.RaiseMenuRequested();
            _menu.Complete(new MatchMenuFlowResult { Choice = MatchMenuChoice.Leave });
            var result = await run;

            Assert.IsFalse(result.StartFailed);
            Assert.AreEqual("RELAY1", result.UsedJoinCode);
            Assert.AreEqual(0, _network.AnnounceEndCount);
            Assert.AreEqual(1, _network.AnnounceLeavingCount);
            Assert.IsFalse(_network.WasStoppedOnAnnounce);
            Assert.AreEqual(1, _network.StopCount);
            Assert.AreEqual(1, _spawning.RemoveCount);
        });

        [UnityTest]
        public IEnumerator HostLeavingTellsEveryoneBeforeStopping() => UniTask.ToCoroutine(async () => {
            _network.ConnectedPlayerCount = 2;
            var run = StartFlow(HostData(4, 2));
            await UniTask.WaitUntil(() => _status.GetStatus() == MatchStatus.InProgress, cancellationToken: _cancellation.Token);

            _view.RaiseMenuRequested();
            _menu.Complete(new MatchMenuFlowResult { Choice = MatchMenuChoice.Leave });
            await run;

            Assert.AreEqual(1, _network.AnnounceEndCount);
            Assert.AreEqual(0, _network.AnnounceLeavingCount);
            Assert.IsFalse(_network.WasStoppedOnAnnounce);
            Assert.AreEqual(1, _network.StopCount);
            Assert.AreEqual(1, _spawning.StopCount);
        });

        [UnityTest]
        public IEnumerator ClientEndsWhenTheHostEndsTheMatch() => UniTask.ToCoroutine(async () => {
            var run = StartFlow(ClientData("RELAY1"));

            _network.RaiseHostEndedMatch();
            var result = await run;

            Assert.IsFalse(result.StartFailed);
            Assert.AreEqual(1, _network.StopCount);
        });

        [UnityTest]
        public IEnumerator MatchEndingClosesAnOpenMenu() => UniTask.ToCoroutine(async () => {
            var run = StartFlow(ClientData("RELAY1"));

            _view.RaiseMenuRequested();
            _game.TrySetResult();
            await run;
            await UniTask.Yield();

            Assert.IsTrue(_menu.LastCancellation.IsCancellationRequested);
        });

        private MatchFlowData ClientData(string joinCode) {
            return MatchFlowData.Client("Ana", _ => UniTask.FromResult(joinCode), PlayGame);
        }

        private MatchFlowData HostData(int maxPlayers, int playerCount) {
            return MatchFlowData.Host(maxPlayers, playerCount, "Ana", joinCode => {
                _sharedJoinCode = joinCode;
                return UniTask.CompletedTask;
            }, PlayGame);
        }

        private UniTask PlayGame(int playerCount, CancellationToken cancellation) {
            _gamePlayerCount = playerCount;
            return _game.Task.AttachExternalCancellation(cancellation);
        }

        private UniTask<MatchFlowResult> StartFlow(MatchFlowData data) {
            var flow = new MatchFlow(data, _view, _network, new MatchPlayerSpawner(_network, _spawning), _spawning, _arena, _localPlayer, _status, _flowRunner);
            return flow.RunAsync(_cancellation.Token).Preserve();
        }
    }
}
