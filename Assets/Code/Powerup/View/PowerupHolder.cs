using System.Collections.Generic;
using Code.Match;
using PurrNet;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Powerup {

    // On the player prefab: the power-up this player holds and whether they're shielded, both set by the server.
    // The owner uses it with Space: movement effects happen on their client straight away, then the server is told.
    public class PowerupHolder : NetworkBehaviour, IHasId {

        // Every player object in the scene, so the host doesn't search for them each frame.
        public static IReadOnlyList<PowerupHolder> Active => Registry.Items;
        private static readonly ActiveRegistry<PowerupHolder> Registry = new();

        [SerializeField] private PowerupSettings settings;
        [SerializeField] private InputActionReference useAction;
        [SerializeField] private Renderer heldMarker;
        [SerializeField] private GameObject shieldMarker;
        [SerializeField] private SyncVar<PowerupType> held = new();
        [SerializeField] private SyncVar<bool> shielded = new();

        private PlayerController _controller;
        private bool _hasEnabledInput;
        private readonly PowerupUseGuard _useGuard = new();
        // Set on the server by the owner's request; the host's PowerupField takes it.
        private PowerupType _usedOnServer = PowerupType.None;

        // Only known on this machine, so it's not sent over the network.
        public int PlayerId { get; private set; }
        int IHasId.Id => PlayerId;
        public PowerupType Held => held.value;
        public bool IsShielded => shielded.value;

        private void Awake() {
            PlayerId = Registry.NextId();
            _controller = GetComponent<PlayerController>();
            held.onChanged += ShowHeld;
            shielded.onChanged += ShowShield;
            ShowHeld(held.value);
            ShowShield(shielded.value);
        }

        private void OnEnable() {
            Registry.Add(this);
        }

        private void OnDisable() {
            Registry.Remove(this);
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            held.onChanged -= ShowHeld;
            shielded.onChanged -= ShowShield;
        }

        // Only works on the server.
        public void SetHeld(PowerupType value) {
            held.value = value;
        }

        // Only works on the server.
        public void SetShielded(bool value) {
            shielded.value = value;
        }

        // On the server: the power-up the owner used since the last call, or None.
        public PowerupType TakeUsed() {
            var used = _usedOnServer;
            _usedOnServer = PowerupType.None;
            return used;
        }

        private void Update() {
            if (!isOwner) {
                return;
            }

            // The action is shared by every player object, so only the owner turns it on.
            if (!_hasEnabledInput) {
                useAction.action.Enable();
                _hasEnabledInput = true;
            }

            if (useAction.action.WasPressedThisFrame() && CanUse()) {
                Use(held.value);
            }
        }

        protected override void OnDespawned() {
            base.OnDespawned();
            if (_hasEnabledInput) {
                useAction.action.Disable();
                _hasEnabledInput = false;
            }
        }

        private bool CanUse() {
            return _useGuard.CanUse(held.value, Time.time) && (_controller == null || _controller.CanMove());
        }

        private void Use(PowerupType type) {
            _useGuard.MarkSent(Time.time);
            // Without a PlayerController there is nothing to move; the power-up is still used up.
            if (_controller != null) {
                switch (type) {
                    case PowerupType.Dash:
                        _controller.Dash(settings.DashSpeed);
                        break;
                    case PowerupType.SpeedBoost:
                        _controller.Boost(settings.SpeedBoostMultiplier, settings.SpeedBoostSeconds);
                        break;
                }
            }
            // The shield is the server's job, because the bomb rules run there.
            UsePowerupOnServer(type);
        }

        [ServerRpc]
        private void UsePowerupOnServer(PowerupType type) {
            _usedOnServer = type;
        }

        private void ShowHeld(PowerupType value) {
            _useGuard.OnHeldChanged();
            var material = settings.GetMaterial(value);
            heldMarker.gameObject.SetActive(material != null);
            if (material != null) {
                heldMarker.sharedMaterial = material;
            }
        }

        private void ShowShield(bool value) {
            shieldMarker.SetActive(value);
        }
    }
}
