using System;
using System.Collections.Generic;
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
        // Clears the starting flag and the join code, then unlocks the lobby, so the next match can start.
        UniTask EndMatch();
        // Tells everyone in the lobby that the host tapped Start, before the join code is ready.
        UniTask AnnounceMatchStart();
        // Marks the local player as on the lobby screen or in a match; the host only starts when everyone is back.
        UniTask SetInLobby(bool isInLobby, CancellationToken cancellation);
        // Back from a match: marks the local player as in the lobby.
        // False if the host removed them in the meantime; the lobby is then gone for this player.
        UniTask<bool> ReturnToLobby(CancellationToken cancellation);
        // The players as Unity Lobby has them right now, not the copy kept from lobby events. Empty if the request fails.
        UniTask<IReadOnlyList<LobbyPlayer>> FetchPlayers();
        // Host only.
        UniTask RemovePlayers(IReadOnlyList<string> playerIds);
    }
}
