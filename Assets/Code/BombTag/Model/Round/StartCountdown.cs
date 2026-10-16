using UnityEngine;

namespace Code.BombTag {

    // The countdown before the first bomb.
    public class StartCountdown {

        private const float COUNTDOWN_SECONDS = 3f;

        private float _secondsLeft;

        public void Start() {
            _secondsLeft = COUNTDOWN_SECONDS;
        }

        public void Tick(float deltaTime) {
            _secondsLeft -= deltaTime;
        }

        public bool IsDone() {
            return _secondsLeft <= 0f;
        }

        // Rounded up, so the screen shows 3, 2, 1 for a full second each.
        public int GetWholeSecondsLeft() {
            return Mathf.Max(0, Mathf.CeilToInt(_secondsLeft));
        }
    }
}
