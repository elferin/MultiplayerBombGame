using Code.Lobby;
using NUnit.Framework;

namespace Code.Tests {

    public class LobbyPlayerTests {

        [TestCase(false, false, true)]
        [TestCase(false, true, false)]
        [TestCase(true, false, false)]
        [TestCase(true, true, false)]
        public void HasNotReturned(bool isLocal, bool isInLobby, bool expected) {
            var player = new LobbyPlayer("ana", "Ana", isInLobby, isLocal);

            Assert.AreEqual(expected, player.HasNotReturned);
        }
    }
}
