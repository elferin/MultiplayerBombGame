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

        // Returns false without spawning when fewer than minPlayers, counting the host, connected in time.
        public UniTask<bool> SpawnWhenAllJoined(int playerCount, int minPlayers, CancellationToken cancellation) {
            return SpawnWhenAllJoined(playerCount, minPlayers, JOIN_TIMEOUT, cancellation);
        }

        public async UniTask<bool> SpawnWhenAllJoined(int playerCount, int minPlayers, TimeSpan timeout,
            CancellationToken cancellation) {
            using (var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellation)) {
                timeoutSource.CancelAfter(timeout);
                await UniTask.WaitUntil(() => _network.GetConnectedPlayerCount() >= playerCount, cancellationToken: timeoutSource.Token)
                    .SuppressCancellationThrow();
            }
            cancellation.ThrowIfCancellationRequested();

            if (_network.GetConnectedPlayerCount() < minPlayers) {
                return false;
            }
            _spawning.StartSpawning();
            return true;
        }
    }
}
