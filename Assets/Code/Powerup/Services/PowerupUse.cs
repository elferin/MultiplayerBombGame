namespace Code.Powerup {

    // A player's request to use the power-up they hold.
    public readonly struct PowerupUse {

        public int PlayerId { get; }
        public PowerupType Type { get; }

        public PowerupUse(int playerId, PowerupType type) {
            PlayerId = playerId;
            Type = type;
        }
    }
}
