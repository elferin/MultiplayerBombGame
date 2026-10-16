using PurrNet.Packing;

namespace Code.BombTag {

    // What every player's screen shows. The host broadcasts the whole state each time it changes,
    // so a client that misses one message is fixed by the next.
    public readonly struct BombTagState : IPackedAuto {

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
    }
}
