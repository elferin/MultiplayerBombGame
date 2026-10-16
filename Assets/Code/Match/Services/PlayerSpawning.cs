using Code.Util;
using JetBrains.Annotations;
using PurrNet;
using UnityEngine;

namespace Code.Match {

    // The scene's PurrNet PlayerSpawner is inactive so it doesn't spawn players as they connect.
    // Activating it while the server runs spawns everyone already in the scene, one per spawn point.
    [UsedImplicitly]
    public class PlayerSpawning : IPlayerSpawning {

        private const string MISSING_SPAWNER = "PlayerSpawning needs a PlayerSpawner in the scene.";

        private readonly IRandom _random;

        public PlayerSpawning(IRandom random) {
            _random = random;
        }

        // Every round shuffles the spawn points again, so each player starts somewhere random.
        public void StartSpawning() {
            var spawner = FindSpawner();
            if (spawner == null) {
                Debug.LogError(MISSING_SPAWNER);
                return;
            }

            var spawnPoints = new RandomSpawnPoints(spawner.SpawnPoints, _random);
            if (spawnPoints.HasPoints()) {
                spawner.SetRespawnPointProvider(spawnPoints);
            }
            spawner.gameObject.SetActive(true);
        }

        // No spawner means nothing to stop, e.g. when Play mode ends mid-match and the scene is already gone.
        public void StopSpawning() {
            var spawner = FindSpawner();
            if (spawner != null) {
                spawner.gameObject.SetActive(false);
            }
        }

        // Called after the network stopped, so this only destroys the local copies and sends nothing.
        public void RemoveSpawnedObjects() {
            foreach (var identity in Object.FindObjectsByType<NetworkIdentity>()) {
                // Scene objects stay for the next match; children go with their root.
                if (!identity.isSceneObject && identity.parent == null) {
                    Object.Destroy(identity.gameObject);
                }
            }
        }

        private PlayerSpawner FindSpawner() {
            return Object.FindAnyObjectByType<PlayerSpawner>(FindObjectsInactive.Include);
        }
    }
}
