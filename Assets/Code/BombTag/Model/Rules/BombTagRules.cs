using System.Collections.Generic;
using Code.Util;
using UnityEngine;

namespace Code.BombTag {

    // The host's rules for one match: a countdown, then the bomb passes on touch until one player is left.
    public class BombTagRules {

        private const float COUNTDOWN_SECONDS = 3f;
        private const float MIN_FUSE_SECONDS = 5f;
        private const float MAX_FUSE_SECONDS = 10f;
        // Player capsules are 1 wide, so centres closer than this are touching.
        private const float TOUCH_DISTANCE = 1.1f;
        // Without it the bomb would jump straight back while the two players still touch.
        private const float PASS_COOLDOWN_SECONDS = 0.5f;
        private const int NO_HOLDER = 0;

        private readonly IBombTagPlayers _players;
        private readonly IRandom _random;
        // Players in this match who haven't been eliminated, in the order they were found.
        private readonly List<BombTagPlayer> _alive = new List<BombTagPlayer>();

        private BombTagState _state = BombTagState.Waiting;
        private float _countdownLeft;
        private int _holderId = NO_HOLDER;
        private float _fuseLeft;
        private float _passCooldownLeft;

        public BombTagRules(IBombTagPlayers players, IRandom random) {
            _players = players;
            _random = random;
        }

        public BombTagState GetState() {
            return _state;
        }

        public bool IsFinished() {
            return _state.Phase == BombTagPhase.Finished;
        }

        // Starts the countdown with the players spawned now; players who spawn later aren't in this match.
        public void Begin() {
            _alive.Clear();
            _alive.AddRange(_players.GetPlayers());
            _countdownLeft = COUNTDOWN_SECONDS;
            _state = new BombTagState(BombTagPhase.Countdown, Mathf.CeilToInt(_countdownLeft), string.Empty);
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
            _countdownLeft -= deltaTime;
            if (_countdownLeft > 0f) {
                _state = new BombTagState(BombTagPhase.Countdown, Mathf.CeilToInt(_countdownLeft), string.Empty);
                return;
            }

            _state = new BombTagState(BombTagPhase.Playing, 0, string.Empty);
            UpdateAlivePlayers();
            if (!TryFinish()) {
                GiveNewBomb();
            }
        }

        private void TickPlaying(float deltaTime) {
            UpdateAlivePlayers();
            if (TryFinish()) {
                return;
            }
            // The holder left the match.
            if (_holderId == NO_HOLDER) {
                GiveNewBomb();
            }

            _fuseLeft -= deltaTime;
            if (_fuseLeft <= 0f) {
                Explode();
                return;
            }

            _passCooldownLeft -= deltaTime;
            if (_passCooldownLeft <= 0f) {
                PassToTouchedPlayer();
            }
        }

        // Takes the latest positions and drops players whose objects are gone.
        private void UpdateAlivePlayers() {
            var spawned = _players.GetPlayers();
            for (var i = _alive.Count - 1; i >= 0; i--) {
                if (TryFind(spawned, _alive[i].Id, out var player)) {
                    _alive[i] = player;
                    continue;
                }
                if (_alive[i].Id == _holderId) {
                    _holderId = NO_HOLDER;
                }
                _alive.RemoveAt(i);
            }
        }

        private void Explode() {
            _players.Eliminate(_holderId);
            RemoveAlive(_holderId);
            _holderId = NO_HOLDER;
            if (!TryFinish()) {
                GiveNewBomb();
            }
        }

        private void PassToTouchedPlayer() {
            if (!TryFind(_alive, _holderId, out var holder)) {
                return;
            }
            foreach (var player in _alive) {
                if (player.Id == _holderId || Vector3.Distance(player.Position, holder.Position) > TOUCH_DISTANCE) {
                    continue;
                }
                // The fuse keeps running.
                _players.SetHasBomb(_holderId, false);
                _holderId = player.Id;
                _players.SetHasBomb(_holderId, true);
                _passCooldownLeft = PASS_COOLDOWN_SECONDS;
                return;
            }
        }

        private void GiveNewBomb() {
            _holderId = _alive[_random.Range(0, _alive.Count)].Id;
            _fuseLeft = _random.Range(MIN_FUSE_SECONDS, MAX_FUSE_SECONDS);
            _passCooldownLeft = 0f;
            _players.SetHasBomb(_holderId, true);
        }

        private bool TryFinish() {
            if (_alive.Count > 1) {
                return false;
            }
            var winnerName = _alive.Count == 1 ? _alive[0].Name : string.Empty;
            if (_alive.Count == 1 && _alive[0].Id == _holderId) {
                _players.SetHasBomb(_holderId, false);
            }
            _holderId = NO_HOLDER;
            _state = new BombTagState(BombTagPhase.Finished, 0, winnerName);
            return true;
        }

        private void RemoveAlive(int playerId) {
            for (var i = 0; i < _alive.Count; i++) {
                if (_alive[i].Id == playerId) {
                    _alive.RemoveAt(i);
                    return;
                }
            }
        }

        private bool TryFind(IReadOnlyList<BombTagPlayer> players, int playerId, out BombTagPlayer found) {
            foreach (var player in players) {
                if (player.Id == playerId) {
                    found = player;
                    return true;
                }
            }
            found = default;
            return false;
        }

        private bool AreEqual(BombTagState a, BombTagState b) {
            return a.Phase == b.Phase && a.CountdownSeconds == b.CountdownSeconds && a.WinnerName == b.WinnerName;
        }
    }
}
