using System.Threading;
using Cysharp.Threading.Tasks;

namespace Code.Lobby {

    public class LobbyJoiner {

        private readonly ILobbyService _lobbyService;
        private readonly IJoinLobby _joinLobby;

        public LobbyJoiner(ILobbyService lobbyService, IJoinLobby joinLobby) {
            _lobbyService = lobbyService;
            _joinLobby = joinLobby;
        }

        // Joins with the stored lobby code. The status is Joining as soon as this is called,
        // before the first await, so the caller can refresh its view before awaiting the result.
        public async UniTask<bool> TryJoinLobby(string playerName, CancellationToken cancellation) {
            _joinLobby.SetJoinStatus(JoinLobbyStatus.Joining);

            var result = await _lobbyService.JoinLobby(_joinLobby.GetLobbyCode().Trim(), playerName, cancellation);
            var lobby = result.Lobby;
            if (!lobby.IsValid) {
                _joinLobby.SetJoinStatus(GetFailedStatus(result.Failure));
                return false;
            }

            // Joined in the moment between the host starting the match and the lobby being locked.
            if (lobby.IsMatchStarting || !string.IsNullOrEmpty(lobby.MatchJoinCode)) {
                await _lobbyService.LeaveLobby();
                _joinLobby.SetJoinStatus(JoinLobbyStatus.MatchInProgress);
                return false;
            }

            _joinLobby.SetJoinStatus(JoinLobbyStatus.Idle);
            return true;
        }

        private JoinLobbyStatus GetFailedStatus(LobbyJoinFailure failure) {
            switch (failure) {
                case LobbyJoinFailure.MatchInProgress: return JoinLobbyStatus.MatchInProgress;
                case LobbyJoinFailure.LobbyFull: return JoinLobbyStatus.LobbyFull;
                default: return JoinLobbyStatus.Failed;
            }
        }
    }
}
