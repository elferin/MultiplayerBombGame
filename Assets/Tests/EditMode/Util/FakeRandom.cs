using System.Collections.Generic;
using Code.Util;

namespace Code.Tests.Util {

    // Picks the configured values instead of random ones.
    public class FakeRandom : IRandom {

        // 0 picks min, 1 picks max.
        public float FloatFraction { get; set; }
        // Used one by one before FloatFraction, for code that draws several different values.
        public Queue<float> NextFloatFractions { get; } = new();
        // Added to min for whole numbers, wrapped into the range.
        public int IntOffset { get; set; }

        public int Range(int min, int max) {
            return min + IntOffset % (max - min);
        }

        public float Range(float min, float max) {
            var fraction = NextFloatFractions.Count > 0 ? NextFloatFractions.Dequeue() : FloatFraction;
            return min + (max - min) * fraction;
        }
    }
}
