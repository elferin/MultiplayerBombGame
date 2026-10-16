using System.Threading;
using Code.BombTag;
using Code.FlowSystem;
using Code.Lobby;
using Code.Match;
using Code.Profile;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Code.GameFlow {

    // The host's join code reaches the other players through the lobby, so neither Lobby nor Match knows the other.
    // The game played in the match is chosen here too, so Match doesn't know about BombTag.
    [UsedImplicitly]
    public class LobbyToMatch {

        private readonly MatchJoinCodeSharer _joinCodeSharer;
        private readonly MatchJoinCodeReceiver _joinCodeReceiver;
        private readonly IUsedJoinCodeGetter _usedJoinCode;
        private readonly IPlayerNameGetter _playerName;
        private readonly IFlowRunner _flowRunner;

        public LobbyToMatch(MatchJoinCodeSharer joinCodeSharer, MatchJoinCodeReceiver joinCodeReceiver,
            IUsedJoinCodeGetter usedJoinCode, IPlayerNameGetter playerName, IFlowRunner flowRunner) {
            _joinCodeSharer = joinCodeSharer;
            _joinCodeReceiver = joinCodeReceiver;
            _usedJoinCode = usedJoinCode;
            _playerName = playerName;
            _flowRunner = flowRunner;
        }

        public MatchFlowData GetMatchData(LobbyFlowResult result) {
            var playerName = _playerName.GetTrimmedPlayerName();
            if (result.IsHost) {
                return MatchFlowData.Host(result.MaxPlayers, result.PlayerCount, playerName, _joinCodeSharer.ShareJoinCode,
                    (playerCount, cancellation) => PlayBombTag(BombTagFlowData.Host(playerCount), cancellation));
            }
            var usedJoinCode = _usedJoinCode.GetUsedJoinCode();
            return MatchFlowData.Client(playerName, cancellation => _joinCodeReceiver.WaitForJoinCode(usedJoinCode, cancellation),
                (_, cancellation) => PlayBombTag(BombTagFlowData.Client(), cancellation));
        }

        private async UniTask PlayBombTag(BombTagFlowData data, CancellationToken cancellation) {
            await _flowRunner.Run(data, cancellation);
        }
    }
}
