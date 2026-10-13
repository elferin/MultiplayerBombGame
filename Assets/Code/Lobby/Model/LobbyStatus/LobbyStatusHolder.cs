using JetBrains.Annotations;

namespace Code.Lobby {

    [UsedImplicitly]
    public class LobbyStatusHolder : ILobbyStatus {

        private LobbyStatus _status;

        public LobbyStatusHolder() {
            _status = LobbyStatus.Creating;
        }

        public void SetStatus(LobbyStatus status) {
            _status = status;
        }

        public LobbyStatus GetStatus() {
            return _status;
        }
    }
}
