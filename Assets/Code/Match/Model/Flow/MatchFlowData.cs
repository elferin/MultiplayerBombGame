using System;
using Cysharp.Threading.Tasks;

namespace Code.Match {

    // Whether the local player hosts the match or joins the host's.
    public class MatchFlowData {

        public bool IsHost { get; }
        public int MaxPlayers { get; }
        // Players the host waits for before spawning.
        public int PlayerCount { get; }
        // Hands the host's join code to the other players once the network runs.
        public Func<string, UniTask> ShareJoinCode { get; }
        // The host's join code a client connects with.
        public string JoinCode { get; }

        private MatchFlowData(bool isHost, int maxPlayers, int playerCount, Func<string, UniTask> shareJoinCode, string joinCode) {
            IsHost = isHost;
            MaxPlayers = maxPlayers;
            PlayerCount = playerCount;
            ShareJoinCode = shareJoinCode;
            JoinCode = joinCode;
        }

        public static MatchFlowData Host(int maxPlayers, int playerCount, Func<string, UniTask> shareJoinCode) {
            return new MatchFlowData(true, maxPlayers, playerCount, shareJoinCode, string.Empty);
        }

        public static MatchFlowData Client(string joinCode) {
            return new MatchFlowData(false, 0, 0, null, joinCode);
        }
    }
}
