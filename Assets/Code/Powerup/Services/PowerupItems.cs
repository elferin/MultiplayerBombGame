using System.Collections.Generic;
using JetBrains.Annotations;
using PurrNet;
using UnityEngine;

namespace Code.Powerup {

    // Spawns and despawns PowerupPickup objects. Only works on the host.
    [UsedImplicitly]
    public class PowerupItems : IPowerupItems {

        private const string MISSING_PREFAB = "PowerupSettings has no pickup prefab.";

        private readonly PowerupSettings _settings;
        // Reused, because the host asks every frame.
        private readonly List<PowerupItem> _items = new();

        public PowerupItems(PowerupSettings settings) {
            _settings = settings;
        }

        public IReadOnlyList<PowerupItem> GetItems() {
            _items.Clear();
            foreach (var pickup in PowerupPickup.Active) {
                _items.Add(new PowerupItem(pickup.PickupId, pickup.Type, pickup.transform.position));
            }
            return _items;
        }

        public void Spawn(PowerupType type, Vector3 position) {
            if (_settings.PickupPrefab == null) {
                Debug.LogError(MISSING_PREFAB);
                return;
            }

            var pickup = Object.Instantiate(_settings.PickupPrefab, position, Quaternion.identity);
            var network = NetworkManager.main;
            if (network != null && !pickup.isSpawned) {
                network.Spawn(pickup.gameObject);
            }
            // Set after spawning, so the change is sent to the clients.
            pickup.SetType(type);
        }

        public void Remove(int itemId) {
            if (PowerupPickup.Active.TryFindById(itemId, out var pickup)) {
                // Destroying a spawned object on the server despawns it on every client.
                Object.Destroy(pickup.gameObject);
            }
        }
    }
}
