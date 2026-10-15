using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Code.Lobby {

    [UsedImplicitly]
    public class LobbyJoiner {

        private readonly ILobbyMembership _lobbyMembership;
        private readonly ILobbyStatusSetter _status;
        private readonly ICurrentLobbySetter _currentLobby;

        public LobbyJoiner(ILobbyMembership lobbyMembership, ILobbyStatusSetter status, ICurrentLobbySetter currentLobby) {
            _lobbyMembership = lobbyMembership;
            _status = status;
            _currentLobby = currentLobby;
        }

        // The status is Joining and the previous lobby is cleared as soon as this is called,
        // before the first await, so the caller can show the view before awaiting the result.
        // Returns None once joined, otherwise why joining failed.
        public async UniTask<LobbyJoinFailure> TryJoinLobby(string lobbyCode, string playerName, CancellationToken cancellation) {
            _status.SetStatus(LobbyStatus.Joining);
            _currentLobby.SetCurrentLobby(LobbyState.None);

            var result = await _lobbyMembership.JoinLobby(lobbyCode, playerName, cancellation);
            var lobby = result.Lobby;
            if (!lobby.IsValid) {
                _status.SetStatus(LobbyStatus.Failed);
                return result.Failure;
            }

            // Joined in the moment between the host starting the match and the lobby being locked.
            if (lobby.IsMatchStarting || !string.IsNullOrEmpty(lobby.MatchJoinCode)) {
                await _lobbyMembership.LeaveLobby();
                _status.SetStatus(LobbyStatus.Failed);
                return LobbyJoinFailure.MatchInProgress;
            }

            _currentLobby.SetCurrentLobby(lobby);
            _status.SetStatus(LobbyStatus.Ready);
            return LobbyJoinFailure.None;
        }
    }
}
