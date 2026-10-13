using System;
using System.Collections;
using System.Threading;
using Code.Lobby;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Code.Tests {

    public class LobbyFlowTests {

        private const int TIMEOUT_MS = 5000;

        private LobbyStatusHolder _status;
        private CurrentLobbyHolder _currentLobby;
        private FakeLobbyViewController _view;
        private FakeLobbyService _lobbyService;
        private CancellationTokenSource _cancellation;

        [SetUp]
        public void SetUp() {
            _status = new LobbyStatusHolder();
            _currentLobby = new CurrentLobbyHolder();
            _view = new FakeLobbyViewController(_status);
            _lobbyService = new FakeLobbyService();
            _cancellation = new CancellationTokenSource(TIMEOUT_MS);
        }

        [TearDown]
        public void TearDown() {
            _cancellation.Cancel();
            _cancellation.Dispose();
        }

        [Test]
        public void CreatingShowsCreatingWhileRequestIsPending() {
            StartFlow(LobbyFlowArgs.CreateLobby(6, "Luka"));

            Assert.IsTrue(_view.IsRunning);
            Assert.AreEqual(LobbyStatus.Creating, _status.GetStatus());
            Assert.AreEqual(6, _lobbyService.LastCreateMaxPlayers);
            Assert.AreEqual("Luka", _lobbyService.LastCreatePlayerName);
        }

        [Test]
        public void NewRunDoesNotShowPreviousRunsState() {
            _status.SetStatus(LobbyStatus.Failed);
            _currentLobby.SetCurrentLobby(FakeLobbyService.CreateLobbyState("OLD123", 4, "Luka"));

            StartFlow(LobbyFlowArgs.CreateLobby(4, "Luka"));

            Assert.AreEqual(LobbyStatus.Creating, _status.GetStatus());
            Assert.AreSame(LobbyState.None, _currentLobby.GetCurrentLobby());
        }

        [Test]
        public void CreatedLobbyIsShownAsReady() {
            StartFlow(LobbyFlowArgs.CreateLobby(6, "Luka"));

            _lobbyService.SucceedCreate();

            Assert.AreEqual(LobbyStatus.Ready, _status.GetStatus());
            Assert.AreEqual("NEW123", _currentLobby.GetCurrentLobby().Code);
            CollectionAssert.AreEqual(new[] { LobbyStatus.Ready }, _view.RefreshedStatuses);
        }

        [UnityTest]
        public IEnumerator LeavingLeavesLobbyThenCloses() => UniTask.ToCoroutine(async () => {
            var run = StartFlow(LobbyFlowArgs.CreateLobby(6, "Luka"));
            _lobbyService.SucceedCreate();

            _view.ClickLeave();

            Assert.AreEqual(LobbyStatus.Leaving, _status.GetStatus());
            Assert.IsTrue(_lobbyService.IsLeavePending);
            Assert.IsFalse(_view.IsClosed);

            _lobbyService.CompleteLeave();
            await run;

            Assert.AreEqual(1, _lobbyService.LeaveCount);
            Assert.IsTrue(_view.IsClosed);
        });

        [UnityTest]
        public IEnumerator StartClickEndsFlowAsHost() => UniTask.ToCoroutine(async () => {
            var run = StartFlow(LobbyFlowArgs.CreateLobby(6, "Luka"));
            _lobbyService.SucceedCreate();

            _view.ClickStart();
            var result = await run;

            Assert.IsTrue(result.ShouldStartMatch);
            Assert.IsTrue(result.IsHost);
            Assert.AreEqual(6, result.MaxPlayers);
            Assert.AreEqual(1, result.PlayerCount);
            Assert.AreEqual(string.Empty, result.MatchJoinCode);
            // The match locks the lobby once its network runs, not the lobby flow.
            Assert.IsFalse(_lobbyService.IsLocked);
            Assert.AreEqual(0, _lobbyService.LeaveCount);
            Assert.IsTrue(_view.IsClosed);
        });

        [UnityTest]
        public IEnumerator JoinedPlayerEndsFlowWhenHostSharesJoinCode() => UniTask.ToCoroutine(async () => {
            _lobbyService.CurrentLobby = CreateJoinedLobby(string.Empty);
            var run = StartFlow(LobbyFlowArgs.JoinedLobby());

            _lobbyService.RaiseLobbyUpdated(CreateJoinedLobby("RELAY1"));
            var result = await run;

            Assert.IsTrue(result.ShouldStartMatch);
            Assert.IsFalse(result.IsHost);
            Assert.AreEqual("RELAY1", result.MatchJoinCode);
            Assert.AreEqual(0, _lobbyService.LeaveCount);
        });

        [Test]
        public void HostReturningFromMatchReopensLobby() {
            _lobbyService.CurrentLobby = CreateHostedLobby("RELAY1");

            StartFlow(LobbyFlowArgs.ReturnFromMatch(string.Empty, false));

            Assert.AreEqual(1, _lobbyService.EndMatchCount);
            Assert.AreEqual(LobbyStatus.Ready, _status.GetStatus());
            Assert.IsFalse(_view.IsClosed);
        }

        [UnityTest]
        public IEnumerator ReturningClientIgnoresUsedJoinCode() => UniTask.ToCoroutine(async () => {
            _lobbyService.CurrentLobby = CreateJoinedLobby("RELAY1");
            var run = StartFlow(LobbyFlowArgs.ReturnFromMatch("RELAY1", false));

            _lobbyService.RaiseLobbyUpdated(CreateJoinedLobby("RELAY1"));
            Assert.IsFalse(_view.IsClosed);
            Assert.AreEqual(0, _lobbyService.EndMatchCount);

            _lobbyService.RaiseLobbyUpdated(CreateJoinedLobby("RELAY2"));
            var result = await run;

            Assert.IsTrue(result.ShouldStartMatch);
            Assert.AreEqual("RELAY2", result.MatchJoinCode);
        });

        [Test]
        public void FailedMatchStartShowsStartFailed() {
            _lobbyService.CurrentLobby = CreateHostedLobby(string.Empty);

            StartFlow(LobbyFlowArgs.ReturnFromMatch(string.Empty, true));

            Assert.AreEqual(LobbyStatus.StartFailed, _status.GetStatus());
        }

        [UnityTest]
        public IEnumerator LeavingDoesNotStartMatch() => UniTask.ToCoroutine(async () => {
            var run = StartFlow(LobbyFlowArgs.CreateLobby(6, "Luka"));
            _lobbyService.SucceedCreate();

            _view.ClickLeave();
            _lobbyService.CompleteLeave();
            var result = await run;

            Assert.IsFalse(result.ShouldStartMatch);
        });

        [UnityTest]
        public IEnumerator FailedCreateShowsFailedAndLeaveJustCloses() => UniTask.ToCoroutine(async () => {
            var run = StartFlow(LobbyFlowArgs.CreateLobby(6, "Luka"));

            _lobbyService.FailCreate();
            Assert.AreEqual(LobbyStatus.Failed, _status.GetStatus());

            _view.ClickLeave();
            await run;

            Assert.AreEqual(0, _lobbyService.LeaveCount);
            Assert.IsTrue(_view.IsClosed);
        });

        [UnityTest]
        public IEnumerator LeaveClickedWhileCreatingLeavesOnceCreated() => UniTask.ToCoroutine(async () => {
            var run = StartFlow(LobbyFlowArgs.CreateLobby(6, "Luka"));

            _view.ClickLeave();
            _lobbyService.SucceedCreate();

            Assert.AreEqual(1, _lobbyService.LeaveCount);
            _lobbyService.CompleteLeave();
            await run;
        });

        [Test]
        public void JoinedLobbyShowsCurrentLobbyWithoutCreating() {
            _lobbyService.CurrentLobby = FakeLobbyService.CreateLobbyState("ABC123", 4, "Ana", "Luka");

            StartFlow(LobbyFlowArgs.JoinedLobby());

            Assert.AreEqual(0, _lobbyService.CreateCount);
            Assert.AreEqual(LobbyStatus.Ready, _status.GetStatus());
            Assert.AreEqual("ABC123", _currentLobby.GetCurrentLobby().Code);
        }

        [Test]
        public void LobbyUpdatesRefreshTheView() {
            StartFlow(LobbyFlowArgs.CreateLobby(4, "Luka"));
            _lobbyService.SucceedCreate();
            var update = FakeLobbyService.CreateLobbyState("NEW123", 4, "Luka", "Ana");

            _lobbyService.RaiseLobbyUpdated(update);

            Assert.AreSame(update, _currentLobby.GetCurrentLobby());
            Assert.AreEqual(2, _view.RefreshedStatuses.Count);
        }

        [Test]
        public void InvalidLobbyUpdateIsIgnored() {
            StartFlow(LobbyFlowArgs.CreateLobby(4, "Luka"));
            _lobbyService.SucceedCreate();
            var lobby = _currentLobby.GetCurrentLobby();

            _lobbyService.RaiseLobbyUpdated(LobbyState.None);

            Assert.AreSame(lobby, _currentLobby.GetCurrentLobby());
            Assert.AreEqual(1, _view.RefreshedStatuses.Count);
        }

        [UnityTest]
        public IEnumerator UpdatesAfterFlowEndsAreIgnored() => UniTask.ToCoroutine(async () => {
            var run = StartFlow(LobbyFlowArgs.CreateLobby(4, "Luka"));
            _lobbyService.SucceedCreate();
            _view.ClickLeave();
            _lobbyService.CompleteLeave();
            await run;
            var refreshCount = _view.RefreshedStatuses.Count;

            _lobbyService.RaiseLobbyUpdated(FakeLobbyService.CreateLobbyState("NEW123", 4, "Luka", "Ana"));

            Assert.AreEqual(refreshCount, _view.RefreshedStatuses.Count);
        });

        [UnityTest]
        public IEnumerator CancellingInLobbyLeavesLobby() => UniTask.ToCoroutine(async () => {
            var run = StartFlow(LobbyFlowArgs.CreateLobby(4, "Luka"));
            _lobbyService.SucceedCreate();

            _cancellation.Cancel();
            var wasCancelled = false;
            try {
                await run;
            }
            catch (OperationCanceledException) {
                wasCancelled = true;
            }

            Assert.IsTrue(wasCancelled);
            Assert.IsTrue(_view.IsClosed);
            Assert.AreEqual(1, _lobbyService.LeaveCount);
        });

        [UnityTest]
        public IEnumerator CancellingWhileCreatingDoesNotLeave() => UniTask.ToCoroutine(async () => {
            var run = StartFlow(LobbyFlowArgs.CreateLobby(4, "Luka"));

            _cancellation.Cancel();
            try {
                await run;
            }
            catch (OperationCanceledException) {
                // Expected.
            }

            // LobbyService itself leaves a lobby that finishes creating after cancellation.
            Assert.AreEqual(0, _lobbyService.LeaveCount);
            Assert.IsTrue(_view.IsClosed);
        });

        private static LobbyState CreateJoinedLobby(string matchJoinCode) {
            return new LobbyState(true, "ABC123", 4, new[] { "Ana", "Luka" }, false, matchJoinCode);
        }

        private static LobbyState CreateHostedLobby(string matchJoinCode) {
            return new LobbyState(true, "ABC123", 4, new[] { "Luka", "Ana" }, true, matchJoinCode);
        }

        // LobbyFlow has no initial await, so the view is running as soon as this returns.
        private UniTask<LobbyFlowResult> StartFlow(LobbyFlowArgs args) {
            var flow = new LobbyFlow(args, _view, _lobbyService, new LobbyCreator(_lobbyService, _status, _currentLobby), _status, _currentLobby);
            return flow.RunAsync(_cancellation.Token).Preserve();
        }
    }
}
