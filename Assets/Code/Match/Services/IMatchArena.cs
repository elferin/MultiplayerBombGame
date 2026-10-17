using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.Match {

    // The arena a match is played in. Not networked: every player creates their own copy.
    public interface IMatchArena {

        UniTask Create(CancellationToken cancellation);
        // Empty when no arena has been created.
        IReadOnlyList<Transform> GetSpawnPoints();
        void Remove();
    }
}
