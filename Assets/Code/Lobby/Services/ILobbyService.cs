using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Code.Lobby {

    // Creating, joining and leaving lobbies, independent of the lobby provider.
    public interface ILobbyService {

        event Action<LobbyState> LobbyUpdated;

        LobbyState CurrentLobby { get; }

        UniTask<LobbyState> CreateLobby(int maxPlayers, string playerName, CancellationToken cancellation);
        // Returns LobbyState.None if the code is wrong or the lobby is full.
        UniTask<LobbyState> JoinLobby(string lobbyCode, string playerName, CancellationToken cancellation);
        UniTask LeaveLobby();
        // Locks the lobby so nobody new can join, then shares the match's join code with everyone in it.
        UniTask StartMatch(string joinCode);
    }
}
