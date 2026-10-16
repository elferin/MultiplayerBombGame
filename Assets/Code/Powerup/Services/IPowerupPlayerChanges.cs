namespace Code.Powerup {

    // Changes the host makes to a player's power-up state; every client sees them.
    public interface IPowerupPlayerChanges {

        // Shown over the player; None clears it.
        void SetHeld(int playerId, PowerupType type);
        void SetShielded(int playerId, bool isShielded);
    }
}
