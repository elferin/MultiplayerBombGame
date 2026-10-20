using System.Collections.Generic;
using Code.MatchMenu;
using NUnit.Framework;

namespace Code.Tests.MatchMenu {

    public class MatchMenuStateHolderTests {

        private MatchMenuStateHolder _state;
        private List<bool> _changes;

        [SetUp]
        public void SetUp() {
            _state = new MatchMenuStateHolder();
            _changes = new List<bool>();
            _state.OpenChanged += _changes.Add;
        }

        [Test]
        public void StartsClosed() {
            Assert.IsFalse(_state.IsOpen());
        }

        [Test]
        public void AnnouncesOpeningAndClosing() {
            _state.SetOpen(true);
            _state.SetOpen(false);

            CollectionAssert.AreEqual(new[] { true, false }, _changes);
            Assert.IsFalse(_state.IsOpen());
        }

        [Test]
        public void DoesNotAnnounceTheSameValueAgain() {
            _state.SetOpen(true);
            _state.SetOpen(true);
            _state.SetOpen(false);
            _state.SetOpen(false);

            CollectionAssert.AreEqual(new[] { true, false }, _changes);
        }
    }
}
