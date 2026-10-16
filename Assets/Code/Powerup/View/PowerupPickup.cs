using System.Collections.Generic;
using PurrNet;
using UnityEngine;

namespace Code.Powerup {

    // A power-up lying in the arena. The server sets its type; everyone sees it in that type's colour.
    public class PowerupPickup : NetworkBehaviour {

        private const float SPIN_DEGREES_PER_SECOND = 90f;

        // Every pickup in the scene, so the host doesn't search for them each frame.
        public static IReadOnlyList<PowerupPickup> Active => ActivePickups;
        private static readonly List<PowerupPickup> ActivePickups = new();
        // Starts at 1, so 0 can mean nothing.
        private static int _lastPickupId;

        [SerializeField] private PowerupSettings settings;
        [SerializeField] private Renderer look;
        [SerializeField] private SyncVar<PowerupType> type = new();

        // Only known on this machine, so it's not sent over the network.
        public int PickupId { get; private set; }
        public PowerupType Type => type.value;

        private void Awake() {
            PickupId = ++_lastPickupId;
            type.onChanged += ShowType;
            ShowType(type.value);
        }

        private void OnEnable() {
            ActivePickups.Add(this);
        }

        private void OnDisable() {
            ActivePickups.Remove(this);
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            type.onChanged -= ShowType;
        }

        // Only works on the server.
        public void SetType(PowerupType value) {
            type.value = value;
        }

        private void Update() {
            look.transform.Rotate(0f, SPIN_DEGREES_PER_SECOND * Time.deltaTime, 0f, Space.World);
        }

        // Hidden until the server has set a type.
        private void ShowType(PowerupType value) {
            var material = settings.GetMaterial(value);
            look.enabled = material != null;
            if (material != null) {
                look.sharedMaterial = material;
            }
        }
    }
}
