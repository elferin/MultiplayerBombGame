using Code.Lobby;
using NUnit.Framework;

namespace Code.Tests {

    public class LobbyViewDataGathererTests {

        private LobbyStatusHolder _status;
        private CurrentLobbyHolder _currentLobby;
        private LobbyViewDataGatherer _gatherer;

        [SetUp]
        public void SetUp() {
            _status = new LobbyStatusHolder();
            _currentLobby = new CurrentLobbyHolder();
            _gatherer = new LobbyViewDataGatherer(_status, _currentLobby, new LobbyStartRules(_status, _currentLobby));
        }

        [TestCase(LobbyStatus.Creating, "Creating lobby...")]
        [TestCase(LobbyStatus.Failed, "Failed to create lobby")]
        [TestCase(LobbyStatus.Leaving, "Leaving lobby...")]
        [TestCase(LobbyStatus.StartFailed, "Failed to start match")]
        [TestCase(LobbyStatus.Ready, "")]
        public void ShowsStatusText(LobbyStatus status, string expected) {
            _status.SetStatus(status);

            Assert.AreEqual(expected, _gatherer.Get().StatusText);
        }

        [Test]
        public void ShowsNoLobbyDetailsWithoutALobby() {
            var data = _gatherer.Get();

            Assert.AreEqual(string.Empty, data.LobbyCodeText);
            Assert.AreEqual(string.Empty, data.PlayersText);
        }

        [Test]
        public void ShowsLobbyCodeAndPlayers() {
            _status.SetStatus(LobbyStatus.Ready);
            _currentLobby.SetCurrentLobby(new LobbyState(true, "XYZ", 4, FakeLobbyService.CreatePlayers("Luka", "Ana"), false, string.Empty));

            var data = _gatherer.Get();

            Assert.AreEqual("Code: XYZ", data.LobbyCodeText);
            Assert.AreEqual("Players (2/4)\nLuka\nAna", data.PlayersText);
        }

        [TestCase(true, LobbyStatus.Ready, true)]
        [TestCase(false, LobbyStatus.Ready, false)]
        [TestCase(true, LobbyStatus.Leaving, false)]
        [TestCase(true, LobbyStatus.StartFailed, true)]
        public void ShowsStartButtonOnlyToReadyHost(bool isOwner, LobbyStatus status, bool expected) {
            _status.SetStatus(status);
            _currentLobby.SetCurrentLobby(new LobbyState(true, "XYZ", 4, FakeLobbyService.CreatePlayers("Luka"), isOwner, string.Empty));

            Assert.AreEqual(expected, _gatherer.Get().ShowsStartButton);
        }

        [Test]
        public void GreysOutStartWhileHostIsAlone() {
            _status.SetStatus(LobbyStatus.Ready);
            _currentLobby.SetCurrentLobby(new LobbyState(true, "XYZ", 4, FakeLobbyService.CreatePlayers("Luka"), true, string.Empty));

            var data = _gatherer.Get();

            Assert.IsTrue(data.ShowsStartButton);
            Assert.IsFalse(data.CanStartMatch);
            Assert.AreEqual("Waiting for players...", data.StatusText);
        }

        [Test]
        public void EnablesStartWithTwoPlayers() {
            _status.SetStatus(LobbyStatus.Ready);
            _currentLobby.SetCurrentLobby(new LobbyState(true, "XYZ", 4, FakeLobbyService.CreatePlayers("Luka", "Ana"), true, string.Empty));

            var data = _gatherer.Get();

            Assert.IsTrue(data.CanStartMatch);
            Assert.AreEqual(string.Empty, data.StatusText);
        }

        [Test]
        public void HidesStartButtonWithoutALobby() {
            _status.SetStatus(LobbyStatus.Ready);

            Assert.IsFalse(_gatherer.Get().ShowsStartButton);
        }
    }
}
