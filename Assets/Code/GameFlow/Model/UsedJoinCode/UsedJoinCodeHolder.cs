using JetBrains.Annotations;

namespace Code.GameFlow {

    // The last match's code can stay in the lobby for a moment after it ends, so it isn't taken for the next match's.
    [UsedImplicitly]
    public class UsedJoinCodeHolder : IUsedJoinCode {

        private string _joinCode = string.Empty;

        public void SetUsedJoinCode(string joinCode) {
            _joinCode = joinCode ?? string.Empty;
        }

        public string GetUsedJoinCode() {
            return _joinCode;
        }
    }
}
