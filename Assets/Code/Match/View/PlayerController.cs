using PurrNet;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Match {

    // On the player prefab: moves the character on its owner's client. Other clients follow through NetworkRigidbody.
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : NetworkBehaviour {

        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private float maxSpeed = 6f;
        [SerializeField] private float acceleration = 40f;

        private Rigidbody _body;
        private MatchPlayer _matchPlayer;
        private Vector2 _moveInput;
        private bool _hasEnabledInput;
        // Top-down: screen up is world +Z, so a player who never moved dashes up the screen.
        private Vector3 _lastDirection = Vector3.forward;
        private float _speedMultiplier = 1f;
        private float _boostSecondsLeft;

        private void Awake() {
            _body = GetComponent<Rigidbody>();
            _matchPlayer = GetComponent<MatchPlayer>();
        }

        // Ownership is given right after spawning, so it's checked every frame rather than once.
        private void Update() {
            if (!isOwner) {
                return;
            }

            // The action is shared by every player object, so only the owner turns it on.
            if (!_hasEnabledInput) {
                moveAction.action.Enable();
                _hasEnabledInput = true;
            }

            // When stopped, the usual acceleration brakes the character instead of freezing it mid-slide.
            _moveInput = CanMove() ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
            if (_moveInput.sqrMagnitude > 0.01f) {
                _lastDirection = new Vector3(_moveInput.x, 0f, _moveInput.y).normalized;
            }

            TickBoost();
        }

        private void FixedUpdate() {
            if (!isOwner) {
                return;
            }

            var target = new Vector3(_moveInput.x, 0f, _moveInput.y) * (maxSpeed * _speedMultiplier);
            var planar = GetPlanarVelocity();
            var change = Vector3.MoveTowards(planar, target, acceleration * Time.fixedDeltaTime) - planar;
            _body.AddForce(change, ForceMode.VelocityChange);
        }

        public bool CanMove() {
            return _matchPlayer == null || _matchPlayer.CanMove;
        }

        // A burst in the last direction moved; the usual acceleration slows it back down. Only works on the owner.
        public void Dash(float speed) {
            _body.AddForce(_lastDirection * speed - GetPlanarVelocity(), ForceMode.VelocityChange);
        }

        // Raises the top speed for a while. Only works on the owner.
        public void Boost(float multiplier, float seconds) {
            _speedMultiplier = multiplier;
            _boostSecondsLeft = seconds;
        }

        protected override void OnDespawned() {
            base.OnDespawned();
            // isOwner may already be false here, so this remembers whether this object turned input on.
            if (_hasEnabledInput) {
                moveAction.action.Disable();
                _hasEnabledInput = false;
            }
        }

        private void TickBoost() {
            if (_boostSecondsLeft <= 0f) {
                return;
            }
            _boostSecondsLeft -= Time.deltaTime;
            if (_boostSecondsLeft <= 0f) {
                _speedMultiplier = 1f;
            }
        }

        private Vector3 GetPlanarVelocity() {
            var velocity = _body.linearVelocity;
            return new Vector3(velocity.x, 0f, velocity.z);
        }
    }
}
