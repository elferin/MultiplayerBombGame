using System;
using System.Collections;
using System.Threading;
using Code.Lobby;
using Code.Tests.Util;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Code.Tests.Lobby {

    public class LobbyFlowTests {

        private const int TIMEOUT_MS = 5000;

        private LobbyStatusHolder _status;
        private CurrentLobbyHolder _currentLobby;
        private FakeLobbyViewController _view;
        private FakeLobbyService _lobbyService;
        private FakeClipboard _clipboard;
        private CancellationTokenSource _cancellation;

        [SetUp]
        public void SetUp() {
            _status = new LobbyStatusHolder();
            _currentLobby = new CurrentLobbyHolder();
            _view = new FakeLobbyViewController(_status);
            _lobbyService = new FakeLobbyService();
            _clipboard = new FakeClipboard();
            _cancellation = new CancellationTokenSource(TIMEOUT_MS);
        }

        [TearDown]
        public void TearDown() {
            _cancellation.Cancel();
            _cancellation.Dispose();
        }

        [Test]
        public void CreatingShowsCreatingWhileRequestIsPending() {
            StartFlow(LobbyFlowData.CreateLobby(6, "Luka"));

            Assert.IsTrue(_view.IsRunning);
            Assert.AreEqual(LobbyStatus.Creating, _status.GetStatus());
            Assert.AreEqual(6, _lobbyService.LastCreateMaxPlayers);
            Assert.AreEqual("Luka", _lobbyService.LastCreatePlayerName);
        }

        [Test]
        public void NewRunDoesNotShowPreviousRunsState() {
            _status.SetStatus(LobbyStatus.Failed);
            _currentLobby.SetCurrentLobby(FakeLobbyService.CreateLobbyState("OLD123", 4, "Luka"));

            StartFlow(LobbyFlowData.CreateLobby(4, "Luka"));

            Assert.AreEqual(LobbyStatus.Creating, _status.GetStatus());
            Assert.AreSame(LobbyState.None, _currentLobby.GetCurrentLobby());
        }

        [Test]
        public void CreatedLobbyIsShownAsReady() {
            StartFlow(LobbyFlowData.CreateLobby(6, "Luka"));

            _lobbyService.SucceedCreate();

            Assert.AreEqual(LobbyStatus.Ready, _status.GetStatus());
            Assert.AreEqual("NEW123", _currentLobby.GetCurrentLobby().Code);
            CollectionAssert.AreEqual(new[] { LobbyStatus.Ready }, _view.RefreshedStatuses);
        }

        [UnityTest]
        public IEnumerator LeavingLeavesLobbyThenCloses() => UniTask.ToCoroutine(async () => {
            var run = StartFlow(LobbyFlowData.CreateLobby(6, "Luka"));
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
            var run = StartFlow(LobbyFlowData.CreateLobby(6, "Luka"));
            _lobbyService.SucceedCreate();
            _lobbyService.RaiseLobbyUpdated(FakeLobbyService.CreateLobbyState("NEW123", 6, "Luka", "Ana"));

            _view.ClickStart();
            var result = await run;

            Assert.IsTrue(result.ShouldStartMatch);
            Assert.IsTrue(result.IsHost);
            Assert.AreEqual(6, result.MaxPlayers);
            Assert.AreEqual(2, result.PlayerCount);
            Assert.AreEqual(LobbyStatus.Starting, _status.GetStatus());
            Assert.AreEqual(false, _lobbyService.LastInLobby);
            Assert.IsTrue(_lobbyService.IsMatchStartAnnounced);
            // The match locks the lobby once its network runs, not the lobby flow.
            Assert.IsFalse(_lobbyService.IsLocked);
            Assert.AreEqual(0, _lobbyService.LeaveCount);
            Assert.IsTrue(_view.IsClosed);
        });

        [Test]
        public void CopyPutsLobbyCodeOnClipboard() {
            StartFlow(LobbyFlowData.CreateLobby(6, "Luka"));
            _lobbyService.SucceedCreate();

            _view.ClickCopyCode();

            Assert.AreEqual("NEW123", _clipboard.Text);
        }

        [Test]
        public void StartClickIsIgnoredWhileHostIsAlone() {
            StartFlow(LobbyFlowData.CreateLobby(6, "Luka"));
            _lobbyService.SucceedCreate();

            _view.ClickStart();

            Assert.IsFalse(_view.IsClosed);
            Assert.AreEqual(LobbyStatus.Ready, _status.GetStatus());
        }

        [UnityTest]
        public IEnumerator JoinedPlayerEndsFlowWhenHostSharesJoinCode() => UniTask.ToCoroutine(async () => {
            var run = StartFlow(LobbyFlowData.JoinLobby("ABC123", "Luka"));
            _lobbyService.SucceedJoin(CreateJoinedLobby(string.Empty));

            _lobbyService.RaiseLobbyUpdated(CreateJoinedLobby("RELAY1"));
            var result = await run;

            Assert.IsTrue(result.ShouldStartMatch);
            Assert.IsFalse(result.IsHost);
            Assert.AreEqual(false, _lobbyService.LastInLobby);
            Assert.IsFalse(_lobbyService.IsMatchStartAnnounced);
            Assert.AreEqual(0, _lobbyService.LeaveCount);
        });

        [UnityTest]
        public IEnumerator ClientLeavesLobbyScreenAsSoonAsHostStarts() => UniTask.ToCoroutine(async () => {
            var run = StartFlow(LobbyFlowData.JoinLobby("ABC123", "Luka"));
            _lobbyService.SucceedJoin(CreateJoinedLobby(string.Empty));

            _lobbyService.RaiseLobbyUpdated(CreateStartingLobby(string.Empty));
            var result = await run;

            Assert.IsTrue(result.ShouldStartMatch);
            Assert.IsFalse(result.IsHost);
        });

        [Test]
        public void ReturningClientIgnoresLeftoverStartingFlag() {
            _lobbyService.CurrentLobby = CreateStartingLobby("RELAY1");

            StartFlow(LobbyFlowData.ReturnFromMatch("RELAY1", false));

            Assert.IsFalse(_view.IsClosed);
        }

        [Test]
        public void HostReturningFromMatchReopensLobby() {
            _lobbyService.CurrentLobby = CreateHostedLobby("RELAY1");

            StartFlow(LobbyFlowData.ReturnFromMatch(string.Empty, false));

            Assert.AreEqual(1, _lobbyService.EndMatchCount);
            Assert.AreEqual(1, _lobbyService.ReturnCount);
            Assert.AreEqual(true, _lobbyService.LastInLobby);
            Assert.AreEqual(LobbyStatus.Ready, _status.GetStatus());
            Assert.IsFalse(_view.IsClosed);
        }

        [UnityTest]
        public IEnumerator ReturningClientIgnoresUsedJoinCode() => UniTask.ToCoroutine(async () => {
            _lobbyService.CurrentLobby = CreateJoinedLobby("RELAY1");
            var run = StartFlow(LobbyFlowData.ReturnFromMatch("RELAY1", false));

            _lobbyService.RaiseLobbyUpdated(CreateJoinedLobby("RELAY1"));
            Assert.IsFalse(_view.IsClosed);
            Assert.AreEqual(0, _lobbyService.EndMatchCount);

            _lobbyService.RaiseLobbyUpdated(CreateJoinedLobby("RELAY2"));
            var result = await run;

            Assert.IsTrue(result.ShouldStartMatch);
        });

        [UnityTest]
        public IEnumerator RemovedPlayerSeesRemovedAndLeaveJustCloses() => UniTask.ToCoroutine(async () => {
            _lobbyService.IsStillMember = false;
            _lobbyService.CurrentLobby = CreateJoinedLobby("RELAY1");
            var run = StartFlow(LobbyFlowData.ReturnFromMatch("RELAY1", false));

            Assert.AreEqual(LobbyStatus.Removed, _status.GetStatus());
            Assert.AreSame(LobbyState.None, _currentLobby.GetCurrentLobby());

            _view.ClickLeave();
            var result = await run;

            Assert.IsFalse(result.ShouldStartMatch);
            Assert.AreEqual(0, _lobbyService.LeaveCount);
            Assert.IsTrue(_view.IsClosed);
        });

        [UnityTest]
        public IEnumerator HostWhoseReturnFailsLeavesTheLobby() => UniTask.ToCoroutine(async () => {
            _lobbyService.EndMatchThrows = true;
            _lobbyService.CurrentLobby = new LobbyState(true, "ABC123", 4, FakeLobbyService.CreatePlayers("Ana", "Luka"), true,
                "RELAY1", false);

            var isThrown = false;
            try {
                await StartFlow(LobbyFlowData.ReturnFromMatch("RELAY1", false));
            }
            catch (InvalidOperationException) {
                isThrown = true;
            }

            Assert.IsTrue(isThrown);
            Assert.AreEqual(1, _lobbyService.EndMatchCount);
            Assert.AreEqual(1, _lobbyService.LeaveCount);
            Assert.IsTrue(_view.IsClosed);
        });

        // The membership check failed, so whether the player is still in is unknown; leaving is the safe choice.
        [UnityTest]
        public IEnumerator ReturnThatFailsBeforeTheMembershipCheckLeavesTheLobby() => UniTask.ToCoroutine(async () => {
            _lobbyService.ReturnThrows = true;
            _lobbyService.CurrentLobby = CreateJoinedLobby("RELAY1");

            var isThrown = false;
            try {
                await StartFlow(LobbyFlowData.ReturnFromMatch("RELAY1", false));
            }
            catch (InvalidOperationException) {
                isThrown = true;
            }

            Assert.IsTrue(isThrown);
            Assert.AreEqual(1, _lobbyService.LeaveCount);
            Assert.IsTrue(_view.IsClosed);
        });

        [UnityTest]
        public IEnumerator LeavingStopsRemovingMissingPlayers() => UniTask.ToCoroutine(async () => {
            _lobbyService.CurrentLobby = new LobbyState(true, "ABC123", 4, new[] {
                new LobbyPlayer("host", "Luka", true, true), new LobbyPlayer("ana", "Ana", false, false)
            }, true, string.Empty, false);
            var run = StartFlow(LobbyFlowData.ReturnFromMatch(string.Empty, false));

            _view.ClickLeave();
            _lobbyService.CompleteLeave();
            await run;

            Assert.IsEmpty(_lobbyService.RemovedPlayerIds);
        });

        [Test]
        public void ReturningToAClosedLobbyShowsClosed() {
            _lobbyService.CurrentLobby = LobbyState.None;

            StartFlow(LobbyFlowData.ReturnFromMatch("RELAY1", false));

            Assert.AreEqual(LobbyStatus.Closed, _status.GetStatus());
        }

        [Test]
        public void FailedMatchStartShowsStartFailed() {
            _lobbyService.CurrentLobby = CreateHostedLobby(string.Empty);

            StartFlow(LobbyFlowData.ReturnFromMatch(string.Empty, true));

            Assert.AreEqual(LobbyStatus.StartFailed, _status.GetStatus());
        }

        [UnityTest]
        public IEnumerator LeavingDoesNotStartMatch() => UniTask.ToCoroutine(async () => {
            var run = StartFlow(LobbyFlowData.CreateLobby(6, "Luka"));
            _lobbyService.SucceedCreate();

            _view.ClickLeave();
            _lobbyService.CompleteLeave();
            var result = await run;

            Assert.IsFalse(result.ShouldStartMatch);
        });

        [UnityTest]
        public IEnumerator FailedCreateShowsFailedAndLeaveJustCloses() => UniTask.ToCoroutine(async () => {
            var run = StartFlow(LobbyFlowData.CreateLobby(6, "Luka"));

            _lobbyService.FailCreate();
            Assert.AreEqual(LobbyStatus.Failed, _status.GetStatus());

            _view.ClickLeave();
            await run;

            Assert.AreEqual(0, _lobbyService.LeaveCount);
            Assert.IsTrue(_view.IsClosed);
        });

        [UnityTest]
        public IEnumerator LeaveClickedWhileCreatingLeavesOnceCreated() => UniTask.ToCoroutine(async () => {
            var run = StartFlow(LobbyFlowData.CreateLobby(6, "Luka"));

            _view.ClickLeave();
            _lobbyService.SucceedCreate();

            Assert.AreEqual(1, _lobbyService.LeaveCount);
            _lobbyService.CompleteLeave();
            await run;
        });

        [Test]
        public void JoiningShowsJoiningWhileRequestIsPending() {
            StartFlow(LobbyFlowData.JoinLobby("ABC123", "Luka"));

            Assert.IsTrue(_view.IsRunning);
            Assert.AreEqual(LobbyStatus.Joining, _status.GetStatus());
            Assert.AreEqual("ABC123", _lobbyService.LastJoinCode);
            Assert.AreEqual("Luka", _lobbyService.LastJoinPlayerName);
            Assert.AreEqual(0, _lobbyService.CreateCount);
        }

        [Test]
        public void JoinedLobbyIsShownAsReady() {
            StartFlow(LobbyFlowData.JoinLobby("ABC123", "Luka"));

            _lobbyService.SucceedJoin(CreateJoinedLobby(string.Empty));

            Assert.AreEqual(LobbyStatus.Ready, _status.GetStatus());
            Assert.AreEqual("ABC123", _currentLobby.GetCurrentLobby().Code);
            CollectionAssert.AreEqual(new[] { LobbyStatus.Ready }, _view.RefreshedStatuses);
        }

        [UnityTest]
        public IEnumerator FailedJoinEndsFlowWithItsFailure() => UniTask.ToCoroutine(async () => {
            var run = StartFlow(LobbyFlowData.JoinLobby("ABC123", "Luka"));

            _lobbyService.FailJoin(LobbyJoinFailure.LobbyFull);
            var result = await run;

            Assert.AreEqual(LobbyJoinFailure.LobbyFull, result.JoinFailure);
            Assert.IsFalse(result.ShouldStartMatch);
            Assert.AreEqual(0, _lobbyService.LeaveCount);
            Assert.IsTrue(_view.IsClosed);
        });

        [UnityTest]
        public IEnumerator JoiningStartedMatchEndsFlowWithMatchInProgress() => UniTask.ToCoroutine(async () => {
            var run = StartFlow(LobbyFlowData.JoinLobby("ABC123", "Luka"));

            _lobbyService.SucceedJoin(CreateJoinedLobby("RELAY1"));
            _lobbyService.CompleteLeave();
            var result = await run;

            Assert.AreEqual(LobbyJoinFailure.MatchInProgress, result.JoinFailure);
            Assert.IsFalse(result.ShouldStartMatch);
            Assert.AreEqual(1, _lobbyService.LeaveCount);
        });

        [UnityTest]
        public IEnumerator LeaveClickedWhileJoiningLeavesOnceJoined() => UniTask.ToCoroutine(async () => {
            var run = StartFlow(LobbyFlowData.JoinLobby("ABC123", "Luka"));

            _view.ClickLeave();
            _lobbyService.SucceedJoin(CreateJoinedLobby(string.Empty));

            Assert.AreEqual(1, _lobbyService.LeaveCount);
            _lobbyService.CompleteLeave();
            var result = await run;

            Assert.AreEqual(LobbyJoinFailure.None, result.JoinFailure);
            Assert.IsFalse(result.ShouldStartMatch);
        });

        [Test]
        public void LobbyUpdatesRefreshTheView() {
            StartFlow(LobbyFlowData.CreateLobby(4, "Luka"));
            _lobbyService.SucceedCreate();
            var update = FakeLobbyService.CreateLobbyState("NEW123", 4, "Luka", "Ana");

            _lobbyService.RaiseLobbyUpdated(update);

            Assert.AreSame(update, _currentLobby.GetCurrentLobby());
            Assert.AreEqual(2, _view.RefreshedStatuses.Count);
        }

        [Test]
        public void InvalidLobbyUpdateIsIgnored() {
            StartFlow(LobbyFlowData.CreateLobby(4, "Luka"));
            _lobbyService.SucceedCreate();
            var lobby = _currentLobby.GetCurrentLobby();

            _lobbyService.RaiseLobbyUpdated(LobbyState.None);

            Assert.AreSame(lobby, _currentLobby.GetCurrentLobby());
            Assert.AreEqual(1, _view.RefreshedStatuses.Count);
        }

        [UnityTest]
        public IEnumerator UpdatesAfterFlowEndsAreIgnored() => UniTask.ToCoroutine(async () => {
            var run = StartFlow(LobbyFlowData.CreateLobby(4, "Luka"));
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
            var run = StartFlow(LobbyFlowData.CreateLobby(4, "Luka"));
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
            var run = StartFlow(LobbyFlowData.CreateLobby(4, "Luka"));

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
            return new LobbyState(true, "ABC123", 4, FakeLobbyService.CreatePlayers("Ana", "Luka"), false, matchJoinCode, false);
        }

        private static LobbyState CreateStartingLobby(string matchJoinCode) {
            return new LobbyState(true, "ABC123", 4, FakeLobbyService.CreatePlayers("Ana", "Luka"), false, matchJoinCode, true);
        }

        private static LobbyState CreateHostedLobby(string matchJoinCode) {
            return new LobbyState(true, "ABC123", 4, FakeLobbyService.CreatePlayers("Luka", "Ana"), true, matchJoinCode, false);
        }

        // LobbyFlow has no initial await, so the view is running as soon as this returns.
        private UniTask<LobbyFlowResult> StartFlow(LobbyFlowData data) {
            var flow = new LobbyFlow(data, _view, _lobbyService, new LobbyCreator(_lobbyService, _status, _currentLobby),
                new LobbyJoiner(_lobbyService, _status, _currentLobby), new LobbyStartRules(_status, _currentLobby),
                new MissingPlayerRemover(_lobbyService), _clipboard, _status, _currentLobby);
            return flow.RunAsync(_cancellation.Token).Preserve();
        }
    }
}
