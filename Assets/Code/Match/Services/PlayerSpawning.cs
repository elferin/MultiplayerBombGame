using JetBrains.Annotations;
using PurrNet;
using UnityEngine;

namespace Code.Match {

    // The scene's PurrNet PlayerSpawner is inactive so it doesn't spawn players as they connect.
    // Activating it while the server runs spawns everyone already in the scene, one per spawn point.
    [UsedImplicitly]
    public class PlayerSpawning : IPlayerSpawning {

        private const string MISSING_SPAWNER = "PlayerSpawning needs a PlayerSpawner in the scene.";

        public void StartSpawning() {
            SetSpawning(true);
        }

        public void StopSpawning() {
            SetSpawning(false);
        }

        // Called after the network stopped, so this only destroys the local copies and sends nothing.
        public void RemovePlayers() {
            foreach (var player in Object.FindObjectsByType<MatchPlayer>()) {
                Object.Destroy(player.gameObject);
            }
        }

        private void SetSpawning(bool isSpawning) {
            var spawner = Object.FindAnyObjectByType<PlayerSpawner>(FindObjectsInactive.Include);
            if (spawner == null) {
                Debug.LogError(MISSING_SPAWNER);
                return;
            }
            spawner.gameObject.SetActive(isSpawning);
        }
    }
}
