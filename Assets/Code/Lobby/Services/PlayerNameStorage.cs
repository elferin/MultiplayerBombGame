using JetBrains.Annotations;
using UnityEngine;
#if UNITY_EDITOR
using ParrelSync;
#endif

namespace Code.Lobby {

    [UsedImplicitly]
    public class PlayerNameStorage : IPlayerNameStorage {

        private const string PLAYER_NAME_KEY = "PlayerName";
        private const string CLONE_PLAYER_NAME_KEY = "PlayerName_Clone";
        private const string DEFAULT_PLAYER_NAME = "Player";

        public string Load() {
            return PlayerPrefs.GetString(GetPlayerNameKey(), DEFAULT_PLAYER_NAME);
        }

        public void Save(string playerName) {
            if (string.IsNullOrWhiteSpace(playerName)) {
                return;
            }
            PlayerPrefs.SetString(GetPlayerNameKey(), playerName);
            PlayerPrefs.Save();
        }

        // Multiplayer testing on one machine: a ParrelSync clone shares the main editor's PlayerPrefs,
        // so without its own key each editor would overwrite the other's saved name. Builds always use PLAYER_NAME_KEY.
        private string GetPlayerNameKey() {
#if UNITY_EDITOR
            if (ClonesManager.IsClone()) {
                return CLONE_PLAYER_NAME_KEY;
            }
#endif
            return PLAYER_NAME_KEY;
        }
    }
}
