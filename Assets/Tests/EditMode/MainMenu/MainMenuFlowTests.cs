using System;
using System.Collections;
using System.Threading;
using Code.Lobby;
using Code.MainMenu;
using Code.Profile;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Code.Tests {

    public class MainMenuFlowTests {

        private const int TIMEOUT_MS = 5000;

        private FakeMainMenuViewController _view;
        private FakePlayerNameStorage _storage;
        private MainMenuScreenLocationNavigator _screenLocation;
        private PlayerNameHolder _playerName;
        private HostLobbySettings _hostLobbySettings;
        private JoinLobbyState _joinLobby;
        private FakeClipboard _clipboard;
        private MainMenuLobbyRules _rules;
        private CancellationTokenSource _cancellation;

        [SetUp]
        public void SetUp() {
            _view = new FakeMainMenuViewController();
            _storage = new FakePlayerNameStorage("Saved");
            _screenLocation = new MainMenuScreenLocationNavigator();
            _playerName = new PlayerNameHolder(_storage);
            _hostLobbySettings = new HostLobbySettings();
            _joinLobby = new JoinLobbyState();
            _clipboard = new FakeClipboard();
            _rules = new MainMenuLobbyRules(_playerName, _hostLobbySettings, _joinLobby);
            _cancellation = new CancellationTokenSource(TIMEOUT_MS);
        }

        [TearDown]
        public void TearDown() {
            _cancellation.Cancel();
            _cancellation.Dispose();
        }

        [UnityTest]
        public IEnumerator OpensOnMainMenuWithSavedName() => UniTask.ToCoroutine(async () => {
            _screenLocation.SetLocation(MainMenuScreen.HostLobby);

            var run = await StartFlow();

            Assert.AreEqual(MainMenuScreen.MainMenu, _screenLocation.GetCurrentScreen());
            Assert.AreEqual("Saved", _playerName.GetPlayerName());

            _view.ClickQuit();
            await run;
        });

        [UnityTest]
        public IEnumerator QuitEndsFlowAndClosesView() => UniTask.ToCoroutine(async () => {
            var run = await StartFlow();

            _view.ClickQuit();
            var result = await run;

            Assert.IsTrue(result.ShouldQuit);
            Assert.IsFalse(result.ShouldHost);
            Assert.IsTrue(_view.IsClosed);
        });

        [UnityTest]
        public IEnumerator CreateLobbyReturnsHostSettingsAndSavesName() => UniTask.ToCoroutine(async () => {
            var run = await StartFlow();

            _view.ClickHost();
            _view.TypePlayerName("  Luka ");
            _view.TypePlayerCount("6");
            _view.ClickCreateLobby();
            var result = await run;

            Assert.IsTrue(result.ShouldHost);
            Assert.AreEqual(6, result.MaxPlayers);
            Assert.AreEqual("Luka", result.PlayerName);
            Assert.AreEqual("Luka", _storage.SavedName);
        });

        [UnityTest]
        public IEnumerator CreateLobbyIsIgnoredWithInvalidCount() => UniTask.ToCoroutine(async () => {
            var run = await StartFlow();

            _view.ClickHost();
            _view.TypePlayerCount("1");
            _view.ClickCreateLobby();

            Assert.AreEqual(UniTaskStatus.Pending, run.Status);

            _view.ClickQuit();
            await run;
        });

        [UnityTest]
        public IEnumerator HostAndBackNavigateAndBackSavesName() => UniTask.ToCoroutine(async () => {
            var run = await StartFlow();

            _view.ClickHost();
            Assert.AreEqual(MainMenuScreen.HostLobby, _screenLocation.GetCurrentScreen());

            _view.TypePlayerName("Ana");
            _view.ClickBack();
            Assert.AreEqual(MainMenuScreen.MainMenu, _screenLocation.GetCurrentScreen());
            Assert.AreEqual("Ana", _storage.SavedName);

            _view.ClickQuit();
            await run;
        });

        [UnityTest]
        public IEnumerator OpeningJoinPanelClearsFailedStatus() => UniTask.ToCoroutine(async () => {
            _joinLobby.SetJoinStatus(JoinLobbyStatus.Failed);
            var run = await StartFlow();

            _view.ClickJoin();

            Assert.AreEqual(MainMenuScreen.JoinLobby, _screenLocation.GetCurrentScreen());
            Assert.AreEqual(JoinLobbyStatus.Idle, _joinLobby.GetJoinStatus());

            _view.ClickQuit();
            await run;
        });

        [UnityTest]
        public IEnumerator TypingCodeClearsFailedStatus() => UniTask.ToCoroutine(async () => {
            var run = await StartFlow();
            _view.ClickJoin();
            _joinLobby.SetJoinStatus(JoinLobbyStatus.Failed);

            _view.TypeLobbyCode("XYZ");

            Assert.AreEqual("XYZ", _joinLobby.GetLobbyCode());
            Assert.AreEqual(JoinLobbyStatus.Idle, _joinLobby.GetJoinStatus());

            _view.ClickQuit();
            await run;
        });

        [UnityTest]
        public IEnumerator TypingCodeClearsLobbyFullStatus() => UniTask.ToCoroutine(async () => {
            var run = await StartFlow();
            _view.ClickJoin();
            _joinLobby.SetJoinStatus(JoinLobbyStatus.LobbyFull);

            _view.TypeLobbyCode("XYZ");

            Assert.AreEqual(JoinLobbyStatus.Idle, _joinLobby.GetJoinStatus());

            _view.ClickQuit();
            await run;
        });

        [UnityTest]
        public IEnumerator PasteSetsTrimmedUpperCaseCodeAndClearsFailure() => UniTask.ToCoroutine(async () => {
            var run = await StartFlow();
            _view.ClickJoin();
            _joinLobby.SetJoinStatus(JoinLobbyStatus.Failed);
            _clipboard.Text = "  abc123\n";

            _view.ClickPasteCode();

            Assert.AreEqual("ABC123", _joinLobby.GetLobbyCode());
            Assert.AreEqual(JoinLobbyStatus.Idle, _joinLobby.GetJoinStatus());

            _view.ClickQuit();
            await run;
        });

        [UnityTest]
        public IEnumerator InputChangesRefreshTheView() => UniTask.ToCoroutine(async () => {
            var run = await StartFlow();
            var refreshCount = _view.RefreshCount;

            _view.TypePlayerName("Ana");
            _view.TypePlayerCount("3");
            _view.TypeLobbyCode("XYZ");

            Assert.AreEqual(refreshCount + 3, _view.RefreshCount);

            _view.ClickQuit();
            await run;
        });

        [UnityTest]
        public IEnumerator CancellingEndsFlowAndClosesView() => UniTask.ToCoroutine(async () => {
            var run = await StartFlow();

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
        });

        [UnityTest]
        public IEnumerator JoinEndsFlowWithTrimmedCodeAndName() => UniTask.ToCoroutine(async () => {
            var run = await StartFlow();
            _view.ClickJoin();
            _view.TypePlayerName(" Luka ");
            _view.TypeLobbyCode(" ABC123 ");

            _view.ClickJoinLobby();
            var result = await run;

            Assert.IsTrue(result.ShouldJoin);
            Assert.IsFalse(result.ShouldHost);
            Assert.AreEqual("ABC123", result.LobbyCode);
            Assert.AreEqual("Luka", result.PlayerName);
            Assert.AreEqual("Luka", _storage.SavedName);
            Assert.IsTrue(_view.IsClosed);
        });

        [UnityTest]
        public IEnumerator JoinIsIgnoredWithoutCode() => UniTask.ToCoroutine(async () => {
            var run = await StartFlow();
            _view.ClickJoin();

            _view.ClickJoinLobby();

            Assert.AreEqual(UniTaskStatus.Pending, run.Status);
            Assert.AreEqual(JoinLobbyStatus.Idle, _joinLobby.GetJoinStatus());

            _view.ClickQuit();
            await run;
        });

        [UnityTest]
        public IEnumerator OpensOnJoinPanelAfterFailedJoin() => UniTask.ToCoroutine(async () => {
            await AssertOpensJoinPanelWith(LobbyJoinFailure.Other, JoinLobbyStatus.Failed);
        });

        [UnityTest]
        public IEnumerator OpensOnJoinPanelAfterJoiningFullLobby() => UniTask.ToCoroutine(async () => {
            await AssertOpensJoinPanelWith(LobbyJoinFailure.LobbyFull, JoinLobbyStatus.LobbyFull);
        });

        [UnityTest]
        public IEnumerator OpensOnJoinPanelAfterJoiningStartedMatch() => UniTask.ToCoroutine(async () => {
            await AssertOpensJoinPanelWith(LobbyJoinFailure.MatchInProgress, JoinLobbyStatus.MatchInProgress);
        });

        // The code the player typed stays, so they can fix it.
        private async UniTask AssertOpensJoinPanelWith(LobbyJoinFailure failure, JoinLobbyStatus expected) {
            _joinLobby.SetLobbyCode("WRONG");

            var run = await StartFlow(MainMenuFlowData.JoinFailed(failure));

            Assert.AreEqual(MainMenuScreen.JoinLobby, _screenLocation.GetCurrentScreen());
            Assert.AreEqual(expected, _joinLobby.GetJoinStatus());
            Assert.AreEqual("WRONG", _joinLobby.GetLobbyCode());

            _view.ClickQuit();
            await run;
        }

        // Starts the flow and waits until it listens to the view.
        private async UniTask<UniTask<MainMenuFlowResult>> StartFlow(MainMenuFlowData data = null) {
            var flow = new MainMenuFlow(data ?? MainMenuFlowData.Default(), _view, _screenLocation, _playerName, _hostLobbySettings,
                _joinLobby, _rules, _clipboard);
            var run = flow.RunAsync(_cancellation.Token).Preserve();
            await UniTask.WaitUntil(() => _view.IsRunning, cancellationToken: _cancellation.Token);
            return run;
        }
    }
}
