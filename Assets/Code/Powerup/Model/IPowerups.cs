namespace Code.Powerup {

    // Power-ups for one round on the host: spawning, pickups and what using one does on the server.
    public interface IPowerups {

        // Spawns the first power-up.
        void Start();
        void Tick(float deltaTime);
    }
}
