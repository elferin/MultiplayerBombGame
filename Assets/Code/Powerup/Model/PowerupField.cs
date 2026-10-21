using System.Collections.Generic;
using Code.Util;
using UnityEngine;

namespace Code.Powerup {

    // The host's power-ups for one round. One lies in the arena at a time; the next appears a while after it's picked up.
    // Using one is the owner's request: the host checks they hold it, clears it, and runs the shield here.
    public class PowerupField : IPowerups {

        private const float SPAWN_HEIGHT = 0.5f;
        // Tries before giving up on a spot far enough from every player.
        private const int SPAWN_TRIES = 10;
        private static readonly PowerupType[] SpawnableTypes = { PowerupType.Dash, PowerupType.Shield, PowerupType.SpeedBoost };

        private readonly IPowerupPlayers _players;
        private readonly IPowerupItems _items;
        private readonly IRandom _random;
        private readonly IPowerupObstacles _obstacles;
        private readonly PowerupSettings _settings;
        private readonly Dictionary<int, float> _shieldSecondsLeft = new();
        // Reused, because shields are ticked every frame.
        private readonly List<int> _shieldedIds = new();
        private readonly List<int> _expiredShields = new();
        private readonly HashSet<int> _pickedUpThisTick = new();
        // A copy, because picking one up removes it from the arena while this loops.
        private readonly List<PowerupItem> _itemsThisTick = new();

        private bool _isWaitingToSpawn;
        private float _secondsUntilSpawn;

        public PowerupField(IPowerupPlayers players, IPowerupItems items, IRandom random, IPowerupObstacles obstacles,
            PowerupSettings settings) {
            _players = players;
            _items = items;
            _random = random;
            _obstacles = obstacles;
            _settings = settings;
        }

        public void Start() {
            SpawnRandom();
        }

        public void Tick(float deltaTime) {
            var players = _players.GetPlayers();
            PickUp(players);
            ApplyUses(players);
            TickShields(deltaTime);
            TickSpawning(deltaTime);
        }

        // The first player within reach who holds nothing takes it.
        private void PickUp(IReadOnlyList<PowerupPlayer> players) {
            _pickedUpThisTick.Clear();
            _itemsThisTick.Clear();
            _itemsThisTick.AddRange(_items.GetItems());
            foreach (var item in _itemsThisTick) {
                foreach (var player in players) {
                    if (player.Held != PowerupType.None || _pickedUpThisTick.Contains(player.Id)
                        || player.Position.FlatDistanceTo(item.Position) > _settings.PickupRadius) {
                        continue;
                    }
                    _players.SetHeld(player.Id, item.Type);
                    _items.Remove(item.Id);
                    _pickedUpThisTick.Add(player.Id);
                    _isWaitingToSpawn = true;
                    _secondsUntilSpawn = _settings.SpawnDelaySeconds;
                    break;
                }
            }
        }

        private void ApplyUses(IReadOnlyList<PowerupPlayer> players) {
            foreach (var use in _players.TakeUses()) {
                // Ignored unless they really hold it, e.g. after a late or repeated request.
                if (!players.TryFindById(use.PlayerId, out var player) || player.Held != use.Type) {
                    continue;
                }
                _players.SetHeld(use.PlayerId, PowerupType.None);
                if (use.Type == PowerupType.Shield) {
                    _players.SetShielded(use.PlayerId, true);
                    _shieldSecondsLeft[use.PlayerId] = _settings.ShieldSeconds;
                }
            }
        }

        private void TickShields(float deltaTime) {
            _shieldedIds.Clear();
            _shieldedIds.AddRange(_shieldSecondsLeft.Keys);
            _expiredShields.Clear();
            foreach (var playerId in _shieldedIds) {
                var secondsLeft = _shieldSecondsLeft[playerId] - deltaTime;
                _shieldSecondsLeft[playerId] = secondsLeft;
                if (secondsLeft <= 0f) {
                    _expiredShields.Add(playerId);
                }
            }
            foreach (var playerId in _expiredShields) {
                _shieldSecondsLeft.Remove(playerId);
                _players.SetShielded(playerId, false);
            }
        }

        private void TickSpawning(float deltaTime) {
            if (!_isWaitingToSpawn) {
                return;
            }
            _secondsUntilSpawn -= deltaTime;
            if (_secondsUntilSpawn <= 0f) {
                _isWaitingToSpawn = false;
                SpawnRandom();
            }
        }

        private void SpawnRandom() {
            var type = SpawnableTypes[_random.Range(0, SpawnableTypes.Length)];
            _items.Spawn(type, FindSpawnPosition());
        }

        // A random spot clear of cover and away from every player. If none is found, the last one clear of cover,
        // because one inside cover couldn't be picked up; the arena centre is kept open for when even that fails.
        private Vector3 FindSpawnPosition() {
            var players = _players.GetPlayers();
            var halfSize = _settings.ArenaHalfSize;
            var position = new Vector3(0f, SPAWN_HEIGHT, 0f);
            for (var i = 0; i < SPAWN_TRIES; i++) {
                var candidate = new Vector3(_random.Range(-halfSize, halfSize), SPAWN_HEIGHT, _random.Range(-halfSize, halfSize));
                if (_obstacles.IsBlocked(candidate, _settings.PickupRadius)) {
                    continue;
                }
                position = candidate;
                if (IsAwayFromPlayers(players, position)) {
                    break;
                }
            }
            return position;
        }

        private bool IsAwayFromPlayers(IReadOnlyList<PowerupPlayer> players, Vector3 position) {
            foreach (var player in players) {
                if (player.Position.FlatDistanceTo(position) < _settings.MinDistanceFromPlayers) {
                    return false;
                }
            }
            return true;
        }
    }
}
