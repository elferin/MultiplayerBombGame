using System.Collections.Generic;
using Code.Match;
using PurrNet;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Powerup {

    // On the player prefab: the power-up this player holds and whether they're shielded, both set by the server.
    // The owner uses it with Space: movement effects happen on their client straight away, then the server is told.
    [RequireComponent(typeof(MatchPlayer))]
    public class PowerupHolder : NetworkBehaviour, IHasId {

        // Every player object in the scene, so the host doesn't search for them each frame.
        public static IReadOnlyList<PowerupHolder> Active => Registry.Items;
        private static readonly ActiveRegistry<PowerupHolder> Registry = new();

        [SerializeField] private PowerupSettings settings;
        [SerializeField] private InputActionReference useAction;
        [SerializeField] private Renderer heldMarker;
        [SerializeField] private MeshFilter heldMarkerShape;
        [SerializeField] private GameObject shieldMarker;
        // Sparkles when this player picks a power-up up.
        [SerializeField] private ParticleSystem pickupEffect;
        [SerializeField] private SyncVar<PowerupType> held = new();
        [SerializeField] private SyncVar<bool> shielded = new();

        private MatchPlayer _matchPlayer;
        private PlayerController _controller;
        private bool _hasEnabledInput;
        private readonly PowerupUseGuard _useGuard = new();
        // Set on the server by the owner's request; the host's PowerupField takes it.
        private PowerupType _usedOnServer = PowerupType.None;

        // Read each time rather than copied in Awake: MatchPlayer's Awake, which sets it, may run after this one's.
        public int PlayerId => _matchPlayer.PlayerId;
        int IHasId.Id => PlayerId;
        public PowerupType Held => held.value;
        public bool IsShielded => shielded.value;

        private void Awake() {
            _matchPlayer = GetComponent<MatchPlayer>();
            _controller = GetComponent<PlayerController>();
            held.onChanged += OnHeldChanged;
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
            held.onChanged -= OnHeldChanged;
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

            if (!useAction.action.WasPressedThisFrame()) {
                return;
            }
            if (CanUse()) {
                Use(held.value);
            }
            else if (_useGuard.ShouldResend(Time.time)) {
                // The host hasn't answered; the effect already ran here, so only the request is sent again.
                SendUse(held.value);
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
            return _useGuard.CanUse(held.value) && (_controller == null || _controller.CanMove());
        }

        private void Use(PowerupType type) {
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
            SendUse(type);
        }

        // A repeated request is ignored: the host has already cleared what the player holds.
        private void SendUse(PowerupType type) {
            _useGuard.MarkSent(Time.time);
            UsePowerupOnServer(type);
        }

        [ServerRpc]
        private void UsePowerupOnServer(PowerupType type) {
            _usedOnServer = type;
        }

        // Only a change sparkles, not the value a late-joining client starts with. Using one sets it back to None.
        private void OnHeldChanged(PowerupType value) {
            ShowHeld(value);
            if (value != PowerupType.None) {
                pickupEffect.Play();
            }
        }

        private void ShowHeld(PowerupType value) {
            _useGuard.OnHeldChanged();
            var material = settings.GetMaterial(value);
            heldMarker.gameObject.SetActive(material != null);
            if (material != null) {
                heldMarker.sharedMaterial = material;
                heldMarkerShape.sharedMesh = settings.GetMesh(value);
            }
        }

        private void ShowShield(bool value) {
            shieldMarker.SetActive(value);
        }
    }
}
