using Code.Powerup;
using NUnit.Framework;

namespace Code.Tests.Powerup {

    public class PowerupUseGuardTests {

        private PowerupUseGuard _guard;

        [SetUp]
        public void SetUp() {
            _guard = new PowerupUseGuard();
        }

        [Test]
        public void NothingHeldCannotBeUsed() {
            Assert.IsFalse(_guard.CanUse(PowerupType.None, 0f));
            Assert.IsTrue(_guard.CanUse(PowerupType.Dash, 0f));
        }

        [Test]
        public void SecondPressWaitsForTheHost() {
            _guard.MarkSent(10f);

            Assert.IsFalse(_guard.CanUse(PowerupType.Dash, 10.5f));
        }

        [Test]
        public void HostAnsweringAllowsTheNextUse() {
            _guard.MarkSent(10f);

            _guard.OnHeldChanged();

            Assert.IsTrue(_guard.CanUse(PowerupType.Shield, 10.1f));
        }

        [Test]
        public void NoAnswerAllowsAnotherTryAfterASecond() {
            _guard.MarkSent(10f);

            Assert.IsFalse(_guard.CanUse(PowerupType.Dash, 10.9f));
            Assert.IsTrue(_guard.CanUse(PowerupType.Dash, 11f));
        }
    }
}
