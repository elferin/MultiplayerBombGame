using Code.DependancyInjection;
using VContainer;

namespace Code.Match {

    public class MatchSystemInstaller : SystemInstaller {

        protected override void InstallInternal(IContainerBuilder builder) {
            builder.Register<IMatchNetwork, MatchNetwork>(Lifetime.Singleton);
            builder.Register<IPlayerSpawning, PlayerSpawning>(Lifetime.Singleton);
            builder.Register<MatchPlayerSpawner>(Lifetime.Singleton);

            // Match state, registered as its getter, setter and combined interface.
            builder.Register<MatchStatusHolder>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.Register<IViewDataGatherer<MatchViewData>, MatchViewDataGatherer>(Lifetime.Singleton);
            builder.Register<MatchAssetLoader>(Lifetime.Singleton);
            builder.Register<IMatchViewControllerFactory, MatchViewControllerFactory>(Lifetime.Singleton);
            builder.Register<IMatchFlowFactory, MatchFlowFactory>(Lifetime.Singleton);
            builder.Register<MatchFlowInstantiator>(Lifetime.Singleton);
        }

        protected override void InstallPublic(SystemExports exports) {
            exports.Expose<MatchFlowInstantiator>();
            // GamePresenter spawns the players once the match has started.
            exports.Expose<MatchPlayerSpawner>();
        }

    }
}
