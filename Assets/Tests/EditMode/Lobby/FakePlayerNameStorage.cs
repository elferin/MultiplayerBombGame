using Code.Lobby;

namespace Code.Tests {

    // Keeps the saved name in memory instead of PlayerPrefs.
    public class FakePlayerNameStorage : IPlayerNameStorage {

        public string SavedName { get; private set; }
        public int SaveCount { get; private set; }

        public FakePlayerNameStorage(string savedName = "Player") {
            SavedName = savedName;
        }

        public string Load() {
            return SavedName;
        }

        public void Save(string playerName) {
            SaveCount++;
            if (string.IsNullOrWhiteSpace(playerName)) {
                return;
            }
            SavedName = playerName;
        }
    }
}
