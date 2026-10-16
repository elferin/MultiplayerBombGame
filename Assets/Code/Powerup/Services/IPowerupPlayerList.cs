using System.Collections.Generic;

namespace Code.Powerup {

    // What the players hold and have asked for, as seen by the host.
    public interface IPowerupPlayerList {

        IReadOnlyList<PowerupPlayer> GetPlayers();
        // The uses players asked for since the last call.
        IReadOnlyList<PowerupUse> TakeUses();
    }
}
