using System.Collections.Generic;
using Code.Powerup;
using UnityEngine;

namespace Code.Tests.Powerup {

    // Players placed by the test; uses are queued by the test like the owners' requests would be.
    public class FakePowerupPlayers : IPowerupPlayers {

        private readonly List<PowerupPlayer> _players = new();
        private readonly List<PowerupUse> _pendingUses = new();
        private readonly List<PowerupUse> _takenUses = new();

        public HashSet<int> Shielded { get; } = new();

        public void Add(int id, Vector3 position, PowerupType held = PowerupType.None) {
            _players.Add(new PowerupPlayer(id, position, held));
        }

        public void Move(int id, Vector3 position) {
            var index = _players.FindIndex(player => player.Id == id);
            _players[index] = new PowerupPlayer(id, position, _players[index].Held);
        }

        public PowerupType GetHeld(int id) {
            return _players.Find(player => player.Id == id).Held;
        }

        public void RequestUse(int id, PowerupType type) {
            _pendingUses.Add(new PowerupUse(id, type));
        }

        public IReadOnlyList<PowerupPlayer> GetPlayers() {
            return _players;
        }

        public void SetHeld(int playerId, PowerupType type) {
            var index = _players.FindIndex(player => player.Id == playerId);
            if (index >= 0) {
                _players[index] = new PowerupPlayer(playerId, _players[index].Position, type);
            }
        }

        public void SetShielded(int playerId, bool isShielded) {
            if (isShielded) {
                Shielded.Add(playerId);
            }
            else {
                Shielded.Remove(playerId);
            }
        }

        public IReadOnlyList<PowerupUse> TakeUses() {
            _takenUses.Clear();
            _takenUses.AddRange(_pendingUses);
            _pendingUses.Clear();
            return _takenUses;
        }
    }
}
