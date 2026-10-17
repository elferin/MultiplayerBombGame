using System.Collections.Generic;
using UnityEngine;

namespace Code.Match {

    // On the arena prefab: the level a match is played in, created on every player's machine when the match loads.
    public class Arena : MonoBehaviour {

        [SerializeField] private Transform[] spawnPoints;

        public IReadOnlyList<Transform> SpawnPoints => spawnPoints;
    }
}
