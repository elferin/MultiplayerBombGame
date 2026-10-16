using System;

namespace Code.BombTag {

    // Sends the host's BombTagState to every player.
    public interface IBombTagSync {

        // Raised on every client, the host's own client included.
        event Action<BombTagState> StateReceived;

        void StartListening();
        void StopListening();
        // Only works on the host.
        void Send(BombTagState state);
    }
}
