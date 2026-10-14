using System;
using System.Collections;
using System.Threading;
using Code.Lobby;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Code.Tests {

    public class LobbyJoinerTests {

        private const int TIMEOUT_MS = 5000;

        private FakeLobbyService _lobbyService;
        private JoinLobbyState _joinLobby;
        private LobbyJoiner _joiner;
        private CancellationTokenSource _cancellation;

        [SetUp]
        public void SetUp() {
            _lobbyService = new FakeLobbyService();
            _joinLobby = new JoinLobbyState();
            _joiner = new LobbyJoiner(_lobbyService, _joinLobby);
            _cancellation = new CancellationTokenSource(TIMEOUT_MS);
        }

        [TearDown]
        public void TearDown() {
            _cancellation.Cancel();
            _cancellation.Dispose();
        }

        [Test]
        public void IsJoiningAsSoonAsCalled() {
            _joinLobby.SetLobbyCode("ABC123");

            _joiner.TryJoinLobby("Luka", _cancellation.Token).Forget();

            Assert.AreEqual(JoinLobbyStatus.Joining, _joinLobby.GetJoinStatus());
            Assert.IsTrue(_lobbyService.IsJoinPending);
        }

        [Test]
        public void SendsTrimmedCodeAndPlayerName() {
            _joinLobby.SetLobbyCode("  ABC123 ");

            _joiner.TryJoinLobby("Luka", _cancellation.Token).Forget();

            Assert.AreEqual("ABC123", _lobbyService.LastJoinCode);
            Assert.AreEqual("Luka", _lobbyService.LastJoinPlayerName);
        }

        [UnityTest]
        public IEnumerator SuccessfulJoinReturnsTrueAndGoesIdle() => UniTask.ToCoroutine(async () => {
            _joinLobby.SetLobbyCode("ABC123");
            var join = _joiner.TryJoinLobby("Luka", _cancellation.Token);

            _lobbyService.SucceedJoin();

            Assert.IsTrue(await join);
            Assert.AreEqual(JoinLobbyStatus.Idle, _joinLobby.GetJoinStatus());
        });

        [UnityTest]
        public IEnumerator FailedJoinReturnsFalseAndFails() => UniTask.ToCoroutine(async () => {
            _joinLobby.SetLobbyCode("WRONG");
            var join = _joiner.TryJoinLobby("Luka", _cancellation.Token);

            _lobbyService.FailJoin();

            Assert.IsFalse(await join);
            Assert.AreEqual(JoinLobbyStatus.Failed, _joinLobby.GetJoinStatus());
        });

        [UnityTest]
        public IEnumerator LockedLobbyShowsMatchInProgress() => UniTask.ToCoroutine(async () => {
            _joinLobby.SetLobbyCode("ABC123");
            var join = _joiner.TryJoinLobby("Luka", _cancellation.Token);

            _lobbyService.FailJoin(LobbyJoinFailure.MatchInProgress);

            Assert.IsFalse(await join);
            Assert.AreEqual(JoinLobbyStatus.MatchInProgress, _joinLobby.GetJoinStatus());
        });

        [UnityTest]
        public IEnumerator FullLobbyShowsLobbyFull() => UniTask.ToCoroutine(async () => {
            _joinLobby.SetLobbyCode("ABC123");
            var join = _joiner.TryJoinLobby("Luka", _cancellation.Token);

            _lobbyService.FailJoin(LobbyJoinFailure.LobbyFull);

            Assert.IsFalse(await join);
            Assert.AreEqual(JoinLobbyStatus.LobbyFull, _joinLobby.GetJoinStatus());
        });

        [UnityTest]
        public IEnumerator JoiningStartedMatchLeavesAndShowsMatchInProgress() => UniTask.ToCoroutine(async () => {
            _joinLobby.SetLobbyCode("ABC123");
            var join = _joiner.TryJoinLobby("Luka", _cancellation.Token);

            _lobbyService.SucceedJoin(new LobbyState(true, "ABC123", 4, FakeLobbyService.CreatePlayers("Ana", "Luka"), false, "RELAY1", false));
            Assert.IsTrue(_lobbyService.IsLeavePending);
            _lobbyService.CompleteLeave();

            Assert.IsFalse(await join);
            Assert.AreEqual(1, _lobbyService.LeaveCount);
            Assert.AreEqual(JoinLobbyStatus.MatchInProgress, _joinLobby.GetJoinStatus());
        });

        [UnityTest]
        public IEnumerator JoiningStartingMatchLeavesAndShowsMatchInProgress() => UniTask.ToCoroutine(async () => {
            _joinLobby.SetLobbyCode("ABC123");
            var join = _joiner.TryJoinLobby("Luka", _cancellation.Token);

            _lobbyService.SucceedJoin(new LobbyState(true, "ABC123", 4, FakeLobbyService.CreatePlayers("Ana", "Luka"), false, string.Empty, true));
            _lobbyService.CompleteLeave();

            Assert.IsFalse(await join);
            Assert.AreEqual(1, _lobbyService.LeaveCount);
            Assert.AreEqual(JoinLobbyStatus.MatchInProgress, _joinLobby.GetJoinStatus());
        });

        [UnityTest]
        public IEnumerator CancellingThrows() => UniTask.ToCoroutine(async () => {
            _joinLobby.SetLobbyCode("ABC123");
            var join = _joiner.TryJoinLobby("Luka", _cancellation.Token);

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
    }
}
