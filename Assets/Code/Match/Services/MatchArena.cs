using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Code.Match {

    [UsedImplicitly]
    public class MatchArena : IMatchArena {

        private readonly MatchAssetLoader _assetLoader;

        private Arena _arena;

        public MatchArena(MatchAssetLoader assetLoader) {
            _assetLoader = assetLoader;
        }

        // Replaces an arena that is still there, so there's never more than one.
        public async UniTask Create(CancellationToken cancellation) {
            Remove();
            _arena = await _assetLoader.CreateArena(cancellation);
        }

        public IReadOnlyList<Transform> GetSpawnPoints() {
            return _arena != null ? _arena.SpawnPoints : Array.Empty<Transform>();
        }

        // Already gone when Play mode or the scene ended first.
        public void Remove() {
            if (_arena != null) {
                _assetLoader.DestroyArena(_arena);
            }
            _arena = null;
        }
    }
}
