namespace Code.Lobby {
    public interface IJoinLobbyGetter {

        string GetLobbyCode();
        JoinLobbyStatus GetJoinStatus();
    }
}
