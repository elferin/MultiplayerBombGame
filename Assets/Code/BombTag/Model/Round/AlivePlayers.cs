using System.Collections.Generic;

namespace Code.BombTag {

    // The players still in the match. Players who spawn after it starts aren't added.
    public class AlivePlayers {

        private readonly IBombTagPlayers _players;
        // In the order they were found.
        private readonly List<BombTagPlayer> _alivePlayers = new();

        public AlivePlayers(IBombTagPlayers players) {
            _players = players;
        }

        // Starts the match with the players spawned now.
        public void TakeSpawnedPlayers() {
            _alivePlayers.Clear();
            _alivePlayers.AddRange(_players.GetPlayers());
        }

        // Takes the latest positions and drops players whose objects are gone, e.g. because they disconnected.
        public void Refresh() {
            var spawned = _players.GetPlayers();
            for (var i = _alivePlayers.Count - 1; i >= 0; i--) {
                if (spawned.TryFindById(_alivePlayers[i].Id, out var player)) {
                    _alivePlayers[i] = player;
                }
                else {
                    _alivePlayers.RemoveAt(i);
                }
            }
        }

        public IReadOnlyList<BombTagPlayer> GetAll() {
            return _alivePlayers;
        }

        public int GetCount() {
            return _alivePlayers.Count;
        }

        public bool TryGet(int playerId, out BombTagPlayer player) {
            return _alivePlayers.TryFindById(playerId, out player);
        }

        // Despawns the player for everyone.
        public void Eliminate(int playerId) {
            _players.Eliminate(playerId);
            for (var i = 0; i < _alivePlayers.Count; i++) {
                if (_alivePlayers[i].Id == playerId) {
                    _alivePlayers.RemoveAt(i);
                    return;
                }
            }
        }

        // The last player left, or empty when nobody is.
        public string GetWinnerName() {
            return _alivePlayers.Count == 1 ? _alivePlayers[0].Name : string.Empty;
        }
    }
}
