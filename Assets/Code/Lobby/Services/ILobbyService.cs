namespace Code.Lobby {

    // Everything the game does with the lobby. LobbyFlow uses all of it; other classes take only the part they need.
    public interface ILobbyService : ILobbyMembership, ILobbyMatchSignals, ILobbyPresence {
    }
}
