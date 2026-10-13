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
            var spawner = Object.FindAnyObjectByType<PlayerSpawner>(FindObjectsInactive.Include);
            if (spawner == null) {
                Debug.LogError(MISSING_SPAWNER);
                return;
            }
            spawner.gameObject.SetActive(true);
        }
    }
}
