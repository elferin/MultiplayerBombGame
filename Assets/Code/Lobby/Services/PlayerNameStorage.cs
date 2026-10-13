using JetBrains.Annotations;
using UnityEngine;

namespace Code.Lobby {

    [UsedImplicitly]
    public class PlayerNameStorage : IPlayerNameStorage {

        private const string PLAYER_NAME_KEY = "PlayerName";
        private const string DEFAULT_PLAYER_NAME = "Player";

        public string Load() {
            return PlayerPrefs.GetString(PLAYER_NAME_KEY, DEFAULT_PLAYER_NAME);
        }

        public void Save(string playerName) {
            if (string.IsNullOrWhiteSpace(playerName)) {
                return;
            }
            PlayerPrefs.SetString(PLAYER_NAME_KEY, playerName);
            PlayerPrefs.Save();
        }
    }
}
