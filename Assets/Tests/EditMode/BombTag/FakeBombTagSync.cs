using System;
using System.Collections.Generic;
using Code.BombTag;
using UnityEngine;

namespace Code.Tests.BombTag {

    // Records what the host sends; the test plays the part of the network by raising states.
    public class FakeBombTagSync : IBombTagSync {

        public event Action<BombTagState> StateReceived = delegate { };
        public event Action<Vector3> ExplodedReceived = delegate { };

        public List<BombTagState> Sent { get; } = new();
        public List<Vector3> SentExplosions { get; } = new();
        public int StartListeningCount { get; private set; }
        public int StopListeningCount { get; private set; }

        public void StartListening() {
            StartListeningCount++;
        }

        public void StopListening() {
            StopListeningCount++;
        }

        public void Send(BombTagState state) {
            Sent.Add(state);
        }

        public void SendExploded(Vector3 position) {
            SentExplosions.Add(position);
        }

        public void Receive(BombTagState state) => StateReceived(state);

        public void ReceiveExploded(Vector3 position) => ExplodedReceived(position);
    }
}
