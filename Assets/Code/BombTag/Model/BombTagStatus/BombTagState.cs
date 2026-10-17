using System;
using PurrNet.Packing;

namespace Code.BombTag {

    // What every player's screen shows. The host broadcasts the whole state each time it changes,
    // so a client that misses one message is fixed by the next.
    public readonly struct BombTagState : IPackedAuto, IEquatable<BombTagState> {

        public static readonly BombTagState Waiting = new (BombTagPhase.Waiting, 0, string.Empty);

        private readonly BombTagPhase _phase;
        private readonly int _countdownSeconds;
        private readonly string _winnerName;

        public BombTagPhase Phase => _phase;
        // Whole seconds left on the start countdown, rounded up.
        public int CountdownSeconds => _countdownSeconds;
        // Empty when nobody was left.
        public string WinnerName => _winnerName ?? string.Empty;

        public BombTagState(BombTagPhase phase, int countdownSeconds, string winnerName) {
            _phase = phase;
            _countdownSeconds = countdownSeconds;
            _winnerName = winnerName ?? string.Empty;
        }

        // Every field, so the host broadcasts any change; a new field must be added here too.
        public bool Equals(BombTagState other) {
            return _phase == other._phase && _countdownSeconds == other._countdownSeconds && WinnerName == other.WinnerName;
        }

        public override bool Equals(object obj) {
            return obj is BombTagState other && Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(_phase, _countdownSeconds, WinnerName);
        }
    }
}
