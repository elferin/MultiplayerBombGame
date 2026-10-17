using System.Collections.Generic;
using JetBrains.Annotations;

namespace Code.Powerup {

    // Reads and sets the PowerupHolder on each player object. Only works on the host.
    [UsedImplicitly]
    public class PowerupPlayers : IPowerupPlayers {

        // Reused, because the host asks every frame.
        private readonly List<PowerupPlayer> _players = new();
        private readonly List<PowerupUse> _uses = new();

        public IReadOnlyList<PowerupPlayer> GetPlayers() {
            _players.Clear();
            foreach (var holder in PowerupHolder.Active) {
                _players.Add(new PowerupPlayer(holder.PlayerId, holder.transform.position, holder.Held));
            }
            return _players;
        }

        public void SetHeld(int playerId, PowerupType type) {
            var holder = Find(playerId);
            if (holder != null) {
                holder.SetHeld(type);
            }
        }

        public void SetShielded(int playerId, bool isShielded) {
            var holder = Find(playerId);
            if (holder != null) {
                holder.SetShielded(isShielded);
            }
        }

        public IReadOnlyList<PowerupUse> TakeUses() {
            _uses.Clear();
            foreach (var holder in PowerupHolder.Active) {
                var used = holder.TakeUsed();
                if (used != PowerupType.None) {
                    _uses.Add(new PowerupUse(holder.PlayerId, used));
                }
            }
            return _uses;
        }

        private PowerupHolder Find(int playerId) {
            return PowerupHolder.Active.TryFindById(playerId, out var holder) ? holder : null;
        }
    }
}
