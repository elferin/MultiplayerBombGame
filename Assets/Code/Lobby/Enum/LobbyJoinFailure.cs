namespace Code.Lobby {
    // Why joining a lobby failed, as far as the game tells the player.
    public enum LobbyJoinFailure {
        None,
        // The lobby is locked because its match has started.
        MatchInProgress,
        LobbyFull,
        // Wrong code, network error and anything else.
        Other
    }
}
