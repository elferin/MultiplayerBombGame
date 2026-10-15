using Code.Lobby;
using NUnit.Framework;

namespace Code.Tests.Lobby {

    public class LobbyStartRulesTests {

        private LobbyStatusHolder _status;
        private CurrentLobbyHolder _currentLobby;
        private LobbyStartRules _rules;

        [SetUp]
        public void SetUp() {
            _status = new LobbyStatusHolder();
            _currentLobby = new CurrentLobbyHolder();
            _rules = new LobbyStartRules(_status, _currentLobby);
        }

        [TestCase(true, LobbyStatus.Ready, 2, true)]
        [TestCase(true, LobbyStatus.StartFailed, 2, true)]
        [TestCase(true, LobbyStatus.Ready, 1, false)]
        [TestCase(false, LobbyStatus.Ready, 2, false)]
        [TestCase(true, LobbyStatus.Creating, 2, false)]
        [TestCase(true, LobbyStatus.Joining, 2, false)]
        [TestCase(true, LobbyStatus.Leaving, 2, false)]
        [TestCase(true, LobbyStatus.Starting, 2, false)]
        public void CanStart(bool isOwner, LobbyStatus status, int playerCount, bool expected) {
            _status.SetStatus(status);
            _currentLobby.SetCurrentLobby(CreateLobby(isOwner, playerCount));

            Assert.AreEqual(expected, _rules.CanStart());
        }

        [Test]
        public void OffersStartToAloneHostWithoutAllowingIt() {
            _status.SetStatus(LobbyStatus.Ready);
            _currentLobby.SetCurrentLobby(CreateLobby(true, 1));

            Assert.IsTrue(_rules.IsStartOffered());
            Assert.IsFalse(_rules.HasEnoughPlayers());
        }

        [Test]
        public void OffersNoStartWithoutALobby() {
            _status.SetStatus(LobbyStatus.Ready);

            Assert.IsFalse(_rules.IsStartOffered());
            Assert.IsFalse(_rules.CanStart());
        }

        [Test]
        public void CannotStartWhileSomeoneIsStillInTheMatch() {
            _status.SetStatus(LobbyStatus.Ready);
            _currentLobby.SetCurrentLobby(CreateLobbyWithPlayers(
                new LobbyPlayer("host", "Luka", true, true), new LobbyPlayer("ana", "Ana", false, false)));

            Assert.IsFalse(_rules.AreAllPlayersBack());
            Assert.IsFalse(_rules.CanStart());
        }

        [Test]
        public void IgnoresTheHostsOwnInLobbyFlag() {
            _status.SetStatus(LobbyStatus.Ready);
            _currentLobby.SetCurrentLobby(CreateLobbyWithPlayers(
                new LobbyPlayer("host", "Luka", false, true), new LobbyPlayer("ana", "Ana", true, false)));

            Assert.IsTrue(_rules.CanStart());
        }

        private LobbyState CreateLobbyWithPlayers(params LobbyPlayer[] players) {
            return new LobbyState(true, "XYZ", 4, players, true, string.Empty, false);
        }

        private LobbyState CreateLobby(bool isOwner, int playerCount) {
            var names = new string[playerCount];
            for (var i = 0; i < playerCount; i++) {
                names[i] = "Player" + i;
            }
            return new LobbyState(true, "XYZ", 4, FakeLobbyService.CreatePlayers(names), isOwner, string.Empty, false);
        }
    }
}
