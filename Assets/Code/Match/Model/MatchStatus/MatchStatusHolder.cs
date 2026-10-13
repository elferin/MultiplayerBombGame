using JetBrains.Annotations;

namespace Code.Match {

    [UsedImplicitly]
    public class MatchStatusHolder : IMatchStatus {

        private MatchStatus _status;
        private bool _isHost;

        public MatchStatusHolder() {
            _status = MatchStatus.Starting;
        }

        public void SetStatus(MatchStatus status) {
            _status = status;
        }

        public void SetIsHost(bool isHost) {
            _isHost = isHost;
        }

        public MatchStatus GetStatus() {
            return _status;
        }

        public bool IsHost() {
            return _isHost;
        }
    }
}
