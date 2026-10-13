using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Code.Lobby {

    // Creating, joining and leaving lobbies, independent of the lobby provider.
    public interface ILobbyService {

        event Action<LobbyState> LobbyUpdated;

        LobbyState CurrentLobby { get; }

        UniTask<LobbyState> CreateLobby(int maxPlayers, string playerName, CancellationToken cancellation);
        // The result's lobby is LobbyState.None if joining failed; its failure says why.
        UniTask<LobbyJoinResult> JoinLobby(string lobbyCode, string playerName, CancellationToken cancellation);
        UniTask LeaveLobby();
        // Locks the lobby so nobody new can join, then shares the match's join code with everyone in it.
        UniTask StartMatch(string joinCode);
        // Unlocks the lobby and clears the join code, so the next match can start.
        UniTask EndMatch();
    }
}
