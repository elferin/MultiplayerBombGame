namespace Code.Lobby {
    public enum LobbyStatus {
        Creating,
        Ready,
        Failed,
        Leaving,
        // The last match failed to start; the host can try again.
        StartFailed
    }
}
