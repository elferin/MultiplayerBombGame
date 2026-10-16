using System.Collections.Generic;
using System.Linq;
using Code.BombTag;
using UnityEngine;

namespace Code.Tests.BombTag {

    // Players placed by the test; eliminating one removes it like a despawn would.
    public class FakeBombTagPlayers : IBombTagPlayers {

        private readonly List<BombTagPlayer> _players = new List<BombTagPlayer>();

        public HashSet<int> BombHolders { get; } = new HashSet<int>();
        public List<int> Eliminated { get; } = new List<int>();
        public int StopMovementCount { get; private set; }

        public void Add(int id, string name, Vector3 position) {
            _players.Add(new BombTagPlayer(id, name, position));
        }

        public void Move(int id, Vector3 position) {
            var index = _players.FindIndex(player => player.Id == id);
            _players[index] = new BombTagPlayer(id, _players[index].Name, position, _players[index].IsShielded);
        }

        public void SetShielded(int id, bool isShielded) {
            var index = _players.FindIndex(player => player.Id == id);
            var player = _players[index];
            _players[index] = new BombTagPlayer(id, player.Name, player.Position, isShielded);
        }

        // Like a player disconnecting.
        public void Remove(int id) {
            _players.RemoveAll(player => player.Id == id);
        }

        public int GetHolder() {
            return BombHolders.Single();
        }

        public IReadOnlyList<BombTagPlayer> GetPlayers() {
            return _players;
        }

        public void SetHasBomb(int playerId, bool hasBomb) {
            if (hasBomb) {
                BombHolders.Add(playerId);
            }
            else {
                BombHolders.Remove(playerId);
            }
        }

        public void StopMovement() {
            StopMovementCount++;
        }

        public void Eliminate(int playerId) {
            Eliminated.Add(playerId);
            BombHolders.Remove(playerId);
            Remove(playerId);
        }
    }
}
