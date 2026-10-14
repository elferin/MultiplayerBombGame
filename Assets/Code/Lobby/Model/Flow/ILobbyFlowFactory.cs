namespace Code.Lobby {
    public interface ILobbyFlowFactory {
        ILobbyFlow Create(LobbyFlowData data);
    }
}
