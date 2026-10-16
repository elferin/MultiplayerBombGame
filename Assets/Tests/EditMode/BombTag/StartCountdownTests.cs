using Code.BombTag;
using NUnit.Framework;

namespace Code.Tests.BombTag {

    public class StartCountdownTests {

        private StartCountdown _countdown;

        [SetUp]
        public void SetUp() {
            _countdown = new StartCountdown();
            _countdown.Start();
        }

        [Test]
        public void StartsAtThreeSeconds() {
            Assert.AreEqual(3, _countdown.GetWholeSecondsLeft());
            Assert.IsFalse(_countdown.IsDone());
        }

        [TestCase(0.1f, 3)]
        [TestCase(1f, 2)]
        [TestCase(2.9f, 1)]
        public void RoundsSecondsUp(float elapsed, int expected) {
            _countdown.Tick(elapsed);

            Assert.AreEqual(expected, _countdown.GetWholeSecondsLeft());
        }

        [Test]
        public void IsDoneAtZeroAndNeverShowsLessThanZero() {
            _countdown.Tick(3.5f);

            Assert.IsTrue(_countdown.IsDone());
            Assert.AreEqual(0, _countdown.GetWholeSecondsLeft());
        }
    }
}
