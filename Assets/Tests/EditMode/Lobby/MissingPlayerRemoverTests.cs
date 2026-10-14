using System;
using System.Collections;
using System.Threading;
using Code.Lobby;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Code.Tests {

    public class MissingPlayerRemoverTests {

        private const int TIMEOUT_MS = 5000;
        private static readonly TimeSpan SHORT_TIMEOUT = TimeSpan.FromMilliseconds(50);

        private FakeLobbyService _lobbyService;
        private CurrentLobbyHolder _currentLobby;
        private MissingPlayerRemover _remover;
        private CancellationTokenSource _cancellation;

        [SetUp]
        public void SetUp() {
            _lobbyService = new FakeLobbyService();
            _currentLobby = new CurrentLobbyHolder();
            _remover = new MissingPlayerRemover(_lobbyService, _currentLobby);
            _cancellation = new CancellationTokenSource(TIMEOUT_MS);
        }

        [TearDown]
        public void TearDown() {
            _cancellation.Cancel();
            _cancellation.Dispose();
        }

        [UnityTest]
        public IEnumerator RemovesOnlyPlayersWhoHaventReturned() => UniTask.ToCoroutine(async () => {
            _currentLobby.SetCurrentLobby(CreateLobby(
                new LobbyPlayer("host", "Luka", false, true),
                new LobbyPlayer("ana", "Ana", true, false),
                new LobbyPlayer("ivo", "Ivo", false, false)));

            await _remover.RemoveMissingPlayersAfter(SHORT_TIMEOUT, _cancellation.Token);

            CollectionAssert.AreEqual(new[] { "ivo" }, _lobbyService.RemovedPlayerIds);
        });

        [UnityTest]
        public IEnumerator RemovesNobodyWhenEveryoneReturned() => UniTask.ToCoroutine(async () => {
            _currentLobby.SetCurrentLobby(CreateLobby(
                new LobbyPlayer("host", "Luka", true, true),
                new LobbyPlayer("ana", "Ana", true, false)));

            await _remover.RemoveMissingPlayersAfter(SHORT_TIMEOUT, _cancellation.Token);

            Assert.IsEmpty(_lobbyService.RemovedPlayerIds);
        });

        [UnityTest]
        public IEnumerator CancellingBeforeTheTimeoutRemovesNobody() => UniTask.ToCoroutine(async () => {
            _currentLobby.SetCurrentLobby(CreateLobby(
                new LobbyPlayer("host", "Luka", true, true),
                new LobbyPlayer("ivo", "Ivo", false, false)));
            var removal = _remover.RemoveMissingPlayersAfter(TimeSpan.FromSeconds(10), _cancellation.Token);

            _cancellation.Cancel();
            var wasCancelled = false;
            try {
                await removal;
            }
            catch (OperationCanceledException) {
                wasCancelled = true;
            }

            Assert.IsTrue(wasCancelled);
            Assert.IsEmpty(_lobbyService.RemovedPlayerIds);
        });

        private LobbyState CreateLobby(params LobbyPlayer[] players) {
            return new LobbyState(true, "ABC123", 4, players, true, string.Empty, false);
        }
    }
}
