using JetBrains.Annotations;

namespace Code.Match {

    [UsedImplicitly]
    public class MatchStatusHolder : IMatchStatus {

        private MatchStatus _status;

        public MatchStatusHolder() {
            _status = MatchStatus.Starting;
        }

        public void SetStatus(MatchStatus status) {
            _status = status;
        }

        public MatchStatus GetStatus() {
            return _status;
        }
    }
}
