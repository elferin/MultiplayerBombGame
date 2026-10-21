using UnityEngine;

namespace Code.Powerup {

    // Cover in the arena that a power-up mustn't appear inside.
    public interface IPowerupObstacles {

        // True when anything solid is within radius of the position, ignoring height.
        bool IsBlocked(Vector3 position, float radius);
    }
}
