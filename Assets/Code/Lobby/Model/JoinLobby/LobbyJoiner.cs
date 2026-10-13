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

            var lobby = await _lobbyService.JoinLobby(_joinLobby.GetLobbyCode().Trim(), playerName, cancellation);
            _joinLobby.SetJoinStatus(lobby.IsValid ? JoinLobbyStatus.Idle : JoinLobbyStatus.Failed);
            return lobby.IsValid;
        }
    }
}
