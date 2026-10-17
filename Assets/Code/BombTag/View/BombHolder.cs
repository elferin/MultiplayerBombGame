using System.Collections.Generic;
using Code.Match;
using Code.Powerup;
using PurrNet;
using UnityEngine;

namespace Code.BombTag {

    // On the player prefab: shows the bomb on whoever has it. Only the server sets it.
    public class BombHolder : NetworkBehaviour, IHasId {

        // Every player object in the scene, so the host's rules don't have to search for them each frame.
        public static IReadOnlyList<BombHolder> Active => Registry.Items;
        private static readonly ActiveRegistry<BombHolder> Registry = new();

        [SerializeField] private GameObject bombMarker;
        [SerializeField] private SyncVar<bool> hasBomb = new SyncVar<bool>();

        // The player's other components, found once, because the host reads them every frame.
        private MatchPlayer _matchPlayer;
        private PowerupHolder _powerupHolder;

        // Only known on this machine, so it's not sent over the network.
        public int PlayerId { get; private set; }
        int IHasId.Id => PlayerId;
        public MatchPlayer MatchPlayer => _matchPlayer;
        public string PlayerName => _matchPlayer != null ? _matchPlayer.PlayerName : string.Empty;
        public bool IsShielded => _powerupHolder != null && _powerupHolder.IsShielded;

        private void Awake() {
            PlayerId = Registry.NextId();
            _matchPlayer = GetComponent<MatchPlayer>();
            _powerupHolder = GetComponent<PowerupHolder>();
            hasBomb.onChanged += ShowBomb;
            ShowBomb(hasBomb.value);
        }

        private void OnEnable() {
            Registry.Add(this);
        }

        private void OnDisable() {
            Registry.Remove(this);
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
