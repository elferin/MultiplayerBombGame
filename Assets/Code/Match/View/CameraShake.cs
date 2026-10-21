using UnityEngine;

namespace Code.Match {

    // On the main camera: shakes it around where it was placed, e.g. when a bomb goes off. The camera itself doesn't move.
    public class CameraShake : MonoBehaviour {

        private Vector3 _restPosition;
        private float _strength;
        private float _secondsLeft;
        private float _duration;

        private void Awake() {
            _restPosition = transform.localPosition;
        }

        // A stronger shake replaces a weaker one still running; a weaker one doesn't cut a stronger one short.
        public void Shake(float strength, float seconds) {
            if (_secondsLeft > 0f && CurrentStrength() > strength) {
                return;
            }
            _strength = strength;
            _duration = seconds;
            _secondsLeft = seconds;
        }

        private void LateUpdate() {
            if (_secondsLeft <= 0f) {
                return;
            }
            _secondsLeft -= Time.deltaTime;
            if (_secondsLeft <= 0f) {
                transform.localPosition = _restPosition;
                return;
            }
            transform.localPosition = _restPosition + Random.insideUnitSphere * CurrentStrength();
        }

        // Fades out over the shake.
        private float CurrentStrength() {
            return _strength * (_secondsLeft / _duration);
        }
    }
}
