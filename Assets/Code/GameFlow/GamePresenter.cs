using System;
using System.Threading;
using Code.FlowSystem;
using Code.Lobby;
using Code.MainMenu;
using Code.Match;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;

namespace Code.GameFlow {

    // The order of the game's flows. Each transition builds the next flow's data from the last flow's result.
    public class GamePresenter : IAsyncStartable {

        private readonly IFlowRunner _flowRunner;
        private readonly MainMenuToLobby _mainMenuToLobby;
        private readonly LobbyToMatch _lobbyToMatch;
        private readonly MatchToLobby _matchToLobby;
        private readonly LobbyToMainMenu _lobbyToMainMenu;

        public GamePresenter(IFlowRunner flowRunner, MainMenuToLobby mainMenuToLobby, LobbyToMatch lobbyToMatch,
            MatchToLobby matchToLobby, LobbyToMainMenu lobbyToMainMenu) {
            _flowRunner = flowRunner;
            _mainMenuToLobby = mainMenuToLobby;
            _lobbyToMatch = lobbyToMatch;
            _matchToLobby = matchToLobby;
            _lobbyToMainMenu = lobbyToMainMenu;
        }

        // The token is cancelled when the lifetime scope is destroyed, e.g. when Play mode ends.
        // The running flow then ends with an OperationCanceledException, which also ends this loop.
        public async UniTask StartAsync(CancellationToken cancellation = default) {
            try {
                var mainMenuData = MainMenuFlowData.Default();
                while (true) {
                    var mainMenuResult = await _flowRunner.Run(mainMenuData, cancellation);
                    if (mainMenuResult.ShouldQuit) {
                        Quit();
                        return;
                    }

                    var lobbyData = _mainMenuToLobby.GetLobbyData(mainMenuResult);
                    if (lobbyData == null) {
                        return;
                    }

                    mainMenuData = await RunLobby(lobbyData, cancellation);
                }
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested) {
                // Expected when the scope is destroyed; VContainer would otherwise log it as an error.
            }
        }

        // A lobby that fails (e.g. a lobby request throws) goes back to the main menu instead of ending the game loop.
        // The lobby flow leaves the lobby itself when it fails.
        private async UniTask<MainMenuFlowData> RunLobby(LobbyFlowData lobbyData, CancellationToken cancellation) {
            try {
                var lobbyResult = await RunLobbyAndMatches(lobbyData, cancellation);
                return _lobbyToMainMenu.GetMainMenuData(lobbyResult);
            }
            catch (Exception exception) when (!(exception is OperationCanceledException)) {
                Debug.LogException(exception);
                return MainMenuFlowData.Default();
            }
        }

        // Every match returns to the lobby; only leaving the lobby, or failing to join it, ends this.
        private async UniTask<LobbyFlowResult> RunLobbyAndMatches(LobbyFlowData lobbyData, CancellationToken cancellation) {
            while (true) {
                var lobbyResult = await _flowRunner.Run(lobbyData, cancellation);
                if (!lobbyResult.ShouldStartMatch) {
                    return lobbyResult;
                }

                var matchResult = await RunMatch(_lobbyToMatch.GetMatchData(lobbyResult), cancellation);
                lobbyData = _matchToLobby.GetLobbyData(matchResult);
            }
        }

        // A match that fails counts as one that failed to start, so everyone gets back to the lobby and the host can try again.
        // MatchFlow handles its own failures and keeps the join code; this only catches what fails before it runs.
        private async UniTask<MatchFlowResult> RunMatch(MatchFlowData matchData, CancellationToken cancellation) {
            try {
                return await _flowRunner.Run(matchData, cancellation);
            }
            catch (Exception exception) when (!(exception is OperationCanceledException)) {
                Debug.LogException(exception);
                return new MatchFlowResult { StartFailed = true };
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
