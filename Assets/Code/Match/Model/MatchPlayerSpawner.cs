using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Code.Match {

    // On the host: spawns the players once everyone from the lobby has connected.
    [UsedImplicitly]
    public class MatchPlayerSpawner {

        // A player who never connects mustn't hold up the others; they spawn when they connect later.
        private static readonly TimeSpan JOIN_TIMEOUT = TimeSpan.FromSeconds(30);

        private readonly IMatchNetwork _network;
        private readonly IPlayerSpawning _spawning;

        public MatchPlayerSpawner(IMatchNetwork network, IPlayerSpawning spawning) {
            _network = network;
            _spawning = spawning;
        }

        public async UniTask SpawnWhenAllJoined(int playerCount, CancellationToken cancellation) {
            using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellation)) {
                timeout.CancelAfter(JOIN_TIMEOUT);
                await UniTask.WaitUntil(() => _network.GetConnectedPlayerCount() >= playerCount, cancellationToken: timeout.Token)
                    .SuppressCancellationThrow();
            }
            cancellation.ThrowIfCancellationRequested();

            _spawning.StartSpawning();
        }
    }
}
