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
        // Including the host. Nobody can play alone, so with fewer the match ends instead.
        private const int MIN_PLAYERS = 2;

        private readonly IMatchNetwork _network;
        private readonly IPlayerSpawning _spawning;

        public MatchPlayerSpawner(IMatchNetwork network, IPlayerSpawning spawning) {
            _network = network;
            _spawning = spawning;
        }

        // Returns false without spawning when too few players connected in time.
        public UniTask<bool> SpawnWhenAllJoined(int playerCount, CancellationToken cancellation) {
            return SpawnWhenAllJoined(playerCount, JOIN_TIMEOUT, cancellation);
        }

        public async UniTask<bool> SpawnWhenAllJoined(int playerCount, TimeSpan timeout, CancellationToken cancellation) {
            using (var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellation)) {
                timeoutSource.CancelAfter(timeout);
                await UniTask.WaitUntil(() => _network.GetConnectedPlayerCount() >= playerCount, cancellationToken: timeoutSource.Token)
                    .SuppressCancellationThrow();
            }
            cancellation.ThrowIfCancellationRequested();

            if (_network.GetConnectedPlayerCount() < MIN_PLAYERS) {
                return false;
            }
            _spawning.StartSpawning();
            return true;
        }
    }
}
