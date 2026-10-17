using UnityEngine;

namespace Code.Powerup {

    // A spawned player as the host's power-up logic sees it.
    public readonly struct PowerupPlayer : IHasId {

        // Only valid on the host.
        public int Id { get; }
        public Vector3 Position { get; }
        public PowerupType Held { get; }

        public PowerupPlayer(int id, Vector3 position, PowerupType held) {
            Id = id;
            Position = position;
            Held = held;
        }
    }
}
