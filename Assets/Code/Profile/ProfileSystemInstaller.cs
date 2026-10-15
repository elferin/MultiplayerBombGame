using Code.DependancyInjection;
using VContainer;

namespace Code.Profile {

    // The local player's own data, kept between sessions.
    public class ProfileSystemInstaller : SystemInstaller {

        protected override void InstallInternal(IContainerBuilder builder) {
            builder.Register<IPlayerNameStorage, PlayerNameStorage>(Lifetime.Singleton);
            // Registered as its getter, setter and combined interface.
            builder.Register<PlayerNameHolder>(Lifetime.Singleton).AsImplementedInterfaces();
        }

        protected override void InstallPublic(SystemExports exports) {
            // The main menu edits the name, so it needs the combined interface and the getter.
            exports.Expose<IPlayerName>();
            exports.Expose<IPlayerNameGetter>();
        }

    }
}
