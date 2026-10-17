namespace Code.Powerup {

    // On the owner: stops a second press from using the held power-up again before the host has cleared it.
    // No retry: the request is reliable, and the host only refuses one when what the player holds has already changed,
    // which the owner hears about too. A retry would apply Dash or Speed boost locally a second time.
    public class PowerupUseGuard {

        private bool _isWaitingForHost;

        public bool CanUse(PowerupType held) {
            return held != PowerupType.None && !_isWaitingForHost;
        }

        public void MarkSent() {
            _isWaitingForHost = true;
        }

        // The host changed what the player holds, so it has answered.
        public void OnHeldChanged() {
            _isWaitingForHost = false;
        }
    }
}
