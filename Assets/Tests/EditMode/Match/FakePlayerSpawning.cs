using Code.Match;

namespace Code.Tests {

    public class FakePlayerSpawning : IPlayerSpawning {

        public int StartCount { get; private set; }

        public void StartSpawning() {
            StartCount++;
        }
    }
}
