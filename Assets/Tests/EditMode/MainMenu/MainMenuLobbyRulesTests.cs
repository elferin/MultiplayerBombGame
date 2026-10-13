using Code.Lobby;
using Code.MainMenu;
using NUnit.Framework;

namespace Code.Tests {

    public class MainMenuLobbyRulesTests {

        private PlayerNameHolder _playerName;
        private HostLobbySettings _hostLobbySettings;
        private JoinLobbyState _joinLobby;
        private MainMenuLobbyRules _rules;

        [SetUp]
        public void SetUp() {
            _playerName = new PlayerNameHolder(new FakePlayerNameStorage());
            _hostLobbySettings = new HostLobbySettings();
            _joinLobby = new JoinLobbyState();
            _rules = new MainMenuLobbyRules(_playerName, _hostLobbySettings, _joinLobby);

            _playerName.SetPlayerName("Luka");
            _joinLobby.SetLobbyCode("ABC123");
        }

        [Test]
        public void CanCreateWithNameAndValidCount() {
            Assert.IsTrue(_rules.CanCreateLobby());
        }

        [Test]
        public void CannotCreateWithoutName() {
            _playerName.SetPlayerName(" ");

            Assert.IsFalse(_rules.CanCreateLobby());
        }

        [Test]
        public void CannotCreateWithInvalidCount() {
            _hostLobbySettings.SetPlayerCount("9");

            Assert.IsFalse(_rules.CanCreateLobby());
        }

        [Test]
        public void CanJoinWithNameAndCode() {
            Assert.IsTrue(_rules.CanJoinLobby());
        }

        [Test]
        public void CannotJoinWithoutName() {
            _playerName.SetPlayerName(string.Empty);

            Assert.IsFalse(_rules.CanJoinLobby());
        }

        [Test]
        public void CannotJoinWithoutCode() {
            _joinLobby.SetLobbyCode("  ");

            Assert.IsFalse(_rules.CanJoinLobby());
        }

        [Test]
        public void CannotJoinOrGoBackWhileJoining() {
            _joinLobby.SetJoinStatus(JoinLobbyStatus.Joining);

            Assert.IsFalse(_rules.CanJoinLobby());
            Assert.IsFalse(_rules.CanGoBack());
        }

        [Test]
        public void CanRetryAndGoBackAfterFailedJoin() {
            _joinLobby.SetJoinStatus(JoinLobbyStatus.Failed);

            Assert.IsTrue(_rules.CanJoinLobby());
            Assert.IsTrue(_rules.CanGoBack());
        }
    }
}
