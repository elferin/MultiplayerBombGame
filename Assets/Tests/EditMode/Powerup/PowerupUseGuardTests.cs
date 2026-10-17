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
            _guard.MarkSent(10f);

            Assert.IsFalse(_guard.CanUse(PowerupType.Dash));
        }

        [Test]
        public void HostAnsweringAllowsTheNextUse() {
            _guard.MarkSent(10f);

            _guard.OnHeldChanged();

            Assert.IsTrue(_guard.CanUse(PowerupType.Shield));
        }

        [Test]
        public void NoResendBeforeASecond() {
            _guard.MarkSent(10f);

            Assert.IsFalse(_guard.ShouldResend(10.9f));
        }

        [Test]
        public void NoAnswerResendsWithoutUsingAgain() {
            _guard.MarkSent(10f);

            Assert.IsTrue(_guard.ShouldResend(11f));
            Assert.IsFalse(_guard.CanUse(PowerupType.Dash));
        }

        [Test]
        public void ResendingWaitsAnotherSecond() {
            _guard.MarkSent(10f);
            _guard.MarkSent(11f);

            Assert.IsFalse(_guard.ShouldResend(11.5f));
            Assert.IsTrue(_guard.ShouldResend(12f));
        }

        [Test]
        public void HostAnsweringStopsResending() {
            _guard.MarkSent(10f);

            _guard.OnHeldChanged();

            Assert.IsFalse(_guard.ShouldResend(20f));
        }
    }
}
