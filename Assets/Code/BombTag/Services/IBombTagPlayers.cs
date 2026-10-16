using System.Collections.Generic;

namespace Code.BombTag {

    // The spawned player objects, as seen by the host.
    public interface IBombTagPlayers {

        // Players who disconnected or were eliminated are no longer listed.
        IReadOnlyList<BombTagPlayer> GetPlayers();
        // Every client sees the bomb on that player.
        void SetHasBomb(int playerId, bool hasBomb);
        // Despawns the player's object for everyone.
        void Eliminate(int playerId);
        // Stops this client's own character; works on every client, not only the host.
        void StopMovement();
    }
}
