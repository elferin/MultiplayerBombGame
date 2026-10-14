using Code.Lobby;
using NUnit.Framework;

namespace Code.Tests {

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
        [TestCase(true, LobbyStatus.Leaving, 2, false)]
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

        private LobbyState CreateLobby(bool isOwner, int playerCount) {
            var names = new string[playerCount];
            for (var i = 0; i < playerCount; i++) {
                names[i] = "Player" + i;
            }
            return new LobbyState(true, "XYZ", 4, names, isOwner, string.Empty);
        }
    }
}
