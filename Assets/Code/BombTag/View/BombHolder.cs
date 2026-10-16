using System.Collections.Generic;
using PurrNet;
using UnityEngine;

namespace Code.BombTag {

    // On the player prefab: shows the bomb on whoever has it. Only the server sets it.
    public class BombHolder : NetworkBehaviour {

        // Every player object in the scene, so the host's rules don't have to search for them each frame.
        public static IReadOnlyList<BombHolder> Active => ActiveHolders;
        private static readonly List<BombHolder> ActiveHolders = new List<BombHolder>();
        // Starts at 1, so 0 can mean nobody.
        private static int _lastPlayerId;

        [SerializeField] private GameObject bombMarker;
        [SerializeField] private SyncVar<bool> hasBomb = new SyncVar<bool>();

        // Only known on this machine, so it's not sent over the network.
        public int PlayerId { get; private set; }

        private void Awake() {
            PlayerId = ++_lastPlayerId;
            hasBomb.onChanged += ShowBomb;
            ShowBomb(hasBomb.value);
        }

        private void OnEnable() {
            ActiveHolders.Add(this);
        }

        private void OnDisable() {
            ActiveHolders.Remove(this);
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            hasBomb.onChanged -= ShowBomb;
        }

        public void SetHasBomb(bool value) {
            hasBomb.value = value;
        }

        private void ShowBomb(bool value) {
            bombMarker.SetActive(value);
        }
    }
}
