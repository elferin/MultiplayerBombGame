namespace Code.Lobby {
    public enum LobbyStatus {
        Creating,
        Ready,
        Failed,
        Leaving,
        // The last match failed to start; the host can try again.
        StartFailed,
        // The host removed this player for not coming back from a match in time.
        Removed,
        // The host tapped Start; the lobby requests before the match screen are still finishing.
        Starting
    }
}
