namespace Code.Powerup {

    // On the owner: stops a second press from using the held power-up again before the host has cleared it.
    // If the host hasn't answered after a moment, a press sends the request again, in case it was lost;
    // the effect isn't applied again, so Dash or Speed boost can't run twice from one pickup.
    public class PowerupUseGuard {

        // Long enough for the host to answer.
        private const float RESEND_SECONDS = 1f;

        private bool _isWaitingForHost;
        private float _sentTime;

        // A new use, which applies the effect.
        public bool CanUse(PowerupType held) {
            return held != PowerupType.None && !_isWaitingForHost;
        }

        // Only sends the request again.
        public bool ShouldResend(float now) {
            return _isWaitingForHost && now - _sentTime >= RESEND_SECONDS;
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
