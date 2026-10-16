using Code.BombTag;
using NUnit.Framework;

namespace Code.Tests.BombTag {

    public class BombTagViewDataGathererTests {

        private BombTagStatusHolder _status;
        private BombTagViewDataGatherer _gatherer;

        [SetUp]
        public void SetUp() {
            _status = new BombTagStatusHolder();
            _gatherer = new BombTagViewDataGatherer(_status);
        }

        [TestCase(BombTagPhase.Waiting, 0, "", "")]
        [TestCase(BombTagPhase.Countdown, 2, "", "2")]
        [TestCase(BombTagPhase.Playing, 0, "", "")]
        [TestCase(BombTagPhase.Finished, 0, "Ana", "Ana wins!")]
        [TestCase(BombTagPhase.Finished, 0, "", "Nobody wins!")]
        public void ShowsCountdownAndWinner(BombTagPhase phase, int countdownSeconds, string winnerName, string expected) {
            _status.SetState(new BombTagState(phase, countdownSeconds, winnerName));

            Assert.AreEqual(expected, _gatherer.Get().CenterText);
        }
    }
}
