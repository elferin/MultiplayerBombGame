using UnityEngine;

namespace Code {

    public static class VectorExtensions {

        // Distance on the floor only. Players stand at different heights than power-ups, and a player pushed up
        // still touches someone beside them, so gameplay distances ignore height.
        public static float FlatDistanceTo(this Vector3 from, Vector3 to) {
            return Vector2.Distance(new Vector2(from.x, from.z), new Vector2(to.x, to.z));
        }
    }
}
