using Code.BombTag;
using NUnit.Framework;

namespace Code.Tests.BombTag {

    public class BombTests {

        private FakeBombTagPlayers _players;
        private Bomb _bomb;

        [SetUp]
        public void SetUp() {
            _players = new FakeBombTagPlayers();
            _bomb = new Bomb(_players);
        }

        [Test]
        public void GiveToShowsTheBombOnThePlayer() {
            _bomb.GiveTo(1, 5f);

            Assert.IsTrue(_bomb.HasHolder());
            Assert.AreEqual(1, _bomb.GetHolderId());
            Assert.AreEqual(1, _players.GetHolder());
        }

        [Test]
        public void ExplodesWhenTheFuseRunsOut() {
            _bomb.GiveTo(1, 5f);

            _bomb.Tick(4.9f, false);
            Assert.IsFalse(_bomb.HasExploded());

            _bomb.Tick(0.2f, false);
            Assert.IsTrue(_bomb.HasExploded());
        }

        [Test]
        public void PassingKeepsTheFuseRunning() {
            _bomb.GiveTo(1, 5f);
            _bomb.Tick(4f, false);

            _bomb.PassTo(2);
            Assert.AreEqual(2, _players.GetHolder());

            _bomb.Tick(1.1f, false);
            Assert.IsTrue(_bomb.HasExploded());
        }

        [Test]
        public void PausedFuseDoesNotBurnButTheCooldownRuns() {
            _bomb.GiveTo(1, 5f);
            _bomb.PassTo(2);

            _bomb.Tick(10f, true);

            Assert.IsFalse(_bomb.HasExploded());
            Assert.IsTrue(_bomb.CanPass());

            _bomb.Tick(5.1f, false);
            Assert.IsTrue(_bomb.HasExploded());
        }

        [Test]
        public void CannotPassAgainUntilTheCooldownEnds() {
            _bomb.GiveTo(1, 5f);
            Assert.IsTrue(_bomb.CanPass());

            _bomb.PassTo(2);
            Assert.IsFalse(_bomb.CanPass());

            _bomb.Tick(0.4f, false);
            Assert.IsFalse(_bomb.CanPass());

            _bomb.Tick(0.2f, false);
            Assert.IsTrue(_bomb.CanPass());
        }

        [Test]
        public void RemoveTakesTheBombAway() {
            _bomb.GiveTo(1, 5f);

            _bomb.Remove();

            Assert.IsFalse(_bomb.HasHolder());
            Assert.AreEqual(0, _players.BombHolders.Count);
        }

        [Test]
        public void NobodyHoldingItMeansNoExplosion() {
            _bomb.Tick(100f, false);

            Assert.IsFalse(_bomb.HasExploded());
            Assert.IsFalse(_bomb.CanPass());
        }
    }
}
