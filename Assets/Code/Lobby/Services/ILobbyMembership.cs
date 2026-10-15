using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Code.Lobby {

    // Creating, joining and leaving lobbies, and the lobby the player is in, independent of the lobby provider.
    public interface ILobbyMembership {

        event Action<LobbyState> LobbyUpdated;

        LobbyState CurrentLobby { get; }

        UniTask<LobbyState> CreateLobby(int maxPlayers, string playerName, CancellationToken cancellation);
        // The result's lobby is LobbyState.None if joining failed; its failure says why.
        UniTask<LobbyJoinResult> JoinLobby(string lobbyCode, string playerName, CancellationToken cancellation);
        UniTask LeaveLobby();
    }
}
