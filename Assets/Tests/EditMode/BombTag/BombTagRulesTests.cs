using Code.BombTag;
using Code.Tests.Util;
using NUnit.Framework;
using UnityEngine;

namespace Code.Tests.BombTag {

    public class BombTagRulesTests {

        // FloatFraction 0 gives the shortest fuse.
        private const float SHORTEST_FUSE = 5f;
        private const float COUNTDOWN = 3f;

        private FakeBombTagPlayers _players;
        private FakeRandom _random;
        private BombTagRules _rules;

        [SetUp]
        public void SetUp() {
            _players = new FakeBombTagPlayers();
            _random = new FakeRandom();
            _rules = new BombTagRules(_players, _random);

            // Far enough apart that nobody touches.
            _players.Add(1, "Ana", new Vector3(0f, 0f, 0f));
            _players.Add(2, "Luka", new Vector3(5f, 0f, 0f));
            _players.Add(3, "Mia", new Vector3(10f, 0f, 0f));
        }

        [Test]
        public void CountsDownFromThree() {
            _rules.Begin();
            Assert.AreEqual(BombTagPhase.Countdown, _rules.GetState().Phase);
            Assert.AreEqual(3, _rules.GetState().CountdownSeconds);

            Assert.IsTrue(_rules.Tick(1.1f));
            Assert.AreEqual(2, _rules.GetState().CountdownSeconds);
            Assert.IsFalse(_rules.Tick(0.1f));
            Assert.AreEqual(0, _players.BombHolders.Count);
        }

        [Test]
        public void GivesOnePlayerTheBombAfterTheCountdown() {
            _random.IntOffset = 1;

            StartPlaying();

            Assert.AreEqual(BombTagPhase.Playing, _rules.GetState().Phase);
            Assert.AreEqual(2, _players.GetHolder());
        }

        [Test]
        public void PassesTheBombOnTouchWithoutResettingTheFuse() {
            StartPlaying();
            Assert.AreEqual(1, _players.GetHolder());

            _rules.Tick(SHORTEST_FUSE - 1f);
            _players.Move(2, new Vector3(1f, 0f, 0f));
            _rules.Tick(0.1f);
            Assert.AreEqual(2, _players.GetHolder());

            // The fuse had about 0.9s left when the bomb passed.
            _players.Move(2, new Vector3(5f, 0f, 0f));
            _rules.Tick(1f);
            CollectionAssert.AreEqual(new[] { 2 }, _players.Eliminated);
        }

        [Test]
        public void DoesNotPassStraightBack() {
            StartPlaying();
            _players.Move(2, new Vector3(1f, 0f, 0f));

            _rules.Tick(0.1f);
            Assert.AreEqual(2, _players.GetHolder());
            _rules.Tick(0.1f);
            Assert.AreEqual(2, _players.GetHolder());

            _rules.Tick(0.5f);
            Assert.AreEqual(1, _players.GetHolder());
        }

        [Test]
        public void ExplosionEliminatesOnlyTheHolderAndGivesANewBomb() {
            StartPlaying();

            _rules.Tick(SHORTEST_FUSE + 0.1f);

            CollectionAssert.AreEqual(new[] { 1 }, _players.Eliminated);
            Assert.AreEqual(BombTagPhase.Playing, _rules.GetState().Phase);
            Assert.AreEqual(2, _players.GetHolder());
        }

        [Test]
        public void LastPlayerLeftWins() {
            StartPlaying();

            _rules.Tick(SHORTEST_FUSE + 0.1f);
            Assert.IsTrue(_rules.Tick(SHORTEST_FUSE + 0.1f));

            Assert.IsTrue(_rules.IsFinished());
            Assert.AreEqual("Mia", _rules.GetState().WinnerName);
            Assert.AreEqual(0, _players.BombHolders.Count);
        }

        [Test]
        public void HolderWhoLeavesIsReplacedByANewHolder() {
            StartPlaying();

            _players.Remove(1);
            _rules.Tick(0.1f);

            Assert.AreEqual(2, _players.GetHolder());
            Assert.IsFalse(_rules.IsFinished());
        }

        [Test]
        public void PlayersWhoSpawnAfterTheStartAreNotInTheMatch() {
            _rules.Begin();
            _players.Add(4, "Late", new Vector3(20f, 0f, 0f));
            _rules.Tick(COUNTDOWN);

            _rules.Tick(SHORTEST_FUSE + 0.1f);
            _rules.Tick(SHORTEST_FUSE + 0.1f);

            Assert.AreEqual("Mia", _rules.GetState().WinnerName);
        }

        [Test]
        public void SinglePlayerWinsAfterTheCountdown() {
            _players.Remove(2);
            _players.Remove(3);

            StartPlaying();

            Assert.IsTrue(_rules.IsFinished());
            Assert.AreEqual("Ana", _rules.GetState().WinnerName);
        }

        private void StartPlaying() {
            _rules.Begin();
            _rules.Tick(COUNTDOWN);
        }
    }
}
