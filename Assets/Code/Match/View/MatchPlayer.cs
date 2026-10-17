using System.Collections.Generic;
using PurrNet;
using TMPro;
using UnityEngine;

namespace Code.Match {

    // On the player prefab: the player's name, shown above the character to everyone.
    public class MatchPlayer : NetworkBehaviour {

        // Every player object in the scene, so finding one doesn't search the whole scene.
        public static IReadOnlyList<MatchPlayer> Active => Registry.Items;
        private static readonly ActiveRegistry<MatchPlayer> Registry = new();

        [SerializeField] private TMP_Text nameLabel;
        // The owner sets it once spawned, because only their client knows the name.
        [SerializeField] private SyncVar<string> playerName = new SyncVar<string>(string.Empty, ownerAuth: true);

        public string PlayerName => playerName.value ?? string.Empty;
        // Local only: each client stops its own character, e.g. when the match is over.
        public bool CanMove { get; private set; } = true;

        private void Awake() {
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
            CanMove = canMove;
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
