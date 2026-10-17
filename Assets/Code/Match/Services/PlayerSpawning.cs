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
        private readonly IMatchArena _arena;

        public PlayerSpawning(IRandom random, IMatchArena arena) {
            _random = random;
            _arena = arena;
        }

        // Every round shuffles the spawn points again, so each player starts somewhere random.
        // The points come from the arena the match created, because the scene has none of its own.
        public void StartSpawning() {
            var spawner = FindSpawner();
            if (spawner == null) {
                Debug.LogError(MISSING_SPAWNER);
                return;
            }

            var spawnPoints = new RandomSpawnPoints(_arena.GetSpawnPoints(), _random);
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
                // Only objects PurrNet spawned from a prefab: isSetup is set when an object is spawned, so scene objects
                // that were never spawned stay too. Children go with their root.
                if (identity.isSetup && !identity.isSceneObject && identity.parent == null) {
                    Object.Destroy(identity.gameObject);
                }
            }
        }

        private PlayerSpawner FindSpawner() {
            return Object.FindAnyObjectByType<PlayerSpawner>(FindObjectsInactive.Include);
        }
    }
}
