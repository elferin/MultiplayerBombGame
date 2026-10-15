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
        // Hands the host's join code to the other players once the network runs.
        public Func<string, UniTask> ShareJoinCode { get; }
        // Waits for the host's join code a client connects with; an empty string means the host's start failed.
        public Func<CancellationToken, UniTask<string>> GetJoinCode { get; }

        private MatchFlowData(bool isHost, int maxPlayers, int playerCount, Func<string, UniTask> shareJoinCode,
            Func<CancellationToken, UniTask<string>> getJoinCode) {
            IsHost = isHost;
            MaxPlayers = maxPlayers;
            PlayerCount = playerCount;
            ShareJoinCode = shareJoinCode;
            GetJoinCode = getJoinCode;
        }

        public static MatchFlowData Host(int maxPlayers, int playerCount, Func<string, UniTask> shareJoinCode) {
            return new MatchFlowData(true, maxPlayers, playerCount, shareJoinCode, null);
        }

        public static MatchFlowData Client(Func<CancellationToken, UniTask<string>> getJoinCode) {
            return new MatchFlowData(false, 0, 0, null, getJoinCode);
        }
    }
}
