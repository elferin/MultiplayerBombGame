namespace Code.Lobby {
    // How LobbyFlow gets its lobby.
    public enum LobbyEntry {
        Create,
        // By the code the player typed on the main menu's Join panel.
        Join,
        // Still joined from before the match.
        ReturnFromMatch
    }
}
