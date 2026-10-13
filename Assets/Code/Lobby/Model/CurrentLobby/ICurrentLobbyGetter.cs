namespace Code.Lobby {
    public interface ICurrentLobbyGetter {

        // LobbyState.None until a lobby is created or joined.
        LobbyState GetCurrentLobby();
    }
}
