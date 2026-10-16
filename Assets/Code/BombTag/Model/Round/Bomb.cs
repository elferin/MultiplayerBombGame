namespace Code.BombTag {

    // The bomb in a running round: who holds it, how long its fuse has left, and whether it can be passed yet.
    // It only tracks time; BombTagRules decides what an explosion or a touch does.
    public class Bomb {

        // Without it the bomb would jump straight back while the two players still touch.
        private const float PASS_COOLDOWN_SECONDS = 0.5f;
        private const int NO_HOLDER = 0;

        private readonly IBombTagPlayers _players;

        private int _holderId = NO_HOLDER;
        private float _fuseLeft;
        private float _passCooldownLeft;

        public Bomb(IBombTagPlayers players) {
            _players = players;
        }

        public bool HasHolder() {
            return _holderId != NO_HOLDER;
        }

        public int GetHolderId() {
            return _holderId;
        }

        public bool HasExploded() {
            return HasHolder() && _fuseLeft <= 0f;
        }

        public bool CanPass() {
            return HasHolder() && _passCooldownLeft <= 0f;
        }

        // A new bomb with a new fuse; it can be passed straight away.
        public void GiveTo(int playerId, float fuseSeconds) {
            _holderId = playerId;
            _fuseLeft = fuseSeconds;
            _passCooldownLeft = 0f;
            _players.SetHasBomb(_holderId, true);
        }

        // The fuse keeps running.
        public void PassTo(int playerId) {
            _players.SetHasBomb(_holderId, false);
            _holderId = playerId;
            _players.SetHasBomb(_holderId, true);
            _passCooldownLeft = PASS_COOLDOWN_SECONDS;
        }

        // The pass cooldown always runs; the fuse doesn't while it's paused, e.g. while the holder is shielded.
        public void Tick(float deltaTime, bool isFusePaused) {
            if (!HasHolder()) {
                return;
            }
            if (!isFusePaused) {
                _fuseLeft -= deltaTime;
            }
            _passCooldownLeft -= deltaTime;
        }

        // Nobody holds it afterwards, e.g. after an explosion or when the holder left.
        public void Remove() {
            if (HasHolder()) {
                _players.SetHasBomb(_holderId, false);
            }
            _holderId = NO_HOLDER;
        }
    }
}
