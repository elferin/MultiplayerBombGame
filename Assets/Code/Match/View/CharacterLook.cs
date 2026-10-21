using PurrNet;
using UnityEngine;

namespace Code.Match {

    // On the player prefab: which character the player looks like, which way it faces and how fast it runs.
    // The character comes from the owner's id, which every client knows, so all of them pick the same one.
    // Facing and speed come from how far it moved, so they work the same on every client, whoever moves it.
    public class CharacterLook : NetworkBehaviour {

        private const string SPEED_PARAMETER = "Speed";
        // Below this, in units per second, the character keeps facing where it last went.
        private const float MIN_TURN_SPEED = 0.5f;

        [Tooltip("Turns to face the way the character moves; the variants are its children.")]
        [SerializeField] private Transform pivot;
        [Tooltip("Each one a whole character with its own Animator. One is shown, the rest stay inactive.")]
        [SerializeField] private GameObject[] variants;
        [SerializeField] private float turnDegreesPerSecond = 720f;
        [Tooltip("How quickly the animation follows a change in speed, in seconds.")]
        [SerializeField] private float speedSmoothing = 0.08f;

        private Animator _animator;
        private Vector3 _lastPosition;
        private float _speed;

        private void Awake() {
            ShowVariant(0);
        }

        private void OnEnable() {
            _lastPosition = transform.position;
        }

        protected override void OnSpawned(bool asServer) {
            base.OnSpawned(asServer);
            ShowOwnersVariant();
        }

        protected override void OnOwnerChanged(PlayerID? oldOwner, PlayerID? newOwner, bool asServer) {
            base.OnOwnerChanged(oldOwner, newOwner, asServer);
            ShowOwnersVariant();
        }

        private void LateUpdate() {
            var position = transform.position;
            var moved = position - _lastPosition;
            moved.y = 0f;
            _lastPosition = position;
            if (Time.deltaTime <= 0f) {
                return;
            }

            var speed = moved.magnitude / Time.deltaTime;
            _speed = Mathf.Lerp(_speed, speed, 1f - Mathf.Exp(-Time.deltaTime / speedSmoothing));
            if (_animator != null) {
                _animator.SetFloat(SPEED_PARAMETER, _speed);
            }

            if (speed > MIN_TURN_SPEED) {
                var facing = Quaternion.LookRotation(moved);
                pivot.rotation = Quaternion.RotateTowards(pivot.rotation, facing, turnDegreesPerSecond * Time.deltaTime);
            }
        }

        private void ShowOwnersVariant() {
            if (owner.HasValue) {
                ShowVariant((int)(owner.Value.id.value % (ulong)variants.Length));
            }
        }

        private void ShowVariant(int index) {
            for (var i = 0; i < variants.Length; i++) {
                variants[i].SetActive(i == index);
            }
            _animator = variants[index].GetComponent<Animator>();
        }
    }
}
