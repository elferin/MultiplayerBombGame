using Code.DependancyInjection;
using VContainer;

namespace Code.Match {

    public class MatchSystemInstaller : SystemInstaller {

        protected override void InstallInternal(IContainerBuilder builder) {
            builder.Register<IMatchNetwork, MatchNetwork>(Lifetime.Singleton);
            builder.Register<IPlayerSpawning, PlayerSpawning>(Lifetime.Singleton);
            builder.Register<MatchPlayerSpawner>(Lifetime.Singleton);
        }

        protected override void InstallPublic(SystemExports exports) {
            // The lobby starts the match network once the host clicks Start.
            exports.Expose<IMatchNetwork>();
            // GamePresenter spawns the players once the match has started.
            exports.Expose<MatchPlayerSpawner>();
        }

    }
}
