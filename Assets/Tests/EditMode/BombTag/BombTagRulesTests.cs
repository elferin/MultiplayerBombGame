using Code.BombTag;
using Code.Tests.Util;
using NUnit.Framework;
using UnityEngine;

namespace Code.Tests.BombTag {

    // Each rule on players and a bomb set up directly, without running a round.
    public class BombTagRulesTests {

        private FakeBombTagPlayers _players;
        private FakeRandom _random;
        private AlivePlayers _alivePlayers;
        private Bomb _bomb;
        private BombTagRules _rules;

        [SetUp]
        public void SetUp() {
            _players = new FakeBombTagPlayers();
            _random = new FakeRandom();
            _alivePlayers = new AlivePlayers(_players);
            _bomb = new Bomb(_players);
            _rules = new BombTagRules(_alivePlayers, _bomb, _random);

            // Far enough apart that nobody touches.
            _players.Add(1, "Ana", new Vector3(0f, 0f, 0f));
            _players.Add(2, "Luka", new Vector3(5f, 0f, 0f));
            _players.Add(3, "Mia", new Vector3(10f, 0f, 0f));
            _alivePlayers.TakeSpawnedPlayers();
        }

        [Test]
        public void GivesABombToARandomPlayerWithAFuseOfFiveToTenSeconds() {
            _random.IntOffset = 2;
            _random.FloatFraction = 1f;

            Assert.IsFalse(_rules.Apply());

            Assert.AreEqual(3, _players.GetHolder());
            _bomb.Tick(9.9f, false);
            Assert.IsFalse(_bomb.HasExploded());
            _bomb.Tick(0.2f, false);
            Assert.IsTrue(_bomb.HasExploded());
        }

        [Test]
        public void PassesTheBombToATouchedPlayer() {
            _bomb.GiveTo(1, 5f);
            Move(2, new Vector3(1f, 0f, 0f));

            _rules.Apply();

            Assert.AreEqual(2, _players.GetHolder());
        }

        [Test]
        public void DoesNotPassToPlayersWhoAreNotTouching() {
            _bomb.GiveTo(1, 5f);
            Move(2, new Vector3(1.2f, 0f, 0f));

            _rules.Apply();

            Assert.AreEqual(1, _players.GetHolder());
        }

        [Test]
        public void DoesNotPassToAShieldedPlayer() {
            _bomb.GiveTo(1, 5f);
            _players.SetShielded(2, true);
            Move(2, new Vector3(1f, 0f, 0f));

            _rules.Apply();

            Assert.AreEqual(1, _players.GetHolder());
        }

        [Test]
        public void ShieldedHolderCanStillPassTheBomb() {
            _bomb.GiveTo(1, 5f);
            _players.SetShielded(1, true);
            Move(2, new Vector3(1f, 0f, 0f));

            _rules.Apply();

            Assert.AreEqual(2, _players.GetHolder());
        }

        [Test]
        public void NewBombSkipsShieldedPlayers() {
            _players.SetShielded(1, true);
            _alivePlayers.Refresh();

            _rules.Apply();

            Assert.AreEqual(2, _players.GetHolder());
        }

        [Test]
        public void NewBombGoesToAShieldedPlayerWhenEveryoneIsShielded() {
            _players.SetShielded(1, true);
            _players.SetShielded(2, true);
            _players.SetShielded(3, true);
            _alivePlayers.Refresh();

            _rules.Apply();

            Assert.AreEqual(1, _players.GetHolder());
        }

        [Test]
        public void DoesNotPassBackUntilTheCooldownEnds() {
            _bomb.GiveTo(1, 5f);
            Move(2, new Vector3(1f, 0f, 0f));
            _rules.Apply();

            _rules.Apply();
            Assert.AreEqual(2, _players.GetHolder());

            _bomb.Tick(0.6f, false);
            _rules.Apply();
            Assert.AreEqual(1, _players.GetHolder());
        }

        [Test]
        public void ExplosionEliminatesOnlyTheHolderAndGivesANewBomb() {
            _bomb.GiveTo(1, 5f);
            _bomb.Tick(5.1f, false);

            Assert.IsFalse(_rules.Apply());

            CollectionAssert.AreEqual(new[] { 1 }, _players.Eliminated);
            Assert.AreEqual(2, _players.GetHolder());
        }

        [Test]
        public void ExplosionWithTwoLeftEndsTheMatch() {
            _alivePlayers.Eliminate(3);
            _bomb.GiveTo(1, 5f);
            _bomb.Tick(5.1f, false);

            Assert.IsTrue(_rules.Apply());

            Assert.AreEqual("Luka", _alivePlayers.GetWinnerName());
            Assert.AreEqual(0, _players.BombHolders.Count);
        }

        [Test]
        public void WinnerHoldingTheBombLosesIt() {
            _bomb.GiveTo(1, 5f);
            _players.Remove(2);
            _players.Remove(3);
            _alivePlayers.Refresh();

            Assert.IsTrue(_rules.Apply());

            Assert.AreEqual(0, _players.BombHolders.Count);
        }

        // The rules read positions through AlivePlayers, which takes them on Refresh.
        private void Move(int id, Vector3 position) {
            _players.Move(id, position);
            _alivePlayers.Refresh();
        }
    }
}
