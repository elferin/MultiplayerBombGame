using Code.Lobby;
using Code.Match;
using JetBrains.Annotations;

namespace Code.GameFlow {

    [UsedImplicitly]
    public class MatchToLobby {

        private readonly IUsedJoinCode _usedJoinCode;

        public MatchToLobby(IUsedJoinCode usedJoinCode) {
            _usedJoinCode = usedJoinCode;
        }

        public LobbyFlowData GetLobbyData(MatchFlowResult result) {
            // A match that failed to start has no code; the last real one is still the one to ignore.
            if (!string.IsNullOrEmpty(result.UsedJoinCode)) {
                _usedJoinCode.SetUsedJoinCode(result.UsedJoinCode);
            }
            return LobbyFlowData.ReturnFromMatch(_usedJoinCode.GetUsedJoinCode(), result.StartFailed);
        }
    }
}
