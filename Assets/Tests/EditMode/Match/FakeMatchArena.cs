using System;
using System.Collections.Generic;
using System.Threading;
using Code.Match;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.Tests.Match {

    // Creates nothing; counts the calls and remembers what the network had done when the arena was created.
    public class FakeMatchArena : IMatchArena {

        private readonly FakeMatchNetwork _network;

        public int CreateCount { get; private set; }
        public int RemoveCount { get; private set; }
        public bool WasNetworkStartedOnCreate { get; private set; }

        public FakeMatchArena(FakeMatchNetwork network) {
            _network = network;
        }

        public UniTask Create(CancellationToken cancellation) {
            CreateCount++;
            WasNetworkStartedOnCreate = _network.HostStartCount > 0 || _network.ClientStartCount > 0;
            return UniTask.CompletedTask;
        }

        public IReadOnlyList<Transform> GetSpawnPoints() {
            return Array.Empty<Transform>();
        }

        public void Remove() {
            RemoveCount++;
        }
    }
}
