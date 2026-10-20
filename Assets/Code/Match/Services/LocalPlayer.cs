using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Code.Match {

    [UsedImplicitly]
    public class LocalPlayer : ILocalPlayer {

        public async UniTask SetName(string playerName, CancellationToken cancellation) {
            MatchPlayer owned = null;
            // Ownership is given right after spawning, so this waits for it rather than checking once.
            await UniTask.WaitUntil(() => TryFindOwned(out owned), cancellationToken: cancellation);
            owned.SetPlayerName(playerName);
        }

        public void SetMenuOpen(bool isMenuOpen) {
            if (TryFindOwned(out var owned)) {
                owned.SetMenuOpen(isMenuOpen);
            }
        }

        private bool TryFindOwned(out MatchPlayer owned) {
            foreach (var player in MatchPlayer.Active) {
                if (player.isOwner) {
                    owned = player;
                    return true;
                }
            }
            owned = null;
            return false;
        }
    }
}
