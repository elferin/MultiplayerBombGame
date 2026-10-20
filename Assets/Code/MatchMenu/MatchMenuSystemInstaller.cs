using Code.DependancyInjection;
using Code.FlowSystem;
using VContainer;

namespace Code.MatchMenu {

    // The menu a player opens with Esc during a match: continue playing or leave. The match keeps running behind it.
    public class MatchMenuSystemInstaller : SystemInstaller {

        protected override void InstallInternal(IContainerBuilder builder) {
            builder.Register<MatchMenuAssetLoader>(Lifetime.Singleton);
            builder.Register<IMatchMenuViewControllerFactory, MatchMenuViewControllerFactory>(Lifetime.Singleton);
            builder.Register<IMatchMenuFlowFactory, MatchMenuFlowFactory>(Lifetime.Singleton);
            builder.Register<MatchMenuFlowInstantiator>(Lifetime.Singleton).AsImplementedInterfaces();
        }

        protected override void InstallPublic(SystemExports exports) {
            exports.RegisterFlow<MatchMenuFlowResult, MatchMenuFlowData>();
        }
    }
}
