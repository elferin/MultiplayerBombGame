using System;
using System.Threading;
using Code.FlowSystem;
using Cysharp.Threading.Tasks;

namespace Code.Match {

    // Whether the local player hosts the match or joins the host's.
    public class MatchFlowData : IFlowData<MatchFlowResult> {

        public bool IsHost { get; }
        public int MaxPlayers { get; }
        // Players the host waits for before spawning.
        public int PlayerCount { get; }
        // Shown above the local player's character.
        public string PlayerName { get; }
        // Hands the host's join code to the other players once the network runs.
        public Func<string, UniTask> ShareJoinCode { get; }
        // Waits for the host's join code a client connects with; an empty string means the host's start failed.
        public Func<CancellationToken, UniTask<string>> GetJoinCode { get; }
        // Plays the game once everyone has spawned; gets the number of connected players (0 on clients).
        // The host ends the match when it returns.
        public Func<int, CancellationToken, UniTask> PlayGame { get; }

        private MatchFlowData(bool isHost, int maxPlayers, int playerCount, string playerName,
            Func<string, UniTask> shareJoinCode, Func<CancellationToken, UniTask<string>> getJoinCode,
            Func<int, CancellationToken, UniTask> playGame) {
            IsHost = isHost;
            MaxPlayers = maxPlayers;
            PlayerCount = playerCount;
            PlayerName = playerName;
            ShareJoinCode = shareJoinCode;
            GetJoinCode = getJoinCode;
            PlayGame = playGame;
        }

        public static MatchFlowData Host(int maxPlayers, int playerCount, string playerName, Func<string, UniTask> shareJoinCode,
            Func<int, CancellationToken, UniTask> playGame) {
            return new MatchFlowData(true, maxPlayers, playerCount, playerName, shareJoinCode, null, playGame);
        }

        public static MatchFlowData Client(string playerName, Func<CancellationToken, UniTask<string>> getJoinCode,
            Func<int, CancellationToken, UniTask> playGame) {
            return new MatchFlowData(false, 0, 0, playerName, null, getJoinCode, playGame);
        }
    }
}
