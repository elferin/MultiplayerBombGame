using Code.Util;

namespace Code.Tests.Util {

    // Picks the configured values instead of random ones.
    public class FakeRandom : IRandom {

        // 0 picks min, 1 picks max.
        public float FloatFraction { get; set; }
        // Added to min for whole numbers, wrapped into the range.
        public int IntOffset { get; set; }

        public int Range(int min, int max) {
            return min + IntOffset % (max - min);
        }

        public float Range(float min, float max) {
            return min + (max - min) * FloatFraction;
        }
    }
}
