using JetBrains.Annotations;
using UnityEngine;

namespace Code.Powerup {

    [UsedImplicitly]
    public class PowerupAssetLoader {

        private const string SETTINGS_PATH = "PowerupSettings";

        // Loaded once, when the container first needs it; small enough to load synchronously.
        public PowerupSettings LoadSettings() {
            return Resources.Load<PowerupSettings>(SETTINGS_PATH);
        }
    }
}
