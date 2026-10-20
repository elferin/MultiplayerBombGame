using System;
using System.Threading;
using Code.Match;
using Cysharp.Threading.Tasks;

namespace Code.Tests.Match {

    // Answers immediately with the configured results.
    public class FakeMatchNetwork : IMatchNetwork {

        public event Action Disconnected = delegate { };
        public event Action HostEndedMatch = delegate { };

        public string HostJoinCode { get; set; } = "RELAY1";
        public bool ClientSucceeds { get; set; } = true;
        public bool ClientThrows { get; set; }
        public int ConnectedPlayerCount { get; set; }

        public int HostStartCount { get; private set; }
        public int LastHostMaxPlayers { get; private set; }
        public int ClientStartCount { get; private set; }
        public string LastClientJoinCode { get; private set; }
        public int StopCount { get; private set; }
        public int AnnounceEndCount { get; private set; }
        public int AnnounceLeavingCount { get; private set; }
        // Whether the network was already stopped when the end or the leaving was announced.
        public bool WasStoppedOnAnnounce { get; private set; }

        public UniTask<string> StartHost(int maxPlayers, CancellationToken cancellation) {
            HostStartCount++;
            LastHostMaxPlayers = maxPlayers;
            return UniTask.FromResult(HostJoinCode);
        }

        public UniTask<bool> StartClient(string joinCode, CancellationToken cancellation) {
            ClientStartCount++;
            LastClientJoinCode = joinCode;
            if (ClientThrows) {
                throw new InvalidOperationException("Client start failed");
            }
            return UniTask.FromResult(ClientSucceeds);
        }

        public int GetConnectedPlayerCount() {
            return ConnectedPlayerCount;
        }

        public void AnnounceMatchEnd() {
            AnnounceEndCount++;
            WasStoppedOnAnnounce = StopCount > 0;
        }

        public void AnnounceLeaving() {
            AnnounceLeavingCount++;
            WasStoppedOnAnnounce = StopCount > 0;
        }

        public void Stop() {
            StopCount++;
        }

        public void RaiseDisconnected() => Disconnected();

        public void RaiseHostEndedMatch() => HostEndedMatch();
    }
}
