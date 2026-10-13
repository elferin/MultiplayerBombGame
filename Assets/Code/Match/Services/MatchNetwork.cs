using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using PurrNet;
using PurrNet.UTP;
using Unity.Services.Relay;
using UnityEngine;

namespace Code.Match {

    // Starts PurrNet through Unity Relay, so players need neither each other's IP nor open ports.
    [UsedImplicitly]
    public class MatchNetwork : IMatchNetwork {

        private const string MISSING_NETWORK = "MatchNetwork needs a NetworkManager in the scene that uses a UTPTransport.";

        public async UniTask<string> StartHost(int maxPlayers, CancellationToken cancellation) {
            if (!TryGetNetwork(out var network, out var transport)) {
                return string.Empty;
            }

            try {
                // The host's own client doesn't use a relay connection.
                var allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
                var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                cancellation.ThrowIfCancellationRequested();

                // In peer-to-peer mode the host's client also connects through the relay, so it needs the client data too.
                if (!transport.InitializeRelayServer(allocation) || !await transport.InitializeRelayClient(joinCode)) {
                    return string.Empty;
                }
                cancellation.ThrowIfCancellationRequested();

                network.StartHost();
                return joinCode;
            }
            catch (RelayServiceException exception) {
                Debug.LogException(exception);
                return string.Empty;
            }
        }

        public async UniTask<bool> StartClient(string joinCode, CancellationToken cancellation) {
            if (!TryGetNetwork(out var network, out var transport)) {
                return false;
            }

            // Logs the error itself when the join code is wrong or the relay can't be reached.
            var isInitialized = await transport.InitializeRelayClient(joinCode);
            cancellation.ThrowIfCancellationRequested();
            if (!isInitialized) {
                return false;
            }

            network.StartClient();
            return true;
        }

        public int GetConnectedPlayerCount() {
            var network = GetNetworkManager();
            return network != null && network.isServer ? network.playerCount : 0;
        }

        private static NetworkManager GetNetworkManager() {
            return NetworkManager.main != null ? NetworkManager.main : Object.FindAnyObjectByType<NetworkManager>();
        }

        private static bool TryGetNetwork(out NetworkManager network, out UTPTransport transport) {
            network = GetNetworkManager();
            transport = network != null ? network.transport as UTPTransport : null;
            if (transport == null) {
                Debug.LogError(MISSING_NETWORK);
                return false;
            }
            return true;
        }
    }
}
