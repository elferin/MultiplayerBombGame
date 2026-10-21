using UnityEngine;

namespace Code.Powerup {

    // Tuning for power-ups, edited in Addressables/Match/PowerupSettings.asset and loaded through Addressables.
    [CreateAssetMenu(fileName = "PowerupSettings", menuName = "Game/Powerup Settings")]
    public class PowerupSettings : ScriptableObject {

        [Header("Spawning")]
        [SerializeField] private PowerupPickup pickupPrefab;
        [Tooltip("Seconds after a pickup before the next power-up appears.")]
        [SerializeField] private float spawnDelaySeconds = 5f;
        [Tooltip("Power-ups appear within this distance of the arena centre on both axes.")]
        [SerializeField] private float arenaHalfSize = 7f;
        [SerializeField] private float minDistanceFromPlayers = 2f;
        [SerializeField] private float pickupRadius = 1f;

        [Header("Effects")]
        [SerializeField] private float dashSpeed = 16f;
        [SerializeField] private float shieldSeconds = 4f;
        [SerializeField] private float speedBoostMultiplier = 1.6f;
        [SerializeField] private float speedBoostSeconds = 3f;

        [Header("Looks")]
        [Tooltip("Shared by every pickup and held marker of that type, so they batch together.")]
        [SerializeField] private Material dashMaterial;
        [SerializeField] private Material shieldMaterial;
        [SerializeField] private Material speedBoostMaterial;
        [SerializeField] private Mesh dashMesh;
        [SerializeField] private Mesh shieldMesh;
        [SerializeField] private Mesh speedBoostMesh;

        public PowerupPickup PickupPrefab => pickupPrefab;
        public float SpawnDelaySeconds => spawnDelaySeconds;
        public float ArenaHalfSize => arenaHalfSize;
        public float MinDistanceFromPlayers => minDistanceFromPlayers;
        public float PickupRadius => pickupRadius;
        public float DashSpeed => dashSpeed;
        public float ShieldSeconds => shieldSeconds;
        public float SpeedBoostMultiplier => speedBoostMultiplier;
        public float SpeedBoostSeconds => speedBoostSeconds;

        // Null for None.
        public Material GetMaterial(PowerupType type) {
            switch (type) {
                case PowerupType.Dash:
                    return dashMaterial;
                case PowerupType.Shield:
                    return shieldMaterial;
                case PowerupType.SpeedBoost:
                    return speedBoostMaterial;
                default:
                    return null;
            }
        }

        // Null for None.
        public Mesh GetMesh(PowerupType type) {
            switch (type) {
                case PowerupType.Dash:
                    return dashMesh;
                case PowerupType.Shield:
                    return shieldMesh;
                case PowerupType.SpeedBoost:
                    return speedBoostMesh;
                default:
                    return null;
            }
        }
    }
}
