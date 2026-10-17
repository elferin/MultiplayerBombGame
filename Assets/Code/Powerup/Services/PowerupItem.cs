using UnityEngine;

namespace Code.Powerup {

    // A power-up lying in the arena, as the host's power-up logic sees it.
    public readonly struct PowerupItem : IHasId {

        // Only valid on the host.
        public int Id { get; }
        public PowerupType Type { get; }
        public Vector3 Position { get; }

        public PowerupItem(int id, PowerupType type, Vector3 position) {
            Id = id;
            Type = type;
            Position = position;
        }
    }
}
