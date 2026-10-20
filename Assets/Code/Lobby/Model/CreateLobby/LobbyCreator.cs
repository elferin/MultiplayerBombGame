using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Code.Lobby {

    [UsedImplicitly]
    public class LobbyCreator {

        private readonly ILobbyMembership _lobbyMembership;
        private readonly ILobbyStatusSetter _status;
        private readonly ICurrentLobbySetter _currentLobby;

        public LobbyCreator(ILobbyMembership lobbyMembership, ILobbyStatusSetter status, ICurrentLobbySetter currentLobby) {
            _lobbyMembership = lobbyMembership;
            _status = status;
            _currentLobby = currentLobby;
        }

        // The status is Creating and the previous lobby is cleared as soon as this is called,
        // before the first await, so the caller can show the view before awaiting the result.
        public async UniTask<bool> TryCreateLobby(int maxPlayers, string playerName, CancellationToken cancellation) {
            _status.SetStatus(LobbyStatus.Creating);
            _currentLobby.SetCurrentLobby(LobbyState.None);

            var lobby = await _lobbyMembership.CreateLobby(maxPlayers, playerName, cancellation);
            _currentLobby.SetCurrentLobby(lobby);
            _status.SetStatus(lobby.IsValid ? LobbyStatus.Ready : LobbyStatus.CreateFailed);
            return lobby.IsValid;
        }
    }
}
