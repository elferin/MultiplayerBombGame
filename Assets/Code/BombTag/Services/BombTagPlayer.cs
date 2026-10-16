using UnityEngine;

namespace Code.BombTag {

    // A spawned player as the host's rules see it.
    public readonly struct BombTagPlayer {

        // Only valid on the host.
        public int Id { get; }
        public string Name { get; }
        public Vector3 Position { get; }

        public BombTagPlayer(int id, string name, Vector3 position) {
            Id = id;
            Name = name ?? string.Empty;
            Position = position;
        }
    }
}
