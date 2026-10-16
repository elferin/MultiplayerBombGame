using System.Collections.Generic;

namespace Code.Powerup {

    // The power-ups lying in the arena, as seen by the host.
    public interface IPowerupItemList {

        IReadOnlyList<PowerupItem> GetItems();
    }
}
