using JetBrains.Annotations;

namespace Code.BombTag {

    [UsedImplicitly]
    public class BombTagStatusHolder : IBombTagStatus {

        private BombTagState _state;

        public BombTagStatusHolder() {
            _state = BombTagState.Waiting;
        }

        public void SetState(BombTagState state) {
            _state = state;
        }

        public BombTagState GetState() {
            return _state;
        }
    }
}
