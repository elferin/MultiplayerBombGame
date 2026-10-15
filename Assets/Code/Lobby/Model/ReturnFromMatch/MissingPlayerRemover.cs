using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Code.Lobby {

    // On the host after a match: removes players who haven't come back to the lobby in time,
    // so a crashed or disconnected player can't block the next match.
    [UsedImplicitly]
    public class MissingPlayerRemover {

        private static readonly TimeSpan RETURN_TIMEOUT = TimeSpan.FromSeconds(30);

        private readonly ILobbyPresence _lobbyPresence;

        public MissingPlayerRemover(ILobbyPresence lobbyPresence) {
            _lobbyPresence = lobbyPresence;
        }

        public UniTask RemoveMissingPlayersAfterTimeout(CancellationToken cancellation) {
            return RemoveMissingPlayersAfter(RETURN_TIMEOUT, cancellation);
        }

        public async UniTask RemoveMissingPlayersAfter(TimeSpan timeout, CancellationToken cancellation) {
            // Real time, so pausing or slowing the game doesn't change the timeout.
            await UniTask.Delay(timeout, DelayType.Realtime, cancellationToken: cancellation);

            // Fresh from the server: the host's copy only changes through lobby events, which can be missed.
            var players = await _lobbyPresence.FetchPlayers();
            cancellation.ThrowIfCancellationRequested();

            var missingPlayerIds = new List<string>();
            foreach (var player in players) {
                if (player.HasNotReturned) {
                    missingPlayerIds.Add(player.Id);
                }
            }
            if (missingPlayerIds.Count > 0) {
                await _lobbyPresence.RemovePlayers(missingPlayerIds);
            }
        }
    }
}
