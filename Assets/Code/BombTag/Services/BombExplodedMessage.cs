using PurrNet.Packing;
using UnityEngine;

namespace Code.BombTag {

    // The host sends it when a bomb goes off, so every player sees the explosion where the holder stood.
    // Sent apart from BombTagState, because an explosion is a moment rather than something the screen keeps showing.
    public readonly struct BombExplodedMessage : IPackedAuto {

        private readonly Vector3 _position;

        public Vector3 Position => _position;

        public BombExplodedMessage(Vector3 position) {
            _position = position;
        }
    }
}
