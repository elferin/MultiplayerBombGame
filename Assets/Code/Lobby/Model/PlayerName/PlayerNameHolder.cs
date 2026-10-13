using JetBrains.Annotations;

namespace Code.Lobby {
    
    [UsedImplicitly]
    public class PlayerNameHolder : IPlayerName {

        private readonly IPlayerNameStorage _storage;
        private string _playerName;

        public PlayerNameHolder(IPlayerNameStorage storage) {
            _storage = storage;
            _playerName = string.Empty;
        }

        public void SetPlayerName(string playerName) {
            _playerName = playerName ?? string.Empty;
        }

        public void LoadPlayerName() {
            SetPlayerName(_storage.Load());
        }

        public void SavePlayerName() {
            _storage.Save(GetTrimmedPlayerName());
        }

        public string GetPlayerName() {
            return _playerName;
        }

        public string GetTrimmedPlayerName() {
            return _playerName.Trim();
        }

        public bool HasPlayerName() {
            return !string.IsNullOrWhiteSpace(_playerName);
        }
    }
}
