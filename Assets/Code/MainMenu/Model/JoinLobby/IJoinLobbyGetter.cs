namespace Code.MainMenu {
    public interface IJoinLobbyGetter {

        string GetLobbyCode();
        JoinLobbyStatus GetJoinStatus();
    }
}
