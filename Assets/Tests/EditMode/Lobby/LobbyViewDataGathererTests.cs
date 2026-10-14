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
        [TestCase(LobbyStatus.Starting, "Starting match...")]
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
            _currentLobby.SetCurrentLobby(new LobbyState(true, "XYZ", 4, FakeLobbyService.CreatePlayers("Luka", "Ana"), false, string.Empty, false));

            var data = _gatherer.Get();

            Assert.AreEqual("Code: XYZ", data.LobbyCodeText);
            Assert.AreEqual("Players (2/4)\nLuka\nAna", data.PlayersText);
        }

        [TestCase(true, LobbyStatus.Ready, true)]
        [TestCase(false, LobbyStatus.Ready, false)]
        [TestCase(true, LobbyStatus.Leaving, false)]
        [TestCase(true, LobbyStatus.StartFailed, true)]
        [TestCase(true, LobbyStatus.Starting, false)]
        public void ShowsStartButtonOnlyToReadyHost(bool isOwner, LobbyStatus status, bool expected) {
            _status.SetStatus(status);
            _currentLobby.SetCurrentLobby(new LobbyState(true, "XYZ", 4, FakeLobbyService.CreatePlayers("Luka"), isOwner, string.Empty, false));

            Assert.AreEqual(expected, _gatherer.Get().ShowsStartButton);
        }

        [Test]
        public void GreysOutStartWhileHostIsAlone() {
            _status.SetStatus(LobbyStatus.Ready);
            _currentLobby.SetCurrentLobby(new LobbyState(true, "XYZ", 4, FakeLobbyService.CreatePlayers("Luka"), true, string.Empty, false));

            var data = _gatherer.Get();

            Assert.IsTrue(data.ShowsStartButton);
            Assert.IsFalse(data.CanStartMatch);
            Assert.AreEqual("Waiting for players...", data.StatusText);
        }

        [Test]
        public void EnablesStartWithTwoPlayers() {
            _status.SetStatus(LobbyStatus.Ready);
            _currentLobby.SetCurrentLobby(new LobbyState(true, "XYZ", 4, FakeLobbyService.CreatePlayers("Luka", "Ana"), true, string.Empty, false));

            var data = _gatherer.Get();

            Assert.IsTrue(data.CanStartMatch);
            Assert.AreEqual(string.Empty, data.StatusText);
        }

        [Test]
        public void WaitsForPlayersToReturnFromMatch() {
            _status.SetStatus(LobbyStatus.Ready);
            _currentLobby.SetCurrentLobby(new LobbyState(true, "XYZ", 4, new[] {
                new LobbyPlayer("host", "Luka", true, true), new LobbyPlayer("ana", "Ana", false, false)
            }, true, string.Empty, false));

            var data = _gatherer.Get();

            Assert.IsFalse(data.CanStartMatch);
            Assert.AreEqual("Waiting for players to return...", data.StatusText);
        }

        [Test]
        public void ShowsRemovedFromLobby() {
            _status.SetStatus(LobbyStatus.Removed);

            Assert.AreEqual("You were removed from the lobby", _gatherer.Get().StatusText);
        }

        [Test]
        public void ShowsCopyButtonOnlyWithALobby() {
            Assert.IsFalse(_gatherer.Get().ShowsCopyButton);

            _currentLobby.SetCurrentLobby(new LobbyState(true, "XYZ", 4, FakeLobbyService.CreatePlayers("Luka"), true, string.Empty, false));

            Assert.IsTrue(_gatherer.Get().ShowsCopyButton);
        }

        [Test]
        public void HidesStartButtonWithoutALobby() {
            _status.SetStatus(LobbyStatus.Ready);

            Assert.IsFalse(_gatherer.Get().ShowsStartButton);
        }
    }
}
