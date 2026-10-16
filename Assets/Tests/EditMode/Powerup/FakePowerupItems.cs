using System.Collections.Generic;
using Code.Powerup;
using UnityEngine;

namespace Code.Tests.Powerup {

    // Keeps spawned power-ups in a list instead of the scene.
    public class FakePowerupItems : IPowerupItems {

        private readonly List<PowerupItem> _items = new();
        private int _lastId;

        public int SpawnCount { get; private set; }

        public IReadOnlyList<PowerupItem> GetItems() {
            return _items;
        }

        public void Spawn(PowerupType type, Vector3 position) {
            SpawnCount++;
            _items.Add(new PowerupItem(++_lastId, type, position));
        }

        public void Remove(int itemId) {
            _items.RemoveAll(item => item.Id == itemId);
        }
    }
}
