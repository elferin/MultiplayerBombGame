using Code.Lobby;
using NUnit.Framework;

namespace Code.Tests {

    public class JoinLobbyStateTests {

        private JoinLobbyState _state;

        [SetUp]
        public void SetUp() {
            _state = new JoinLobbyState();
        }

        [Test]
        public void StartsEmptyAndIdle() {
            Assert.AreEqual(string.Empty, _state.GetLobbyCode());
            Assert.AreEqual(JoinLobbyStatus.Idle, _state.GetJoinStatus());
        }

        [Test]
        public void StoresLobbyCode() {
            _state.SetLobbyCode("ABC123");

            Assert.AreEqual("ABC123", _state.GetLobbyCode());
        }

        [Test]
        public void StoresNullCodeAsEmpty() {
            _state.SetLobbyCode(null);

            Assert.AreEqual(string.Empty, _state.GetLobbyCode());
        }

        [Test]
        public void StoresJoinStatus() {
            _state.SetJoinStatus(JoinLobbyStatus.Failed);

            Assert.AreEqual(JoinLobbyStatus.Failed, _state.GetJoinStatus());
        }
    }
}
