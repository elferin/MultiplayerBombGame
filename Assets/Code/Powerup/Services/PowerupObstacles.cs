using JetBrains.Annotations;
using UnityEngine;

namespace Code.Powerup {

    // Checks the colliders on the Obstacle layer, e.g. the columns and blocks in the arena.
    [UsedImplicitly]
    public class PowerupObstacles : IPowerupObstacles {

        private const string OBSTACLE_LAYER = "Obstacle";
        // Tall enough to reach any cover from the floor, whatever height the position is at.
        private const float CHECK_HEIGHT = 3f;

        private readonly int _layerMask = LayerMask.GetMask(OBSTACLE_LAYER);

        public bool IsBlocked(Vector3 position, float radius) {
            var bottom = new Vector3(position.x, 0f, position.z);
            var top = bottom + Vector3.up * CHECK_HEIGHT;
            return Physics.CheckCapsule(bottom, top, radius, _layerMask, QueryTriggerInteraction.Ignore);
        }
    }
}
