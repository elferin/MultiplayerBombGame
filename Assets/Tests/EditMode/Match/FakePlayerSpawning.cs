using Code.Match;

namespace Code.Tests.Match {

    public class FakePlayerSpawning : IPlayerSpawning {

        public int StartCount { get; private set; }
        public int StopCount { get; private set; }
        public int RemoveCount { get; private set; }

        public void StartSpawning() {
            StartCount++;
        }

        public void StopSpawning() {
            StopCount++;
        }

        public void RemovePlayers() {
            RemoveCount++;
        }
    }
}
