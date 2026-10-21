using System;
using UnityEngine;

namespace Code.BombTag {

    // Sends the host's BombTagState and explosions to every player.
    public interface IBombTagSync {

        // Raised on every client, the host's own client included.
        event Action<BombTagState> StateReceived;
        // Raised once on every client, the host's own client included, with where the bomb went off.
        event Action<Vector3> ExplodedReceived;

        void StartListening();
        void StopListening();
        // Only works on the host.
        void Send(BombTagState state);
        // Only works on the host.
        void SendExploded(Vector3 position);
    }
}
