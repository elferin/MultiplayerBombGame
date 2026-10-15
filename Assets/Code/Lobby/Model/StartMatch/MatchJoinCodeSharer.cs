using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Code.Lobby {

    // Shares the host's relay join code with everyone in the lobby once the match network is running.
    [UsedImplicitly]
    public class MatchJoinCodeSharer {

        private readonly ILobbyMatchSignals _matchSignals;

        public MatchJoinCodeSharer(ILobbyMatchSignals matchSignals) {
            _matchSignals = matchSignals;
        }

        // Locks the lobby first, so nobody new joins the started match.
        public UniTask ShareJoinCode(string joinCode) {
            return _matchSignals.StartMatch(joinCode);
        }
    }
}
