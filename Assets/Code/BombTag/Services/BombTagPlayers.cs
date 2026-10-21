using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Code.BombTag {

    // Reads the BombHolder on each player object. Only the host may change them; StopMovement works everywhere.
    [UsedImplicitly]
    public class BombTagPlayers : IBombTagPlayers {

        // Reused, because the rules ask for the players every frame.
        private readonly List<BombTagPlayer> _players = new List<BombTagPlayer>();
        private readonly IBombTagSync _sync;

        public BombTagPlayers(IBombTagSync sync) {
            _sync = sync;
        }

        public IReadOnlyList<BombTagPlayer> GetPlayers() {
            _players.Clear();
            foreach (var holder in BombHolder.Active) {
                _players.Add(new BombTagPlayer(holder.PlayerId, holder.PlayerName, holder.transform.position, holder.IsShielded));
            }
            return _players;
        }

        public void SetHasBomb(int playerId, bool hasBomb) {
            var holder = Find(playerId);
            if (holder != null) {
                holder.SetHasBomb(hasBomb);
            }
        }

        public void Eliminate(int playerId) {
            var holder = Find(playerId);
            if (holder != null) {
                // A player is only ever eliminated by the bomb going off. Sent first, while the position is still known.
                _sync.SendExploded(holder.transform.position);
                // Destroying a spawned object on the server despawns it on every client.
                Object.Destroy(holder.gameObject);
            }
        }

        public void StopMovement() {
            foreach (var holder in BombHolder.Active) {
                holder.MatchPlayer.SetCanMove(false);
            }
        }

        private BombHolder Find(int playerId) {
            return BombHolder.Active.TryFindById(playerId, out var holder) ? holder : null;
        }
    }
}
