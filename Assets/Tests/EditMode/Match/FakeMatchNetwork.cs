using System;
using System.Threading;
using Code.Match;
using Cysharp.Threading.Tasks;

namespace Code.Tests.Match {

    // Answers immediately with the configured results.
    public class FakeMatchNetwork : IMatchNetwork {

        public event Action Disconnected = delegate { };

        public string HostJoinCode { get; set; } = "RELAY1";
        public bool ClientSucceeds { get; set; } = true;
        public int ConnectedPlayerCount { get; set; }

        public int HostStartCount { get; private set; }
        public int LastHostMaxPlayers { get; private set; }
        public int ClientStartCount { get; private set; }
        public string LastClientJoinCode { get; private set; }
        public int StopCount { get; private set; }

        public UniTask<string> StartHost(int maxPlayers, CancellationToken cancellation) {
            HostStartCount++;
            LastHostMaxPlayers = maxPlayers;
            return UniTask.FromResult(HostJoinCode);
        }

        public UniTask<bool> StartClient(string joinCode, CancellationToken cancellation) {
            ClientStartCount++;
            LastClientJoinCode = joinCode;
            return UniTask.FromResult(ClientSucceeds);
        }

        public int GetConnectedPlayerCount() {
            return ConnectedPlayerCount;
        }

        public void Stop() {
            StopCount++;
        }

        public void RaiseDisconnected() => Disconnected();
    }
}
