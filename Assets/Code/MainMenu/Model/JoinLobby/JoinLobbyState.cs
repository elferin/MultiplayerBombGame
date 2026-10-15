using JetBrains.Annotations;

namespace Code.MainMenu {
    
    [UsedImplicitly]
    public class JoinLobbyState : IJoinLobby {

        private string _lobbyCode = string.Empty;
        private JoinLobbyStatus _joinStatus = JoinLobbyStatus.Idle;

        public void SetLobbyCode(string lobbyCode) {
            _lobbyCode = lobbyCode ?? string.Empty;
        }

        public void SetJoinStatus(JoinLobbyStatus joinStatus) {
            _joinStatus = joinStatus;
        }

        public string GetLobbyCode() {
            return _lobbyCode;
        }

        public JoinLobbyStatus GetJoinStatus() {
            return _joinStatus;
        }
    }
}
