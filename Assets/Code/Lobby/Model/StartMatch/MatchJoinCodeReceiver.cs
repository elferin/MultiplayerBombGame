using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Code.Lobby {

    // On the other players: waits for the host's relay join code to reach them through the lobby.
    [UsedImplicitly]
    public class MatchJoinCodeReceiver {

        // The host normally shares the code within a few seconds of tapping Start.
        private static readonly TimeSpan JOIN_CODE_TIMEOUT = TimeSpan.FromSeconds(30);

        private readonly ILobbyMembership _lobbyMembership;

        public MatchJoinCodeReceiver(ILobbyMembership lobbyMembership) {
            _lobbyMembership = lobbyMembership;
        }

        // Returns the new join code, or an empty string if the host's start failed, the lobby is gone, or it took too long.
        public UniTask<string> WaitForJoinCode(string usedJoinCode, CancellationToken cancellation) {
            return WaitForJoinCode(usedJoinCode, JOIN_CODE_TIMEOUT, cancellation);
        }

        public async UniTask<string> WaitForJoinCode(string usedJoinCode, TimeSpan timeout, CancellationToken cancellation) {
            var received = new UniTaskCompletionSource<string>();

            void OnLobbyUpdated(LobbyState lobby) {
                if (TryGetResult(lobby, usedJoinCode, out var joinCode)) {
                    received.TrySetResult(joinCode);
                }
            }

            _lobbyMembership.LobbyUpdated += OnLobbyUpdated;
            try {
                // The code may already be there.
                OnLobbyUpdated(_lobbyMembership.CurrentLobby);

                using (var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellation)) {
                    timeoutSource.CancelAfter(timeout);
                    var (isTimedOut, joinCode) = await received.Task.AttachExternalCancellation(timeoutSource.Token)
                        .SuppressCancellationThrow();
                    cancellation.ThrowIfCancellationRequested();
                    return isTimedOut ? string.Empty : joinCode;
                }
            }
            finally {
                _lobbyMembership.LobbyUpdated -= OnLobbyUpdated;
            }
        }

        // False while the host is still setting up the match network.
        private bool TryGetResult(LobbyState lobby, string usedJoinCode, out string joinCode) {
            joinCode = string.Empty;
            if (!lobby.IsValid) {
                return true;
            }
            if (!string.IsNullOrEmpty(lobby.MatchJoinCode) && lobby.MatchJoinCode != usedJoinCode) {
                joinCode = lobby.MatchJoinCode;
                return true;
            }
            // The host clears the flag when its start fails.
            return !lobby.IsMatchStarting;
        }
    }
}
