using Code.Lobby;
using Code.MainMenu;
using NUnit.Framework;

namespace Code.Tests {

    public class MainMenuViewDataGathererTests {

        private MainMenuScreenLocationNavigator _screenLocation;
        private PlayerNameHolder _playerName;
        private HostLobbySettings _hostLobbySettings;
        private JoinLobbyState _joinLobby;
        private MainMenuViewDataGatherer _gatherer;

        [SetUp]
        public void SetUp() {
            _screenLocation = new MainMenuScreenLocationNavigator();
            _playerName = new PlayerNameHolder(new FakePlayerNameStorage());
            _hostLobbySettings = new HostLobbySettings();
            _joinLobby = new JoinLobbyState();
            var rules = new MainMenuLobbyRules(_playerName, _hostLobbySettings, _joinLobby);
            _gatherer = new MainMenuViewDataGatherer(_screenLocation, _playerName, _hostLobbySettings, _joinLobby, rules);

            _playerName.SetPlayerName("Luka");
        }

        [TestCase(MainMenuScreen.MainMenu, true, false, false)]
        [TestCase(MainMenuScreen.HostLobby, false, true, false)]
        [TestCase(MainMenuScreen.JoinLobby, false, false, true)]
        public void ShowsOnlyTheCurrentScreen(MainMenuScreen screen, bool mainMenu, bool hostLobby, bool joinLobby) {
            _screenLocation.SetLocation(screen);

            var data = _gatherer.Get();

            Assert.AreEqual(mainMenu, data.ShowMainMenu);
            Assert.AreEqual(hostLobby, data.ShowHostLobby);
            Assert.AreEqual(joinLobby, data.ShowJoinLobby);
        }

        [Test]
        public void HostPanelShowsStateAndRange() {
            var hostLobby = _gatherer.Get().HostLobby;

            Assert.AreEqual("Luka", hostLobby.PlayerNameText);
            Assert.AreEqual("4", hostLobby.PlayerCountText);
            Assert.AreEqual("2-8 players", hostLobby.HintText);
            Assert.IsTrue(hostLobby.CanCreate);
        }

        [Test]
        public void HostPanelAsksForNameFirst() {
            _playerName.SetPlayerName(string.Empty);
            _hostLobbySettings.SetPlayerCount("99");

            var hostLobby = _gatherer.Get().HostLobby;

            Assert.AreEqual("Enter a name", hostLobby.HintText);
            Assert.IsFalse(hostLobby.CanCreate);
        }

        [Test]
        public void HostPanelAsksForValidCount() {
            _hostLobbySettings.SetPlayerCount("99");

            var hostLobby = _gatherer.Get().HostLobby;

            Assert.AreEqual("Enter a number from 2 to 8", hostLobby.HintText);
            Assert.IsFalse(hostLobby.CanCreate);
        }

        [TestCase(JoinLobbyStatus.Idle, "")]
        [TestCase(JoinLobbyStatus.Joining, "Joining lobby...")]
        [TestCase(JoinLobbyStatus.Failed, "Couldn't join. Check the code.")]
        [TestCase(JoinLobbyStatus.MatchInProgress, "Match already in progress")]
        public void JoinPanelShowsStatus(JoinLobbyStatus status, string expected) {
            _joinLobby.SetJoinStatus(status);

            Assert.AreEqual(expected, _gatherer.Get().JoinLobby.StatusText);
        }

        [Test]
        public void JoinPanelAsksForNameWhenIdle() {
            _playerName.SetPlayerName(string.Empty);

            Assert.AreEqual("Enter a name", _gatherer.Get().JoinLobby.StatusText);
        }

        [Test]
        public void JoinPanelShowsStateAndRules() {
            _joinLobby.SetLobbyCode("ABC123");

            var joinLobby = _gatherer.Get().JoinLobby;

            Assert.AreEqual("Luka", joinLobby.PlayerNameText);
            Assert.AreEqual("ABC123", joinLobby.CodeText);
            Assert.IsTrue(joinLobby.CanJoin);
            Assert.IsTrue(joinLobby.CanGoBack);
        }
    }
}
