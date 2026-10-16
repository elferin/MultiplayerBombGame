using Code.DependancyInjection;
using VContainer;

namespace Code.Util {

    // Helpers that don't belong to one game system.
    public class UtilSystemInstaller : SystemInstaller {

        protected override void InstallInternal(IContainerBuilder builder) {
            builder.Register<IClipboard, Clipboard>(Lifetime.Singleton);
            builder.Register<IRandom, UnityRandom>(Lifetime.Singleton);
        }

        protected override void InstallPublic(SystemExports exports) {
            // The lobby copies its code; the main menu pastes it.
            exports.Expose<IClipboard>();
            // Bomb tag picks who gets the bomb and how long its fuse is.
            exports.Expose<IRandom>();
        }

    }
}
