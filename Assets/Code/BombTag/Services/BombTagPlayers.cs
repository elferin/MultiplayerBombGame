using System.Collections.Generic;
using Code.Match;
using Code.Powerup;
using JetBrains.Annotations;
using UnityEngine;

namespace Code.BombTag {

    // Reads the BombHolder on each player object. Only the host may change them; StopMovement works everywhere.
    [UsedImplicitly]
    public class BombTagPlayers : IBombTagPlayers {

        // Reused, because the rules ask for the players every frame.
        private readonly List<BombTagPlayer> _players = new List<BombTagPlayer>();

        public IReadOnlyList<BombTagPlayer> GetPlayers() {
            _players.Clear();
            foreach (var holder in BombHolder.Active) {
                var matchPlayer = holder.GetComponent<MatchPlayer>();
                var name = matchPlayer != null ? matchPlayer.PlayerName : string.Empty;
                var powerupHolder = holder.GetComponent<PowerupHolder>();
                var isShielded = powerupHolder != null && powerupHolder.IsShielded;
                _players.Add(new BombTagPlayer(holder.PlayerId, name, holder.transform.position, isShielded));
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
                // Destroying a spawned object on the server despawns it on every client.
                Object.Destroy(holder.gameObject);
            }
        }

        public void StopMovement() {
            foreach (var holder in BombHolder.Active) {
                var matchPlayer = holder.GetComponent<MatchPlayer>();
                if (matchPlayer != null) {
                    matchPlayer.SetCanMove(false);
                }
            }
        }

        private BombHolder Find(int playerId) {
            foreach (var holder in BombHolder.Active) {
                if (holder.PlayerId == playerId) {
                    return holder;
                }
            }
            return null;
        }
    }
}
