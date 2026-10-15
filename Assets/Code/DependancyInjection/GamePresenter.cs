using System;
using System.Threading;
using Code.FlowSystem;
using Code.Lobby;
using Code.MainMenu;
using Code.Match;
using Cysharp.Threading.Tasks;
using VContainer.Unity;

namespace Code.DependancyInjection {

    public class GamePresenter : IAsyncStartable {
        
        private readonly IFlowRunner _flowRunner;
        private readonly MatchJoinCodeSharer _joinCodeSharer;
        private readonly MatchJoinCodeReceiver _joinCodeReceiver;

        public GamePresenter(IFlowRunner flowRunner, MatchJoinCodeSharer joinCodeSharer, MatchJoinCodeReceiver joinCodeReceiver) {
            _flowRunner = flowRunner;
            _joinCodeSharer = joinCodeSharer;
            _joinCodeReceiver = joinCodeReceiver;
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

                    LobbyFlowData lobbyData;
                    if (mainMenuResult.ShouldHost) {
                        lobbyData = LobbyFlowData.CreateLobby(mainMenuResult.MaxPlayers, mainMenuResult.PlayerName);
                    }
                    else if (mainMenuResult.ShouldJoin) {
                        lobbyData = LobbyFlowData.JoinLobby(mainMenuResult.LobbyCode, mainMenuResult.PlayerName);
                    }
                    else {
                        return;
                    }

                    // Leaving the lobby returns to the main menu; a failed join returns to its Join panel.
                    var joinFailure = await RunLobbyAndMatches(lobbyData, cancellation);
                    mainMenuData = joinFailure == LobbyJoinFailure.None
                        ? MainMenuFlowData.Default()
                        : MainMenuFlowData.JoinFailed(joinFailure);
                }
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested) {
                // Expected when the scope is destroyed; VContainer would otherwise log it as an error.
            }
        }

        // Every match returns to the lobby; only leaving the lobby, or failing to join it, ends this.
        private async UniTask<LobbyJoinFailure> RunLobbyAndMatches(LobbyFlowData lobbyData, CancellationToken cancellation) {
            // The last match's code can stay in the lobby for a moment, so it isn't taken for the next match's.
            var usedJoinCode = string.Empty;
            while (true) {
                var lobbyResult = await _flowRunner.Run(lobbyData, cancellation);
                if (!lobbyResult.ShouldStartMatch) {
                    return lobbyResult.JoinFailure;
                }

                var matchResult = await _flowRunner.Run(GetMatchData(lobbyResult, usedJoinCode), cancellation);
                if (!string.IsNullOrEmpty(matchResult.UsedJoinCode)) {
                    usedJoinCode = matchResult.UsedJoinCode;
                }
                lobbyData = LobbyFlowData.ReturnFromMatch(usedJoinCode, matchResult.StartFailed);
            }
        }

        // The host's join code reaches the other players through the lobby.
        private MatchFlowData GetMatchData(LobbyFlowResult lobbyResult, string usedJoinCode) {
            if (lobbyResult.IsHost) {
                return MatchFlowData.Host(lobbyResult.MaxPlayers, lobbyResult.PlayerCount, _joinCodeSharer.ShareJoinCode);
            }
            return MatchFlowData.Client(cancellation => _joinCodeReceiver.WaitForJoinCode(usedJoinCode, cancellation));
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
