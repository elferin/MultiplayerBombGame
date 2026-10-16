using Code.Powerup;
using Code.Util;

namespace Code.BombTag {

    // One round on the host: the countdown, then play until the rules say it's over.
    // Moves time on and keeps the phase; BombTagRules decides what happens to the players and the bomb.
    public class BombTagRound {

        private readonly StartCountdown _countdown;
        private readonly AlivePlayers _alivePlayers;
        private readonly Bomb _bomb;
        private readonly BombTagRules _rules;
        private readonly IPowerups _powerups;

        private BombTagState _state = BombTagState.Waiting;

        public BombTagRound(IBombTagPlayers players, IRandom random, IPowerups powerups) {
            _countdown = new();
            _alivePlayers = new(players);
            _bomb = new(players);
            _rules = new(_alivePlayers, _bomb, random);
            _powerups = powerups;
        }

        public BombTagState GetState() {
            return _state;
        }

        public bool IsFinished() {
            return _state.Phase == BombTagPhase.Finished;
        }

        // Starts the countdown with the players spawned now; players who spawn later aren't in this round.
        public void Begin() {
            _alivePlayers.TakeSpawnedPlayers();
            _countdown.Start();
            _state = new BombTagState(BombTagPhase.Countdown, _countdown.GetWholeSecondsLeft(), string.Empty);
        }

        // Returns true when the state the players see has changed.
        public bool Tick(float deltaTime) {
            var oldState = _state;
            switch (_state.Phase) {
                case BombTagPhase.Countdown:
                    TickCountdown(deltaTime);
                    break;
                case BombTagPhase.Playing:
                    TickPlaying(deltaTime);
                    break;
            }
            return !AreEqual(oldState, _state);
        }

        private void TickCountdown(float deltaTime) {
            _countdown.Tick(deltaTime);
            if (!_countdown.IsDone()) {
                _state = new BombTagState(BombTagPhase.Countdown, _countdown.GetWholeSecondsLeft(), string.Empty);
                return;
            }

            // The rules give the first bomb; the first power-up appears with it.
            _state = new BombTagState(BombTagPhase.Playing, 0, string.Empty);
            RefreshPlayers();
            ApplyRules();
            if (!IsFinished()) {
                _powerups.Start();
            }
        }

        private void TickPlaying(float deltaTime) {
            // First, so the players read next already see a shield used this frame.
            _powerups.Tick(deltaTime);
            RefreshPlayers();
            _bomb.Tick(deltaTime, IsHolderShielded());
            ApplyRules();
        }

        // A shielded holder's fuse doesn't burn.
        private bool IsHolderShielded() {
            return _alivePlayers.TryGet(_bomb.GetHolderId(), out var holder) && holder.IsShielded;
        }

        // Drops players who left, e.g. by disconnecting, so the rules only see players still in the round.
        // A holder who left takes the bomb with them; the rules then give a new one.
        private void RefreshPlayers() {
            _alivePlayers.Refresh();
            if (_bomb.HasHolder() && !_alivePlayers.TryGet(_bomb.GetHolderId(), out _)) {
                _bomb.Remove();
            }
        }

        private void ApplyRules() {
            if (_rules.Apply()) {
                _state = new BombTagState(BombTagPhase.Finished, 0, _alivePlayers.GetWinnerName());
            }
        }

        private bool AreEqual(BombTagState a, BombTagState b) {
            return a.Phase == b.Phase && a.CountdownSeconds == b.CountdownSeconds && a.WinnerName == b.WinnerName;
        }
    }
}
