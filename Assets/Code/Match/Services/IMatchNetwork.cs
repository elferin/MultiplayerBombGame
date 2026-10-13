using System.Threading;
using Cysharp.Threading.Tasks;

namespace Code.Match {

    // Starts the networked match for the players in a lobby.
    public interface IMatchNetwork {

        // Returns the join code other players connect with, or an empty string if starting failed.
        UniTask<string> StartHost(int maxPlayers, CancellationToken cancellation);
        UniTask<bool> StartClient(string joinCode, CancellationToken cancellation);
        // Players connected to the server, including the host; 0 when not hosting.
        int GetConnectedPlayerCount();
    }
}
