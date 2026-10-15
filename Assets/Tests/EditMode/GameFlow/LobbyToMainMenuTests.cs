using Code.GameFlow;
using Code.Lobby;
using NUnit.Framework;

namespace Code.Tests.GameFlow {

    public class LobbyToMainMenuTests {

        private LobbyToMainMenu _transition;

        [SetUp]
        public void SetUp() {
            _transition = new LobbyToMainMenu();
        }

        [Test]
        public void LeavingOpensTheMainMenu() {
            var data = _transition.GetMainMenuData(new LobbyFlowResult());

            Assert.AreEqual(LobbyJoinFailure.None, data.JoinFailure);
        }

        [Test]
        public void FailedJoinOpensTheJoinPanelWithItsReason() {
            var data = _transition.GetMainMenuData(new LobbyFlowResult { JoinFailure = LobbyJoinFailure.LobbyFull });

            Assert.AreEqual(LobbyJoinFailure.LobbyFull, data.JoinFailure);
        }
    }
}
