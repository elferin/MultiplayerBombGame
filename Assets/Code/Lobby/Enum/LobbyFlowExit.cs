namespace Code.Lobby {

    // Why LobbyFlow stops waiting in the lobby.
    public enum LobbyFlowExit {
        Leave,
        // The local player is the host and clicked Start.
        StartAsHost,
        // The host shared the match's join code through the lobby.
        JoinStartedMatch
    }
}
