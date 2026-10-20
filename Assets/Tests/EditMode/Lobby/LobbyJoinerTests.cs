using System;
using System.Collections;
using System.Threading;
using Code.Lobby;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Code.Tests.Lobby {

    public class LobbyJoinerTests {

        private const int TIMEOUT_MS = 5000;

        private FakeLobbyService _lobbyService;
        private LobbyStatusHolder _status;
        private CurrentLobbyHolder _currentLobby;
        private LobbyJoiner _joiner;
        private CancellationTokenSource _cancellation;

        [SetUp]
        public void SetUp() {
            _lobbyService = new FakeLobbyService();
            _status = new LobbyStatusHolder();
            _currentLobby = new CurrentLobbyHolder();
            _joiner = new LobbyJoiner(_lobbyService, _status, _currentLobby);
            _cancellation = new CancellationTokenSource(TIMEOUT_MS);
        }

        [TearDown]
        public void TearDown() {
            _cancellation.Cancel();
            _cancellation.Dispose();
        }

        [Test]
        public void IsJoiningWithoutALobbyAsSoonAsCalled() {
            _currentLobby.SetCurrentLobby(FakeLobbyService.CreateLobbyState("OLD123", 4, "Luka"));

            _joiner.TryJoinLobby("ABC123", "Luka", _cancellation.Token).Forget();

            Assert.AreEqual(LobbyStatus.Joining, _status.GetStatus());
            Assert.AreSame(LobbyState.None, _currentLobby.GetCurrentLobby());
            Assert.IsTrue(_lobbyService.IsJoinPending);
        }

        [Test]
        public void SendsCodeAndPlayerName() {
            _joiner.TryJoinLobby("ABC123", "Luka", _cancellation.Token).Forget();

            Assert.AreEqual("ABC123", _lobbyService.LastJoinCode);
            Assert.AreEqual("Luka", _lobbyService.LastJoinPlayerName);
        }

        [UnityTest]
        public IEnumerator SuccessfulJoinShowsLobbyAsReady() => UniTask.ToCoroutine(async () => {
            var join = _joiner.TryJoinLobby("ABC123", "Luka", _cancellation.Token);

            _lobbyService.SucceedJoin(CreateLobby(string.Empty, false));

            Assert.AreEqual(LobbyJoinFailure.None, await join);
            Assert.AreEqual(LobbyStatus.Ready, _status.GetStatus());
            Assert.AreEqual("ABC123", _currentLobby.GetCurrentLobby().Code);
        });

        [UnityTest]
        public IEnumerator FailedJoinReturnsItsFailure() => UniTask.ToCoroutine(async () => {
            var join = _joiner.TryJoinLobby("WRONG", "Luka", _cancellation.Token);

            _lobbyService.FailJoin(LobbyJoinFailure.LobbyFull);

            Assert.AreEqual(LobbyJoinFailure.LobbyFull, await join);
            Assert.AreEqual(LobbyStatus.JoinFailed, _status.GetStatus());
            Assert.AreSame(LobbyState.None, _currentLobby.GetCurrentLobby());
        });

        [UnityTest]
        public IEnumerator JoiningStartedMatchLeavesAndReturnsMatchInProgress() => UniTask.ToCoroutine(async () => {
            var join = _joiner.TryJoinLobby("ABC123", "Luka", _cancellation.Token);

            _lobbyService.SucceedJoin(CreateLobby("RELAY1", false));
            Assert.IsTrue(_lobbyService.IsLeavePending);
            _lobbyService.CompleteLeave();

            Assert.AreEqual(LobbyJoinFailure.MatchInProgress, await join);
            Assert.AreEqual(1, _lobbyService.LeaveCount);
            Assert.AreSame(LobbyState.None, _currentLobby.GetCurrentLobby());
        });

        [UnityTest]
        public IEnumerator JoiningStartingMatchLeavesAndReturnsMatchInProgress() => UniTask.ToCoroutine(async () => {
            var join = _joiner.TryJoinLobby("ABC123", "Luka", _cancellation.Token);

            _lobbyService.SucceedJoin(CreateLobby(string.Empty, true));
            _lobbyService.CompleteLeave();

            Assert.AreEqual(LobbyJoinFailure.MatchInProgress, await join);
            Assert.AreEqual(1, _lobbyService.LeaveCount);
        });

        [UnityTest]
        public IEnumerator CancellingThrows() => UniTask.ToCoroutine(async () => {
            var join = _joiner.TryJoinLobby("ABC123", "Luka", _cancellation.Token);

            _cancellation.Cancel();
            var wasCancelled = false;
            try {
                await join;
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
