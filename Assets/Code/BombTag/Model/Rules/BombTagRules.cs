using System.Collections.Generic;
using Code.Util;
using UnityEngine;

namespace Code.BombTag {

    // What the game is: the bomb passes on touch (not to shielded players), an explosion eliminates only its holder,
    // a new bomb goes to a random unshielded player, the last player left wins.
    // Applies these to the round's players and bomb; the round moves time on and keeps the phase.
    public class BombTagRules {

        private const float MIN_FUSE_SECONDS = 5f;
        private const float MAX_FUSE_SECONDS = 10f;
        // Player capsules are 1 wide, so centres closer than this are touching.
        private const float TOUCH_DISTANCE = 1.1f;

        private readonly AlivePlayers _alivePlayers;
        private readonly Bomb _bomb;
        private readonly IRandom _random;
        // Reused, because a new bomb can be given out several times a round.
        private readonly List<BombTagPlayer> _candidates = new();

        public BombTagRules(AlivePlayers alivePlayers, Bomb bomb, IRandom random) {
            _alivePlayers = alivePlayers;
            _bomb = bomb;
            _random = random;
        }

        // Returns true when one player or none is left. Expects the bomb's holder, if any, to be alive.
        public bool Apply() {
            if (IsOver()) {
                // The winner may be holding it.
                _bomb.Remove();
                return true;
            }

            if (_bomb.HasExploded()) {
                EliminateHolder();
                if (IsOver()) {
                    return true;
                }
            }

            if (!_bomb.HasHolder()) {
                GiveNewBomb();
            }
            else if (_bomb.CanPass()) {
                PassToTouchedPlayer();
            }
            return false;
        }

        private bool IsOver() {
            return _alivePlayers.GetCount() <= 1;
        }

        // Only the holder is eliminated.
        private void EliminateHolder() {
            var holderId = _bomb.GetHolderId();
            _bomb.Remove();
            _alivePlayers.Eliminate(holderId);
        }

        // A random unshielded player, with a random fuse. A shielded holder's fuse is paused, so giving them a new bomb
        // would stall the round; only when everyone left is shielded does anyone get it.
        private void GiveNewBomb() {
            _candidates.Clear();
            foreach (var player in _alivePlayers.GetAll()) {
                if (!player.IsShielded) {
                    _candidates.Add(player);
                }
            }
            if (_candidates.Count == 0) {
                _candidates.AddRange(_alivePlayers.GetAll());
            }
            var playerId = _candidates[_random.Range(0, _candidates.Count)].Id;
            _bomb.GiveTo(playerId, _random.Range(MIN_FUSE_SECONDS, MAX_FUSE_SECONDS));
        }

        private void PassToTouchedPlayer() {
            if (!_alivePlayers.TryGet(_bomb.GetHolderId(), out var holder)) {
                return;
            }
            foreach (var player in _alivePlayers.GetAll()) {
                if (player.Id != holder.Id && !player.IsShielded
                    && Vector3.Distance(player.Position, holder.Position) <= TOUCH_DISTANCE) {
                    _bomb.PassTo(player.Id);
                    return;
                }
            }
        }
    }
}
