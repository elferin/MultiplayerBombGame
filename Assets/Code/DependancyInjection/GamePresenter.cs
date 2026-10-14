using System;
using System.Threading;
using Code.Lobby;
using Code.MainMenu;
using Code.Match;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace Code.DependancyInjection {

    public class GamePresenter : IAsyncStartable {
        
        private readonly MainMenuFlowInstantiator _mainMenuFlowInstantiator;
        private readonly LobbyFlowInstantiator _lobbyFlowInstantiator;
        private readonly MatchFlowInstantiator _matchFlowInstantiator;
        private readonly MatchJoinCodeSharer _joinCodeSharer;

        public GamePresenter(MainMenuFlowInstantiator mainMenuFlowInstantiator, LobbyFlowInstantiator lobbyFlowInstantiator,
            MatchFlowInstantiator matchFlowInstantiator, MatchJoinCodeSharer joinCodeSharer) {
            _mainMenuFlowInstantiator = mainMenuFlowInstantiator;
            _lobbyFlowInstantiator = lobbyFlowInstantiator;
            _matchFlowInstantiator = matchFlowInstantiator;
            _joinCodeSharer = joinCodeSharer;
        }

        // The token is cancelled when the lifetime scope is destroyed, e.g. when Play mode ends.
        // The running flow then ends with an OperationCanceledException, which also ends this loop.
        public async UniTask StartAsync(CancellationToken cancellation = default) {
            try {
                while (true) {
                    var mainMenuResult = await _mainMenuFlowInstantiator.StartFlow(cancellation);
                    if (mainMenuResult.ShouldQuit) {
                        Quit();
                        return;
                    }

                    LobbyFlowData lobbyData;
                    if (mainMenuResult.ShouldHost) {
                        lobbyData = LobbyFlowData.CreateLobby(mainMenuResult.MaxPlayers, mainMenuResult.PlayerName);
                    }
                    else if (mainMenuResult.HasJoinedLobby) {
                        lobbyData = LobbyFlowData.JoinedLobby();
                    }
                    else {
                        return;
                    }

                    await RunLobbyAndMatches(lobbyData, cancellation);
                    // Leaving the lobby returns to the main menu.
                }
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested) {
                // Expected when the scope is destroyed; VContainer would otherwise log it as an error.
            }
        }

        // Every match returns to the lobby; only leaving the lobby ends this.
        private async UniTask RunLobbyAndMatches(LobbyFlowData lobbyData, CancellationToken cancellation) {
            while (true) {
                var lobbyResult = await _lobbyFlowInstantiator.StartFlow(lobbyData, cancellation);
                if (!lobbyResult.ShouldStartMatch) {
                    return;
                }

                var matchResult = await _matchFlowInstantiator.StartFlow(GetMatchData(lobbyResult), cancellation);
                lobbyData = LobbyFlowData.ReturnFromMatch(matchResult.UsedJoinCode, matchResult.StartFailed);
            }
        }

        private MatchFlowData GetMatchData(LobbyFlowResult lobbyResult) {
            if (lobbyResult.IsHost) {
                // The host's join code reaches the other players through the lobby.
                return MatchFlowData.Host(lobbyResult.MaxPlayers, lobbyResult.PlayerCount, _joinCodeSharer.ShareJoinCode);
            }
            return MatchFlowData.Client(lobbyResult.MatchJoinCode);
        }

        private void Quit() {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            UnityEngine.Application.Quit();
#endif
        }
    }
}
