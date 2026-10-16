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
                return;
            }

            var spawnPoints = new RandomSpawnPoints(spawner.SpawnPoints, _random);
            if (spawnPoints.HasPoints()) {
                spawner.SetRespawnPointProvider(spawnPoints);
            }
            spawner.gameObject.SetActive(true);
        }

        public void StopSpawning() {
            var spawner = FindSpawner();
            if (spawner != null) {
                spawner.gameObject.SetActive(false);
            }
        }

        // Called after the network stopped, so this only destroys the local copies and sends nothing.
        public void RemovePlayers() {
            foreach (var player in Object.FindObjectsByType<MatchPlayer>()) {
                Object.Destroy(player.gameObject);
            }
        }

        private PlayerSpawner FindSpawner() {
            var spawner = Object.FindAnyObjectByType<PlayerSpawner>(FindObjectsInactive.Include);
            if (spawner == null) {
                Debug.LogError(MISSING_SPAWNER);
            }
            return spawner;
        }
    }
}
