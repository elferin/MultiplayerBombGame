using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Code.Lobby {

    // Who is on the lobby screen and who is still in a match; the host only starts when everyone is back.
    public interface ILobbyPresence {

        // Marks the local player as on the lobby screen or in a match.
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
