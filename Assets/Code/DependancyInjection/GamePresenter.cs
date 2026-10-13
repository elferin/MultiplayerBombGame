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
        private readonly MatchPlayerSpawner _matchPlayerSpawner;

        public GamePresenter(MainMenuFlowInstantiator mainMenuFlowInstantiator,
            LobbyFlowInstantiator lobbyFlowInstantiator, MatchPlayerSpawner matchPlayerSpawner) {
            _mainMenuFlowInstantiator = mainMenuFlowInstantiator;
            _lobbyFlowInstantiator = lobbyFlowInstantiator;
            _matchPlayerSpawner = matchPlayerSpawner;
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

                    LobbyFlowArgs lobbyArgs;
                    if (mainMenuResult.ShouldHost) {
                        lobbyArgs = LobbyFlowArgs.CreateLobby(mainMenuResult.MaxPlayers, mainMenuResult.PlayerName);
                    }
                    else if (mainMenuResult.HasJoinedLobby) {
                        lobbyArgs = LobbyFlowArgs.JoinedLobby();
                    }
                    else {
                        return;
                    }

                    var lobbyResult = await _lobbyFlowInstantiator.StartFlow(lobbyArgs, cancellation);
                    if (lobbyResult.ShouldStartMatch) {
                        // Only the host's server spawns players; clients receive them over the network.
                        if (lobbyResult.IsHost) {
                            await _matchPlayerSpawner.SpawnWhenAllJoined(lobbyResult.PlayerCount, cancellation);
                        }
                        // TODO: run the match flow here once it exists.
                        return;
                    }
                    // Leaving the lobby returns to the main menu.
                }
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested) {
                // Expected when the scope is destroyed; VContainer would otherwise log it as an error.
            }
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
