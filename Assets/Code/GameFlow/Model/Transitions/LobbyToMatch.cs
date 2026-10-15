using Code.Lobby;
using Code.Match;
using JetBrains.Annotations;

namespace Code.GameFlow {

    // The host's join code reaches the other players through the lobby, so neither Lobby nor Match knows the other.
    [UsedImplicitly]
    public class LobbyToMatch {

        private readonly MatchJoinCodeSharer _joinCodeSharer;
        private readonly MatchJoinCodeReceiver _joinCodeReceiver;
        private readonly IUsedJoinCodeGetter _usedJoinCode;

        public LobbyToMatch(MatchJoinCodeSharer joinCodeSharer, MatchJoinCodeReceiver joinCodeReceiver,
            IUsedJoinCodeGetter usedJoinCode) {
            _joinCodeSharer = joinCodeSharer;
            _joinCodeReceiver = joinCodeReceiver;
            _usedJoinCode = usedJoinCode;
        }

        public MatchFlowData GetMatchData(LobbyFlowResult result) {
            if (result.IsHost) {
                return MatchFlowData.Host(result.MaxPlayers, result.PlayerCount, _joinCodeSharer.ShareJoinCode);
            }
            var usedJoinCode = _usedJoinCode.GetUsedJoinCode();
            return MatchFlowData.Client(cancellation => _joinCodeReceiver.WaitForJoinCode(usedJoinCode, cancellation));
        }
    }
}
