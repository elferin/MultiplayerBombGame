using Code.GameFlow;
using Code.Lobby;
using Code.Match;
using NUnit.Framework;

namespace Code.Tests.GameFlow {

    public class MatchToLobbyTests {

        private UsedJoinCodeHolder _usedJoinCode;
        private MatchToLobby _transition;

        [SetUp]
        public void SetUp() {
            _usedJoinCode = new UsedJoinCodeHolder();
            _transition = new MatchToLobby(_usedJoinCode);
        }

        [Test]
        public void ReturnsToTheLobbyAndRemembersTheMatchesJoinCode() {
            var data = _transition.GetLobbyData(new MatchFlowResult { UsedJoinCode = "RELAY1" });

            Assert.AreEqual(LobbyEntry.ReturnFromMatch, data.Entry);
            Assert.AreEqual("RELAY1", data.UsedJoinCode);
            Assert.AreEqual("RELAY1", _usedJoinCode.GetUsedJoinCode());
            Assert.IsFalse(data.MatchStartFailed);
        }

        [Test]
        public void MatchWithoutACodeKeepsTheLastOne() {
            _usedJoinCode.SetUsedJoinCode("RELAY1");

            var data = _transition.GetLobbyData(new MatchFlowResult { StartFailed = true });

            Assert.AreEqual("RELAY1", data.UsedJoinCode);
            Assert.IsTrue(data.MatchStartFailed);
        }
    }
}
