using Code.GameFlow;
using Code.Lobby;
using Code.MainMenu;
using NUnit.Framework;

namespace Code.Tests.GameFlow {

    public class MainMenuToLobbyTests {

        private UsedJoinCodeHolder _usedJoinCode;
        private MainMenuToLobby _transition;

        [SetUp]
        public void SetUp() {
            _usedJoinCode = new UsedJoinCodeHolder();
            _transition = new MainMenuToLobby(_usedJoinCode);
        }

        [Test]
        public void HostCreatesALobby() {
            var data = _transition.GetLobbyData(new MainMenuFlowResult { ShouldHost = true, MaxPlayers = 6, PlayerName = "Luka" });

            Assert.AreEqual(LobbyEntry.Create, data.Entry);
            Assert.AreEqual(6, data.MaxPlayers);
            Assert.AreEqual("Luka", data.PlayerName);
        }

        [Test]
        public void JoinJoinsByCode() {
            var data = _transition.GetLobbyData(new MainMenuFlowResult { ShouldJoin = true, LobbyCode = "ABC123", PlayerName = "Luka" });

            Assert.AreEqual(LobbyEntry.Join, data.Entry);
            Assert.AreEqual("ABC123", data.LobbyCode);
            Assert.AreEqual("Luka", data.PlayerName);
        }

        [Test]
        public void NeitherHostingNorJoiningGivesNoLobby() {
            Assert.IsNull(_transition.GetLobbyData(new MainMenuFlowResult()));
        }

        [Test]
        public void ANewLobbyForgetsTheLastMatchesJoinCode() {
            _usedJoinCode.SetUsedJoinCode("RELAY1");

            _transition.GetLobbyData(new MainMenuFlowResult { ShouldHost = true, MaxPlayers = 4, PlayerName = "Luka" });

            Assert.AreEqual(string.Empty, _usedJoinCode.GetUsedJoinCode());
        }
    }
}
