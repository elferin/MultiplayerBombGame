using System.Collections.Generic;
using Code.Util;
using PurrNet;
using UnityEngine;

namespace Code.Match {

    // Lets PurrNet's PlayerSpawner place players on the scene's spawn points in a random order.
    public class RandomSpawnPoints : IProvideSpawnPoints {

        private readonly List<Transform> _points = new();
        private readonly SpawnPointShuffle _shuffle;

        public RandomSpawnPoints(IEnumerable<Transform> points, IRandom random) {
            foreach (var point in points) {
                if (point != null) {
                    _points.Add(point);
                }
            }
            _shuffle = new SpawnPointShuffle(_points.Count, random);
        }

        public bool HasPoints() {
            return _points.Count > 0;
        }

        public SpawnPoint NextSpawnPoint(PlayerID player, SceneID scene) {
            var point = _points[_shuffle.Next()];
            return new SpawnPoint { position = point.position, rotation = point.rotation };
        }
    }
}
