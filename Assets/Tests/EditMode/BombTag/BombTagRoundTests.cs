using Code.BombTag;
using Code.Tests.Util;
using NUnit.Framework;
using UnityEngine;

namespace Code.Tests.BombTag {

    // The round's phases and the state players see; what the rules do is in BombTagRulesTests.
    public class BombTagRoundTests {

        // FloatFraction 0 gives the shortest fuse.
        private const float SHORTEST_FUSE = 5f;
        private const float COUNTDOWN = 3f;

        private FakeBombTagPlayers _players;
        private FakeRandom _random;
        private BombTagRound _round;

        [SetUp]
        public void SetUp() {
            _players = new FakeBombTagPlayers();
            _random = new FakeRandom();
            _round = new BombTagRound(_players, _random);

            // Far enough apart that nobody touches.
            _players.Add(1, "Ana", new Vector3(0f, 0f, 0f));
            _players.Add(2, "Luka", new Vector3(5f, 0f, 0f));
            _players.Add(3, "Mia", new Vector3(10f, 0f, 0f));
        }

        [Test]
        public void StartsWaiting() {
            Assert.AreEqual(BombTagPhase.Waiting, _round.GetState().Phase);
            Assert.IsFalse(_round.Tick(1f));
        }

        [Test]
        public void CountsDownFromThreeWithoutABomb() {
            _round.Begin();
            Assert.AreEqual(BombTagPhase.Countdown, _round.GetState().Phase);
            Assert.AreEqual(3, _round.GetState().CountdownSeconds);

            Assert.IsTrue(_round.Tick(1.1f));
            Assert.AreEqual(2, _round.GetState().CountdownSeconds);
            Assert.IsFalse(_round.Tick(0.1f));
            Assert.AreEqual(0, _players.BombHolders.Count);
        }

        [Test]
        public void PlaysWithABombAfterTheCountdown() {
            _random.IntOffset = 1;

            Assert.IsTrue(StartPlaying());

            Assert.AreEqual(BombTagPhase.Playing, _round.GetState().Phase);
            Assert.AreEqual(2, _players.GetHolder());
        }

        [Test]
        public void FuseBurnsWhilePlaying() {
            StartPlaying();

            Assert.IsFalse(_round.Tick(SHORTEST_FUSE - 0.1f));
            Assert.AreEqual(0, _players.Eliminated.Count);

            Assert.IsFalse(_round.Tick(0.2f));
            CollectionAssert.AreEqual(new[] { 1 }, _players.Eliminated);
            Assert.AreEqual(BombTagPhase.Playing, _round.GetState().Phase);
        }

        [Test]
        public void FinishesWithTheLastPlayerAsWinner() {
            StartPlaying();

            _round.Tick(SHORTEST_FUSE + 0.1f);
            Assert.IsTrue(_round.Tick(SHORTEST_FUSE + 0.1f));

            Assert.IsTrue(_round.IsFinished());
            Assert.AreEqual("Mia", _round.GetState().WinnerName);
            Assert.AreEqual(0, _players.BombHolders.Count);
        }

        [Test]
        public void NothingHappensAfterTheRoundFinished() {
            StartPlaying();
            _round.Tick(SHORTEST_FUSE + 0.1f);
            _round.Tick(SHORTEST_FUSE + 0.1f);

            Assert.IsFalse(_round.Tick(SHORTEST_FUSE + 0.1f));
            Assert.AreEqual(2, _players.Eliminated.Count);
        }

        [Test]
        public void HolderWhoLeavesIsReplacedByANewHolder() {
            StartPlaying();

            _players.Remove(1);
            _round.Tick(0.1f);

            Assert.AreEqual(2, _players.GetHolder());
            Assert.AreEqual(0, _players.Eliminated.Count);
            Assert.IsFalse(_round.IsFinished());
        }

        [Test]
        public void PlayersWhoSpawnAfterTheStartAreNotInTheRound() {
            _round.Begin();
            _players.Add(4, "Late", new Vector3(20f, 0f, 0f));
            _round.Tick(COUNTDOWN);

            _round.Tick(SHORTEST_FUSE + 0.1f);
            _round.Tick(SHORTEST_FUSE + 0.1f);

            Assert.AreEqual("Mia", _round.GetState().WinnerName);
        }

        [Test]
        public void SinglePlayerWinsAfterTheCountdown() {
            _players.Remove(2);
            _players.Remove(3);

            StartPlaying();

            Assert.IsTrue(_round.IsFinished());
            Assert.AreEqual("Ana", _round.GetState().WinnerName);
        }

        private bool StartPlaying() {
            _round.Begin();
            return _round.Tick(COUNTDOWN);
        }
    }
}
