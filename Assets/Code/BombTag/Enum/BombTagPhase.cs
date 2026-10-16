namespace Code.BombTag {
    public enum BombTagPhase {
        // The host is waiting for every player object to spawn.
        Waiting,
        // The start countdown is on screen.
        Countdown,
        // Someone has the bomb.
        Playing,
        // One player is left, or nobody.
        Finished
    }
}
