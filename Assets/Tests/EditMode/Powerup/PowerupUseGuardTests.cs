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
            Assert.IsFalse(_guard.CanUse(PowerupType.None));
            Assert.IsTrue(_guard.CanUse(PowerupType.Dash));
        }

        [Test]
        public void SecondPressWaitsForTheHost() {
            _guard.MarkSent();

            Assert.IsFalse(_guard.CanUse(PowerupType.Dash));
        }

        [Test]
        public void HostAnsweringAllowsTheNextUse() {
            _guard.MarkSent();

            _guard.OnHeldChanged();

            Assert.IsTrue(_guard.CanUse(PowerupType.Shield));
        }
    }
}
