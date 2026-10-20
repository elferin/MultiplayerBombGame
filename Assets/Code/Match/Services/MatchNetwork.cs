using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using PurrNet;
using PurrNet.Modules;
using PurrNet.Transports;
using PurrNet.UTP;
using Unity.Services.Relay;
using UnityEngine;

namespace Code.Match {

    // Starts PurrNet through Unity Relay, so players need neither each other's IP nor open ports.
    [UsedImplicitly]
    public class MatchNetwork : IMatchNetwork {

        private const string MISSING_NETWORK = "MatchNetwork needs a NetworkManager in the scene that uses a UTPTransport.";
        private const string CLIENT_STATE_PROPERTY = "clientState";
        private const string MISSING_CLIENT_STATE_SETTER =
            "UTPTransport.clientState has no setter any more; check whether PurrNet fixed Disconnect() and remove ResetClientState.";

        public event Action Disconnected = delegate { };
        public event Action HostEndedMatch = delegate { };

        private NetworkManager _startedNetwork;

        public async UniTask<string> StartHost(int maxPlayers, CancellationToken cancellation) {
            if (!TryGetNetwork(out var network, out var transport)) {
                return string.Empty;
            }

            try {
                // The allocation counts only the other players: the host's own client connects through the relay too
                // but doesn't take one of its connections. Tested with 2 players.
                var allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
                var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                cancellation.ThrowIfCancellationRequested();

                // In peer-to-peer mode the host's client also connects through the relay, so it needs the client data too.
                if (!transport.InitializeRelayServer(allocation) || !await transport.InitializeRelayClient(joinCode)) {
                    return string.Empty;
                }
                cancellation.ThrowIfCancellationRequested();

                Listen(network);
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

            Listen(network);
            network.StartClient();
            return true;
        }

        public int GetConnectedPlayerCount() {
            var network = NetworkManagerLookup.Find();
            return network != null && network.isServer ? network.playerCount : 0;
        }

        public void AnnounceMatchEnd() {
            if (_startedNetwork == null || !_startedNetwork.isServer) {
                return;
            }
            _startedNetwork.SendToAll(new MatchEndedMessage());
        }

        public void AnnounceLeaving() {
            if (_startedNetwork == null || _startedNetwork.isServer || !_startedNetwork.isClient) {
                return;
            }
            _startedNetwork.SendToServer(new PlayerLeavingMessage());
        }

        public void Stop() {
            if (_startedNetwork == null) {
                return;
            }

            // Stopped on purpose, so this isn't reported as a lost connection.
            _startedNetwork.onClientConnectionState -= OnClientConnectionState;
            _startedNetwork.Unsubscribe<MatchEndedMessage>(OnMatchEnded, false);
            _startedNetwork.Unsubscribe<PlayerLeavingMessage>(OnPlayerLeaving, true);
            if (_startedNetwork.isServer) {
                _startedNetwork.StopServer();
            }
            if (_startedNetwork.isClient) {
                _startedNetwork.StopClient();
                ResetClientState(_startedNetwork.transport as UTPTransport);
            }
            _startedNetwork = null;
        }

        // Workaround for a PurrNet bug (v1.23.0, still in v1.24.2-beta.14): UTPTransport.Disconnect() stops listening to its
        // client before stopping it, so clientState stays Connected and the next StartClient refuses to run.
        // Reported upstream; once PurrNet sets the state itself, the check below finds Disconnected and this can be deleted.
        private void ResetClientState(UTPTransport transport) {
            if (transport == null || transport.clientState == ConnectionState.Disconnected) {
                return;
            }

            // The setter is private. It also raises onConnectionState, so NetworkManager cleans up as after a real disconnect.
            var setter = typeof(UTPTransport).GetProperty(CLIENT_STATE_PROPERTY)?.GetSetMethod(true);
            if (setter == null) {
                Debug.LogError(MISSING_CLIENT_STATE_SETTER);
                return;
            }
            setter.Invoke(transport, new object[] { ConnectionState.Disconnected });
        }

        private void Listen(NetworkManager network) {
            _startedNetwork = network;
            network.onClientConnectionState += OnClientConnectionState;
            // As a client, so the host's own client hears it too; the host is already ending then.
            network.Subscribe<MatchEndedMessage>(OnMatchEnded, false);
            // Only heard on the host; PurrNet keeps the subscription until the server starts.
            network.Subscribe<PlayerLeavingMessage>(OnPlayerLeaving, true);
        }

        private void OnClientConnectionState(ConnectionState state) {
            if (state == ConnectionState.Disconnected) {
                Disconnected.Invoke();
            }
        }

        private void OnMatchEnded(PlayerID sender, MatchEndedMessage message, bool asServer) {
            HostEndedMatch.Invoke();
        }

        // Kicking runs PurrNet's usual player-left handling, so the network rules despawn the player's character now
        // instead of after the Relay timeout.
        private void OnPlayerLeaving(PlayerID sender, PlayerLeavingMessage message, bool asServer) {
            if (_startedNetwork != null && _startedNetwork.TryGetModule<PlayersManager>(out var players, true)) {
                players.KickPlayer(sender);
            }
        }

        private bool TryGetNetwork(out NetworkManager network, out UTPTransport transport) {
            network = NetworkManagerLookup.Find();
            transport = network != null ? network.transport as UTPTransport : null;
            if (transport == null) {
                Debug.LogError(MISSING_NETWORK);
                return false;
            }
            return true;
        }
    }
}
