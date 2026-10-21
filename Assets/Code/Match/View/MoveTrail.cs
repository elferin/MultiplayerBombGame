using UnityEngine;

namespace Code.Match {

    // On the player prefab: leaves a trail while the character moves faster than it can run, i.e. while dashing or boosted.
    // Speed comes from how far it moved, so it works the same on every client, whoever moves the character.
    public class MoveTrail : MonoBehaviour {

        [SerializeField] private TrailRenderer trail;
        [Tooltip("Faster than this on the floor, in units per second, shows the trail. A little above the normal top speed.")]
        [SerializeField] private float minSpeed = 7f;

        private Vector3 _lastPosition;

        private void OnEnable() {
            _lastPosition = transform.position;
            trail.emitting = false;
            trail.Clear();
        }

        private void LateUpdate() {
            var position = transform.position;
            var moved = position - _lastPosition;
            moved.y = 0f;
            _lastPosition = position;
            if (Time.deltaTime > 0f) {
                trail.emitting = moved.magnitude / Time.deltaTime > minSpeed;
            }
        }
    }
}
