using UnityEngine;

namespace Code.Powerup {

    // Puts power-ups into the arena and takes them out, for every player. Only works on the host.
    public interface IPowerupItemSpawner {

        void Spawn(PowerupType type, Vector3 position);
        void Remove(int itemId);
    }
}
