using System;
using System.Collections.Generic;
using PurrNet;
using TMPro;
using UnityEngine;

namespace Code.Match {

    // On the player prefab: the player's id and name, shown above the character to everyone.
    // The other player components use this id, so every system knows a player by the same one.
    public class MatchPlayer : NetworkBehaviour, IHasId {

        // Every player object in the scene, so finding one doesn't search the whole scene.
        public static IReadOnlyList<MatchPlayer> Active => Registry.Items;
        private static readonly ActiveRegistry<MatchPlayer> Registry = new();
        // This client became the owner of a player object, i.e. its own character appeared. Static, like Active,
        // because scene objects can't be injected; may be raised more than once for the same object.
        public static event Action<MatchPlayer> OwnedLocally = delegate { };

        [SerializeField] private TMP_Text nameLabel;
        // The owner sets it once spawned, because only their client knows the name.
        [SerializeField] private SyncVar<string> playerName = new SyncVar<string>(string.Empty, ownerAuth: true);

        // Local only: each client stops its own character, e.g. when the match is over.
        private bool _canMove = true;
        // Local only, set from outside (e.g. while a menu is open), and kept apart from _canMove so unblocking
        // input on the winner screen doesn't free the character.
        private bool _isInputBlocked;

        // Only known on this machine, so it's not sent over the network.
        public int PlayerId { get; private set; }
        int IHasId.Id => PlayerId;
        public string PlayerName => playerName.value ?? string.Empty;
        public bool CanMove => _canMove && !_isInputBlocked;

        private void Awake() {
            PlayerId = Registry.NextId();
            playerName.onChanged += ShowName;
            ShowName(playerName.value);
        }

        private void OnEnable() {
            Registry.Add(this);
        }

        private void OnDisable() {
            Registry.Remove(this);
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            playerName.onChanged -= ShowName;
        }

        // Only works on the owner's client.
        public void SetPlayerName(string value) {
            playerName.value = value;
        }

        public void SetCanMove(bool canMove) {
            _canMove = canMove;
        }

        public void SetInputBlocked(bool isInputBlocked) {
            _isInputBlocked = isInputBlocked;
        }

        // Ownership can arrive with the spawn or right after it, so both are checked.
        protected override void OnSpawned(bool asServer) {
            base.OnSpawned(asServer);
            if (!asServer && isOwner) {
                OwnedLocally.Invoke(this);
            }
        }

        protected override void OnOwnerChanged(PlayerID? oldOwner, PlayerID? newOwner, bool asServer) {
            base.OnOwnerChanged(oldOwner, newOwner, asServer);
            if (!asServer && isOwner) {
                OwnedLocally.Invoke(this);
            }
        }

        // Keeps the label facing the camera however the player moves.
        private void LateUpdate() {
            var view = Camera.main;
            if (view != null) {
                nameLabel.transform.rotation = view.transform.rotation;
            }
        }

        private void ShowName(string value) {
            nameLabel.text = value;
        }
    }
}
