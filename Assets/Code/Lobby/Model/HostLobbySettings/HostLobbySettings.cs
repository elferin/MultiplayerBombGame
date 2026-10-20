using JetBrains.Annotations;

namespace Code.Lobby {
    
    [UsedImplicitly]
    public class HostLobbySettings : IHostLobbySettings {

        // Nobody can play alone. The lobby's start rule and the match's player check use this too.
        private const int MIN_PLAYERS = 2;
        private const int MAX_PLAYERS = 8;
        private const int DEFAULT_PLAYERS = 4;

        private string _playerCountText;
        private int _playerCount;
        private bool _isPlayerCountValid;

        public HostLobbySettings() {
            SetPlayerCount(DEFAULT_PLAYERS.ToString());
        }

        public void SetPlayerCount(string playerCountText) {
            var isValid = int.TryParse(playerCountText, out var playerCount)
                          && playerCount is >= MIN_PLAYERS and <= MAX_PLAYERS;

            _playerCountText = playerCountText ?? string.Empty;
            _playerCount = isValid ? playerCount : 0;
            _isPlayerCountValid = isValid;
        }

        public int GetMinPlayers() {
            return MIN_PLAYERS;
        }

        public int GetMaxPlayers() {
            return MAX_PLAYERS;
        }

        public string GetPlayerCountText() {
            return _playerCountText;
        }

        public int GetPlayerCount() {
            return _playerCount;
        }

        public bool IsPlayerCountValid() {
            return _isPlayerCountValid;
        }
    }
}
