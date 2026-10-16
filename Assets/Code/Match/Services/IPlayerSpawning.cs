namespace Code.Match {

    // Spawns a player object for every player in the arena, now and when more join.
    public interface IPlayerSpawning {

        void StartSpawning();
        // Spawns nobody until StartSpawning is called again.
        void StopSpawning();
        // Destroys the player objects left in the scene; PurrNet keeps them when the network stops.
        void RemovePlayers();
    }
}
