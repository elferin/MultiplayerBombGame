using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Code.Match {

    // Starts the networked match for the players in a lobby.
    public interface IMatchNetwork {

        // The local client lost its connection, e.g. because the host stopped the match.
        event Action Disconnected;
        // The host left the match and told everyone before stopping.
        event Action HostEndedMatch;

        // Returns the join code other players connect with, or an empty string if starting failed.
        UniTask<string> StartHost(int maxPlayers, CancellationToken cancellation);
        UniTask<bool> StartClient(string joinCode, CancellationToken cancellation);
        // Players connected to the server, including the host; 0 when not hosting.
        int GetConnectedPlayerCount();
        // On the host: tells every client the match is over. Does nothing when not hosting.
        void AnnounceMatchEnd();
        // On a client: tells the host this player is leaving, so the host removes them at once. Does nothing on the host.
        void AnnounceLeaving();
        // Stops the server and client started here.
        void Stop();
    }
}
