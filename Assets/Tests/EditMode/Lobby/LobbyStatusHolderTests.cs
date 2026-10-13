using Code.Lobby;
using NUnit.Framework;

namespace Code.Tests {

    public class LobbyStatusHolderTests {

        [Test]
        public void StartsCreating() {
            Assert.AreEqual(LobbyStatus.Creating, new LobbyStatusHolder().GetStatus());
        }

        [Test]
        public void StoresStatus() {
            var holder = new LobbyStatusHolder();

            holder.SetStatus(LobbyStatus.Leaving);

            Assert.AreEqual(LobbyStatus.Leaving, holder.GetStatus());
        }
    }
}
