using Code.DependancyInjection;
using Code.FlowSystem;
using VContainer;

namespace Code.BombTag {

    // The game mode played during a match: pass the bomb on touch, the last player left wins.
    public class BombTagSystemInstaller : SystemInstaller {

        protected override void InstallInternal(IContainerBuilder builder) {
            builder.Register<IBombTagSync, BombTagSync>(Lifetime.Singleton);
            builder.Register<IBombTagPlayers, BombTagPlayers>(Lifetime.Singleton);

            // Registered as its getter, setter and combined interface.
            builder.Register<BombTagStatusHolder>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.Register<IViewDataGatherer<BombTagViewData>, BombTagViewDataGatherer>(Lifetime.Singleton);
            builder.Register<BombTagAssetLoader>(Lifetime.Singleton);
            builder.Register<IBombTagViewControllerFactory, BombTagViewControllerFactory>(Lifetime.Singleton);
            builder.Register<IBombTagFlowFactory, BombTagFlowFactory>(Lifetime.Singleton);
            builder.Register<BombTagFlowInstantiator>(Lifetime.Singleton).AsImplementedInterfaces();
        }

        protected override void InstallPublic(SystemExports exports) {
            exports.RegisterFlow<BombTagFlowResult, BombTagFlowData>();
        }
    }
}
