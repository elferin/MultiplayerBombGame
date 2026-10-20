using System;
using Code.MatchMenu;
using JetBrains.Annotations;

namespace Code.Match {

    // Applies what this client decides about its own character: the name everyone sees, and no input while the match
    // menu is open. A character that only appears later, e.g. once the host spawns everyone, gets both as it appears.
    [UsedImplicitly]
    public class LocalPlayer : ILocalPlayer, IDisposable {

        private readonly IMatchMenuStateGetter _menuState;

        private string _playerName = string.Empty;

        public LocalPlayer(IMatchMenuStateGetter menuState) {
            _menuState = menuState;
            _menuState.OpenChanged += OnMenuOpenChanged;
            MatchPlayer.OwnedLocally += OnOwnedLocally;
        }

        public void SetName(string playerName) {
            _playerName = playerName ?? string.Empty;
            if (TryFindOwned(out var owned)) {
                owned.SetPlayerName(_playerName);
            }
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
            player.SetPlayerName(_playerName);
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
