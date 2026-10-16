namespace Code.Powerup {
    public enum PowerupType {
        None,
        // A burst in the last direction moved.
        Dash,
        // The bomb can't be passed to this player for a while.
        Shield,
        // A higher top speed for a while.
        SpeedBoost
    }
}
