using Code.DependancyInjection;
using Code.FlowSystem;
using VContainer;

namespace Code.Match {

    public class MatchSystemInstaller : SystemInstaller {

        protected override void InstallInternal(IContainerBuilder builder) {
            builder.Register<IMatchNetwork, MatchNetwork>(Lifetime.Singleton);
            builder.Register<IPlayerSpawning, PlayerSpawning>(Lifetime.Singleton);
            builder.Register<ILocalPlayer, LocalPlayer>(Lifetime.Singleton);
            builder.Register<MatchPlayerSpawner>(Lifetime.Singleton);

            // Match state, registered as its getter, setter and combined interface.
            builder.Register<MatchStatusHolder>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.Register<IViewDataGatherer<MatchViewData>, MatchViewDataGatherer>(Lifetime.Singleton);
            builder.Register<MatchAssetLoader>(Lifetime.Singleton);
            builder.Register<IMatchViewControllerFactory, MatchViewControllerFactory>(Lifetime.Singleton);
            builder.Register<IMatchFlowFactory, MatchFlowFactory>(Lifetime.Singleton);
            builder.Register<MatchFlowInstantiator>(Lifetime.Singleton).AsImplementedInterfaces();
        }

        protected override void InstallPublic(SystemExports exports) {
            exports.RegisterFlow<MatchFlowResult, MatchFlowData>();
        }

    }
}
