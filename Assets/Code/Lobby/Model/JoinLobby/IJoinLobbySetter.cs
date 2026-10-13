namespace Code.Lobby {
    public interface IJoinLobbySetter {

        void SetLobbyCode(string lobbyCode);
        void SetJoinStatus(JoinLobbyStatus joinStatus);
    }
}
