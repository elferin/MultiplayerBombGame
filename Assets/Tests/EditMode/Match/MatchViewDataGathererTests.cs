using Code.Match;
using NUnit.Framework;

namespace Code.Tests.Match {

    public class MatchViewDataGathererTests {

        private MatchStatusHolder _status;
        private MatchViewDataGatherer _gatherer;

        [SetUp]
        public void SetUp() {
            _status = new MatchStatusHolder();
            _gatherer = new MatchViewDataGatherer(_status);
        }

        [TestCase(MatchStatus.Starting, "Match is starting...")]
        [TestCase(MatchStatus.InProgress, "")]
        public void ShowsStatusText(MatchStatus status, string expected) {
            _status.SetStatus(status);

            Assert.AreEqual(expected, _gatherer.Get().StatusText);
        }
    }
}
