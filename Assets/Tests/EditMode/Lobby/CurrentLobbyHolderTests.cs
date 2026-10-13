using Code.Lobby;
using NUnit.Framework;

namespace Code.Tests {

    public class CurrentLobbyHolderTests {

        [Test]
        public void StartsWithoutALobby() {
            Assert.AreSame(LobbyState.None, new CurrentLobbyHolder().GetCurrentLobby());
        }

        [Test]
        public void StoresLobby() {
            var holder = new CurrentLobbyHolder();
            var lobby = FakeLobbyService.CreateLobbyState("XYZ", 4, "Luka");

            holder.SetCurrentLobby(lobby);

            Assert.AreSame(lobby, holder.GetCurrentLobby());
        }

        [Test]
        public void StoresNullAsNoLobby() {
            var holder = new CurrentLobbyHolder();

            holder.SetCurrentLobby(null);

            Assert.AreSame(LobbyState.None, holder.GetCurrentLobby());
        }
    }
}
