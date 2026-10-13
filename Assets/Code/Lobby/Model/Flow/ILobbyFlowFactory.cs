namespace Code.Lobby {
    public interface ILobbyFlowFactory {
        ILobbyFlow Create(LobbyFlowArgs args);
    }
}
