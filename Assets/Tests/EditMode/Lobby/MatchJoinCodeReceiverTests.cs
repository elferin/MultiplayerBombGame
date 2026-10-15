using System;
using System.Collections;
using System.Threading;
using Code.Lobby;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Code.Tests.Lobby {

    public class MatchJoinCodeReceiverTests {

        private const int TIMEOUT_MS = 5000;

        private FakeLobbyService _lobbyService;
        private MatchJoinCodeReceiver _receiver;
        private CancellationTokenSource _cancellation;

        [SetUp]
        public void SetUp() {
            _lobbyService = new FakeLobbyService();
            _receiver = new MatchJoinCodeReceiver(_lobbyService);
            _cancellation = new CancellationTokenSource(TIMEOUT_MS);
        }

        [TearDown]
        public void TearDown() {
            _cancellation.Cancel();
            _cancellation.Dispose();
        }

        [UnityTest]
        public IEnumerator ReturnsCodeAlreadyInTheLobby() => UniTask.ToCoroutine(async () => {
            _lobbyService.CurrentLobby = CreateLobby("RELAY1", true);

            Assert.AreEqual("RELAY1", await _receiver.WaitForJoinCode(string.Empty, _cancellation.Token));
        });

        [UnityTest]
        public IEnumerator WaitsForTheHostToShareTheCode() => UniTask.ToCoroutine(async () => {
            _lobbyService.CurrentLobby = CreateLobby(string.Empty, true);
            var wait = _receiver.WaitForJoinCode(string.Empty, _cancellation.Token).Preserve();

            Assert.AreEqual(UniTaskStatus.Pending, wait.Status);
            _lobbyService.RaiseLobbyUpdated(CreateLobby("RELAY1", true));

            Assert.AreEqual("RELAY1", await wait);
        });

        [UnityTest]
        public IEnumerator IgnoresTheCodeOfTheLastMatch() => UniTask.ToCoroutine(async () => {
            _lobbyService.CurrentLobby = CreateLobby("OLD", true);
            var wait = _receiver.WaitForJoinCode("OLD", _cancellation.Token).Preserve();

            Assert.AreEqual(UniTaskStatus.Pending, wait.Status);
            _lobbyService.RaiseLobbyUpdated(CreateLobby("NEW", true));

            Assert.AreEqual("NEW", await wait);
        });

        [UnityTest]
        public IEnumerator ReturnsEmptyWhenTheHostsStartFails() => UniTask.ToCoroutine(async () => {
            _lobbyService.CurrentLobby = CreateLobby(string.Empty, true);
            var wait = _receiver.WaitForJoinCode(string.Empty, _cancellation.Token).Preserve();

            _lobbyService.RaiseLobbyUpdated(CreateLobby(string.Empty, false));

            Assert.AreEqual(string.Empty, await wait);
        });

        [UnityTest]
        public IEnumerator ReturnsEmptyAfterTheTimeout() => UniTask.ToCoroutine(async () => {
            _lobbyService.CurrentLobby = CreateLobby(string.Empty, true);

            var joinCode = await _receiver.WaitForJoinCode(string.Empty, TimeSpan.FromMilliseconds(50), _cancellation.Token);

            Assert.AreEqual(string.Empty, joinCode);
        });

        [UnityTest]
        public IEnumerator CancellingThrows() => UniTask.ToCoroutine(async () => {
            _lobbyService.CurrentLobby = CreateLobby(string.Empty, true);
            var wait = _receiver.WaitForJoinCode(string.Empty, _cancellation.Token);

            _cancellation.Cancel();
            var wasCancelled = false;
            try {
                await wait;
            }
            catch (OperationCanceledException) {
                wasCancelled = true;
            }

            Assert.IsTrue(wasCancelled);
        });

        private LobbyState CreateLobby(string matchJoinCode, bool isMatchStarting) {
            return new LobbyState(true, "ABC123", 4, FakeLobbyService.CreatePlayers("Ana", "Luka"), false, matchJoinCode, isMatchStarting);
        }
    }
}
