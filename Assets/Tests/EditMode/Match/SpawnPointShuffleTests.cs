using System.Collections.Generic;
using System.Linq;
using Code.Match;
using Code.Tests.Util;
using NUnit.Framework;

namespace Code.Tests.Match {

    public class SpawnPointShuffleTests {

        private FakeRandom _random;

        [SetUp]
        public void SetUp() {
            _random = new FakeRandom();
        }

        [Test]
        public void UsesEveryPointOnceBeforeRepeating() {
            var shuffle = new SpawnPointShuffle(8, _random);

            var points = Take(shuffle, 8);

            CollectionAssert.AreEquivalent(Enumerable.Range(0, 8), points);
        }

        [Test]
        public void OrderFollowsTheRandomNumbers() {
            // Always picking the first remaining slot moves each last point to the front: 1, 2, 3, 0.
            var shuffle = new SpawnPointShuffle(4, _random);

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 0 }, Take(shuffle, 4));
        }

        [Test]
        public void DifferentRandomNumbersGiveADifferentOrder() {
            var first = Take(new SpawnPointShuffle(4, _random), 4);
            _random.IntOffset = 1;
            var second = Take(new SpawnPointShuffle(4, _random), 4);

            CollectionAssert.AreNotEqual(first, second);
        }

        [Test]
        public void ShufflesAgainOnceEveryPointWasUsed() {
            var shuffle = new SpawnPointShuffle(3, _random);

            var points = Take(shuffle, 6);

            CollectionAssert.AreEquivalent(new[] { 0, 1, 2 }, points.Take(3));
            CollectionAssert.AreEquivalent(new[] { 0, 1, 2 }, points.Skip(3));
        }

        private List<int> Take(SpawnPointShuffle shuffle, int count) {
            var points = new List<int>();
            for (var i = 0; i < count; i++) {
                points.Add(shuffle.Next());
            }
            return points;
        }
    }
}
