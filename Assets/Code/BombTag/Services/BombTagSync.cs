using System;
using JetBrains.Annotations;
using PurrNet;
using UnityEngine;

namespace Code.BombTag {

    // Uses PurrNet broadcasts, so no networked object is needed in the scene.
    [UsedImplicitly]
    public class BombTagSync : IBombTagSync {

        private const string MISSING_NETWORK = "BombTagSync needs a NetworkManager in the scene.";

        public event Action<BombTagState> StateReceived = delegate { };
        public event Action<Vector3> ExplodedReceived = delegate { };

        private NetworkManager _listeningNetwork;

        public void StartListening() {
            var network = NetworkManagerLookup.Find();
            if (network == null) {
                Debug.LogError(MISSING_NETWORK);
                return;
            }
            // Subscribing before the client connects is fine; PurrNet keeps the subscription until it does.
            _listeningNetwork = network;
            network.Subscribe<BombTagState>(OnStateReceived, false);
            network.Subscribe<BombExplodedMessage>(OnExplodedReceived, false);
        }

        public void StopListening() {
            if (_listeningNetwork == null) {
                return;
            }
            _listeningNetwork.Unsubscribe<BombTagState>(OnStateReceived, false);
            _listeningNetwork.Unsubscribe<BombExplodedMessage>(OnExplodedReceived, false);
            _listeningNetwork = null;
        }

        public void Send(BombTagState state) {
            var network = NetworkManagerLookup.Find();
            if (network == null || !network.isServer) {
                return;
            }
            network.SendToAll(state);
        }

        // The host raises it here, so it doesn't matter whether its own client also gets the broadcast.
        public void SendExploded(Vector3 position) {
            var network = NetworkManagerLookup.Find();
            if (network == null || !network.isServer) {
                return;
            }
            network.SendToAll(new BombExplodedMessage(position));
            ExplodedReceived.Invoke(position);
        }

        private void OnStateReceived(PlayerID sender, BombTagState state, bool asServer) {
            StateReceived.Invoke(state);
        }

        // Ignored on the host, which has already raised it in SendExploded; an explosion shown twice would be seen.
        private void OnExplodedReceived(PlayerID sender, BombExplodedMessage message, bool asServer) {
            if (_listeningNetwork != null && _listeningNetwork.isServer) {
                return;
            }
            ExplodedReceived.Invoke(message.Position);
        }
    }
}
