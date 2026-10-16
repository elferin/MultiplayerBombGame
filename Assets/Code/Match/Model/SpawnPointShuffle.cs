using System.Collections.Generic;
using Code.Util;

namespace Code.Match {

    // Hands out spawn point numbers in a random order, so no two players share a point until every point is used.
    public class SpawnPointShuffle {

        private readonly int _pointCount;
        private readonly IRandom _random;
        private readonly List<int> _order = new();

        private int _next;

        public SpawnPointShuffle(int pointCount, IRandom random) {
            _pointCount = pointCount;
            _random = random;
            Shuffle();
        }

        // Shuffles again once every point has been handed out, e.g. for late joiners.
        public int Next() {
            if (_next >= _order.Count) {
                Shuffle();
            }
            return _order[_next++];
        }

        // Fisher-Yates: every order is equally likely.
        private void Shuffle() {
            _order.Clear();
            for (var i = 0; i < _pointCount; i++) {
                _order.Add(i);
            }
            for (var i = _order.Count - 1; i > 0; i--) {
                var j = _random.Range(0, i + 1);
                (_order[i], _order[j]) = (_order[j], _order[i]);
            }
            _next = 0;
        }
    }
}
