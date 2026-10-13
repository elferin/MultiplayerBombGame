using System.Collections;
using System.Threading;
using Code.Match;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Code.Tests {

    public class MatchFlowTests {

        private const int TIMEOUT_MS = 5000;

        private MatchStatusHolder _status;
        private FakeMatchViewController _view;
        private FakeMatchNetwork _network;
        private FakePlayerSpawning _spawning;
        private CancellationTokenSource _cancellation;
        private string _sharedJoinCode;

        [SetUp]
        public void SetUp() {
            _status = new MatchStatusHolder();
            _view = new FakeMatchViewController(_status);
            _network = new FakeMatchNetwork();
            _spawning = new FakePlayerSpawning();
            _cancellation = new CancellationTokenSource(TIMEOUT_MS);
            _sharedJoinCode = string.Empty;
        }

        [TearDown]
        public void TearDown() {
            _cancellation.Cancel();
            _cancellation.Dispose();
        }

        [UnityTest]
        public IEnumerator HostSharesJoinCodeSpawnsAndStopsOnEnd() => UniTask.ToCoroutine(async () => {
            _network.ConnectedPlayerCount = 2;
            var run = StartFlow(HostArgs(4, 2));

            await UniTask.WaitUntil(() => _status.GetStatus() == MatchStatus.InProgress, cancellationToken: _cancellation.Token);
            Assert.AreEqual(4, _network.LastHostMaxPlayers);
            Assert.AreEqual("RELAY1", _sharedJoinCode);
            Assert.AreEqual(1, _spawning.StartCount);
            Assert.IsTrue(_status.IsHost());
            Assert.AreEqual(0, _network.StopCount);

            _view.ClickEnd();
            var result = await run;

            Assert.IsFalse(result.StartFailed);
            Assert.AreEqual("RELAY1", result.UsedJoinCode);
            Assert.AreEqual(1, _network.StopCount);
            Assert.AreEqual(1, _spawning.StopCount);
            Assert.IsTrue(_view.IsClosed);
        });

        [UnityTest]
        public IEnumerator FailedHostStartReturnsStartFailed() => UniTask.ToCoroutine(async () => {
            _network.HostJoinCode = string.Empty;

            var result = await StartFlow(HostArgs(4, 2));

            Assert.IsTrue(result.StartFailed);
            Assert.AreEqual(string.Empty, _sharedJoinCode);
            Assert.AreEqual(0, _spawning.StartCount);
            Assert.AreEqual(0, _network.StopCount);
            Assert.IsTrue(_view.IsClosed);
        });

        [UnityTest]
        public IEnumerator ClientConnectsAndEndsWhenDisconnected() => UniTask.ToCoroutine(async () => {
            var run = StartFlow(MatchFlowArgs.Client("RELAY1"));

            Assert.AreEqual("RELAY1", _network.LastClientJoinCode);
            Assert.AreEqual(MatchStatus.InProgress, _status.GetStatus());
            Assert.IsFalse(_status.IsHost());

            _network.RaiseDisconnected();
            var result = await run;

            Assert.IsFalse(result.StartFailed);
            Assert.AreEqual("RELAY1", result.UsedJoinCode);
            Assert.AreEqual(1, _network.StopCount);
            Assert.AreEqual(0, _spawning.StopCount);
            Assert.AreEqual(0, _network.HostStartCount);
        });

        [UnityTest]
        public IEnumerator FailedClientStartReturnsStartFailed() => UniTask.ToCoroutine(async () => {
            _network.ClientSucceeds = false;

            var result = await StartFlow(MatchFlowArgs.Client("RELAY1"));

            Assert.IsTrue(result.StartFailed);
            Assert.AreEqual("RELAY1", result.UsedJoinCode);
            Assert.AreEqual(0, _network.StopCount);
        });

        private MatchFlowArgs HostArgs(int maxPlayers, int playerCount) {
            return MatchFlowArgs.Host(maxPlayers, playerCount, joinCode => {
                _sharedJoinCode = joinCode;
                return UniTask.CompletedTask;
            });
        }

        private UniTask<MatchFlowResult> StartFlow(MatchFlowArgs args) {
            var flow = new MatchFlow(args, _view, _network, new MatchPlayerSpawner(_network, _spawning), _spawning, _status);
            return flow.RunAsync(_cancellation.Token).Preserve();
        }
    }
}
