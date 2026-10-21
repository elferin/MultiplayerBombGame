using Code.DependancyInjection;
using VContainer;

namespace Code.Powerup {

    // Power-ups that appear in the arena during a round: Dash, Shield and Speed boost.
    public class PowerupSystemInstaller : SystemInstaller {

        protected override void InstallInternal(IContainerBuilder builder) {
            builder.Register<PowerupAssetLoader>(Lifetime.Singleton);
            builder.Register(resolver => resolver.Resolve<PowerupAssetLoader>().LoadSettings(), Lifetime.Singleton);

            // Registered as their narrow and combined interfaces, like the lobby service.
            builder.Register<PowerupPlayers>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<PowerupItems>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<IPowerupObstacles, PowerupObstacles>(Lifetime.Singleton);
            builder.Register<IPowerupsFactory, PowerupsFactory>(Lifetime.Singleton);
        }

        protected override void InstallPublic(SystemExports exports) {
            // The bomb tag round spawns and ticks them.
            exports.Expose<IPowerupsFactory>();
        }
    }
}
