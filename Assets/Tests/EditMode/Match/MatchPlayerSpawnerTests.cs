using System.Collections;
using System.Threading;
using Code.Match;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Code.Tests.Match {

    public class MatchPlayerSpawnerTests {

        private const int TIMEOUT_MS = 5000;

        private FakeMatchNetwork _network;
        private FakePlayerSpawning _spawning;
        private MatchPlayerSpawner _spawner;
        private CancellationTokenSource _cancellation;

        [SetUp]
        public void SetUp() {
            _network = new FakeMatchNetwork();
            _spawning = new FakePlayerSpawning();
            _spawner = new MatchPlayerSpawner(_network, _spawning);
            _cancellation = new CancellationTokenSource(TIMEOUT_MS);
        }

        [TearDown]
        public void TearDown() {
            _cancellation.Cancel();
            _cancellation.Dispose();
        }

        [UnityTest]
        public IEnumerator WaitsForAllPlayersBeforeSpawning() => UniTask.ToCoroutine(async () => {
            _network.ConnectedPlayerCount = 1;
            var run = _spawner.SpawnWhenAllJoined(3, _cancellation.Token).Preserve();

            await UniTask.Yield();
            _network.ConnectedPlayerCount = 2;
            await UniTask.Yield();
            Assert.AreEqual(0, _spawning.StartCount);

            _network.ConnectedPlayerCount = 3;
            await run;

            Assert.AreEqual(1, _spawning.StartCount);
        });

        [UnityTest]
        public IEnumerator SpawnsRightAwayWhenEveryoneIsConnected() => UniTask.ToCoroutine(async () => {
            _network.ConnectedPlayerCount = 2;

            await _spawner.SpawnWhenAllJoined(2, _cancellation.Token);

            Assert.AreEqual(1, _spawning.StartCount);
        });
    }
}
