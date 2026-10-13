using System;
using System.Collections;
using System.Threading;
using Code.Lobby;
using Code.MainMenu;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Code.Tests {

    public class MainMenuFlowTests {

        private const int TIMEOUT_MS = 5000;

        private FakeMainMenuViewController _view;
        private FakeLobbyService _lobbyService;
        private FakePlayerNameStorage _storage;
        private MainMenuScreenLocationNavigator _screenLocation;
        private PlayerNameHolder _playerName;
        private HostLobbySettings _hostLobbySettings;
        private JoinLobbyState _joinLobby;
        private MainMenuFlow _flow;
        private CancellationTokenSource _cancellation;

        [SetUp]
        public void SetUp() {
            _view = new FakeMainMenuViewController();
            _lobbyService = new FakeLobbyService();
            _storage = new FakePlayerNameStorage("Saved");
            _screenLocation = new MainMenuScreenLocationNavigator();
            _playerName = new PlayerNameHolder(_storage);
            _hostLobbySettings = new HostLobbySettings();
            _joinLobby = new JoinLobbyState();
            var rules = new MainMenuLobbyRules(_playerName, _hostLobbySettings, _joinLobby);
            var lobbyJoiner = new LobbyJoiner(_lobbyService, _joinLobby);
            _flow = new MainMenuFlow(_view, lobbyJoiner, _screenLocation, _playerName, _hostLobbySettings, _joinLobby, rules);
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
        public IEnumerator JoinShowsJoiningThenReturnsJoinedLobby() => UniTask.ToCoroutine(async () => {
            var run = await StartFlow();
            _view.ClickJoin();
            _view.TypePlayerName(" Luka ");
            _view.TypeLobbyCode(" ABC123 ");
            var refreshCount = _view.RefreshCount;

            _view.ClickJoinLobby();

            // Refreshed while the request is still pending, so the view shows "Joining lobby...".
            Assert.AreEqual(JoinLobbyStatus.Joining, _joinLobby.GetJoinStatus());
            Assert.AreEqual(refreshCount + 1, _view.RefreshCount);
            Assert.AreEqual("ABC123", _lobbyService.LastJoinCode);
            Assert.AreEqual("Luka", _lobbyService.LastJoinPlayerName);

            _lobbyService.SucceedJoin();
            var result = await run;

            Assert.IsTrue(result.HasJoinedLobby);
            Assert.IsFalse(result.ShouldHost);
            Assert.AreEqual("Luka", result.PlayerName);
            Assert.AreEqual("Luka", _storage.SavedName);
            Assert.IsTrue(_view.IsClosed);
        });

        [UnityTest]
        public IEnumerator FailedJoinStaysOnJoinPanel() => UniTask.ToCoroutine(async () => {
            var run = await StartFlow();
            _view.ClickJoin();
            _view.TypeLobbyCode("WRONG");

            _view.ClickJoinLobby();
            _lobbyService.FailJoin();
            await UniTask.Yield();

            Assert.AreEqual(UniTaskStatus.Pending, run.Status);
            Assert.AreEqual(MainMenuScreen.JoinLobby, _screenLocation.GetCurrentScreen());
            Assert.AreEqual(JoinLobbyStatus.Failed, _joinLobby.GetJoinStatus());
            Assert.AreEqual("Saved", _storage.SavedName);

            _view.ClickQuit();
            await run;
        });

        [UnityTest]
        public IEnumerator CanRetryAfterFailedJoin() => UniTask.ToCoroutine(async () => {
            var run = await StartFlow();
            _view.ClickJoin();
            _view.TypeLobbyCode("WRONG");
            _view.ClickJoinLobby();
            _lobbyService.FailJoin();
            await UniTask.Yield();

            _view.TypeLobbyCode("ABC123");
            _view.ClickJoinLobby();
            _lobbyService.SucceedJoin();
            var result = await run;

            Assert.AreEqual(2, _lobbyService.JoinCount);
            Assert.IsTrue(result.HasJoinedLobby);
        });

        [UnityTest]
        public IEnumerator JoinAndBackAreIgnoredWhileJoining() => UniTask.ToCoroutine(async () => {
            var run = await StartFlow();
            _view.ClickJoin();
            _view.TypeLobbyCode("ABC123");
            _view.ClickJoinLobby();

            _view.ClickJoinLobby();
            _view.ClickBack();

            Assert.AreEqual(1, _lobbyService.JoinCount);
            Assert.AreEqual(MainMenuScreen.JoinLobby, _screenLocation.GetCurrentScreen());

            _lobbyService.SucceedJoin();
            await run;
        });

        [UnityTest]
        public IEnumerator JoinIsIgnoredWithoutCode() => UniTask.ToCoroutine(async () => {
            var run = await StartFlow();
            _view.ClickJoin();

            _view.ClickJoinLobby();

            Assert.AreEqual(0, _lobbyService.JoinCount);
            Assert.AreEqual(JoinLobbyStatus.Idle, _joinLobby.GetJoinStatus());

            _view.ClickQuit();
            await run;
        });

        // Starts the flow and waits until it listens to the view.
        private async UniTask<UniTask<MainMenuFlowResult>> StartFlow() {
            var run = _flow.RunAsync(_cancellation.Token).Preserve();
            await UniTask.WaitUntil(() => _view.IsRunning, cancellationToken: _cancellation.Token);
            return run;
        }
    }
}
