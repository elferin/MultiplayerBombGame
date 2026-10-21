using System.Collections.Generic;
using Code.Powerup;
using UnityEngine;

namespace Code.Tests.Powerup {

    // Blocks exactly the spots the test adds, ignoring the radius and height.
    public class FakePowerupObstacles : IPowerupObstacles {

        private readonly List<Vector2> _blocked = new();

        public void Block(float x, float z) {
            _blocked.Add(new Vector2(x, z));
        }

        public bool IsBlocked(Vector3 position, float radius) {
            return _blocked.Contains(new Vector2(position.x, position.z));
        }
    }
}
