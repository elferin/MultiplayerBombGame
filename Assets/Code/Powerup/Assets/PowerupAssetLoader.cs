using Code.Util;
using JetBrains.Annotations;

namespace Code.Powerup {

    [UsedImplicitly]
    public class PowerupAssetLoader {

        private const string SETTINGS_ADDRESS = "PowerupSettings";

        private readonly IAddressableAssets _assets;

        public PowerupAssetLoader(IAddressableAssets assets) {
            _assets = assets;
        }

        // Loaded once, when the container first needs it; small enough to load synchronously.
        public PowerupSettings LoadSettings() {
            return _assets.LoadNow<PowerupSettings>(SETTINGS_ADDRESS);
        }
    }
}
