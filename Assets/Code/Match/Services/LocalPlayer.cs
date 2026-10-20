using System;
using System.Threading;
using Code.MatchMenu;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Code.Match {

    // Also keeps the character from taking input while the match menu is open, including a character that only
    // appears while it's already open.
    [UsedImplicitly]
    public class LocalPlayer : ILocalPlayer, IDisposable {

        private readonly IMatchMenuStateGetter _menuState;

        public LocalPlayer(IMatchMenuStateGetter menuState) {
            _menuState = menuState;
            _menuState.OpenChanged += OnMenuOpenChanged;
            MatchPlayer.OwnedLocally += OnOwnedLocally;
        }

        public async UniTask SetName(string playerName, CancellationToken cancellation) {
            MatchPlayer owned = null;
            // Ownership is given right after spawning, so this waits for it rather than checking once.
            await UniTask.WaitUntil(() => TryFindOwned(out owned), cancellationToken: cancellation);
            owned.SetPlayerName(playerName);
        }

        public void Dispose() {
            _menuState.OpenChanged -= OnMenuOpenChanged;
            MatchPlayer.OwnedLocally -= OnOwnedLocally;
        }

        private void OnMenuOpenChanged(bool isOpen) {
            if (TryFindOwned(out var owned)) {
                owned.SetInputBlocked(isOpen);
            }
        }

        private void OnOwnedLocally(MatchPlayer player) {
            player.SetInputBlocked(_menuState.IsOpen());
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
