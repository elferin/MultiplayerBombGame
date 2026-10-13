using Code.Match;
using NUnit.Framework;

namespace Code.Tests {

    public class MatchViewDataGathererTests {

        private MatchStatusHolder _status;
        private MatchViewDataGatherer _gatherer;

        [SetUp]
        public void SetUp() {
            _status = new MatchStatusHolder();
            _gatherer = new MatchViewDataGatherer(_status);
        }

        [TestCase(true, MatchStatus.Starting, "Starting match...")]
        [TestCase(false, MatchStatus.Starting, "Joining match...")]
        [TestCase(true, MatchStatus.InProgress, "")]
        [TestCase(false, MatchStatus.InProgress, "")]
        public void ShowsStatusText(bool isHost, MatchStatus status, string expected) {
            _status.SetIsHost(isHost);
            _status.SetStatus(status);

            Assert.AreEqual(expected, _gatherer.Get().StatusText);
        }

        [TestCase(true, MatchStatus.InProgress, true)]
        [TestCase(false, MatchStatus.InProgress, false)]
        [TestCase(true, MatchStatus.Starting, false)]
        public void ShowsEndButtonOnlyToHostDuringMatch(bool isHost, MatchStatus status, bool expected) {
            _status.SetIsHost(isHost);
            _status.SetStatus(status);

            Assert.AreEqual(expected, _gatherer.Get().ShowsEndButton);
        }
    }
}
