namespace Code.Powerup {

    // On the owner: stops a second press from using the held power-up again before the host has cleared it.
    // After a moment a press is allowed again, in case the request was lost or refused.
    public class PowerupUseGuard {

        // Long enough for the host to answer.
        private const float RETRY_SECONDS = 1f;

        private bool _isWaitingForHost;
        private float _sentTime;

        public bool CanUse(PowerupType held, float now) {
            return held != PowerupType.None && (!_isWaitingForHost || now - _sentTime >= RETRY_SECONDS);
        }

        public void MarkSent(float now) {
            _isWaitingForHost = true;
            _sentTime = now;
        }

        // The host changed what the player holds, so it has answered.
        public void OnHeldChanged() {
            _isWaitingForHost = false;
        }
    }
}
