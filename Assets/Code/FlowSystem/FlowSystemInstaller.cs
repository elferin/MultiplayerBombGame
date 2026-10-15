using Code.DependancyInjection;
using VContainer;

namespace Code.FlowSystem {

    public class FlowSystemInstaller : SystemInstaller {

        protected override void InstallInternal(IContainerBuilder builder) {
            builder.Register<FlowManager>(Lifetime.Singleton).AsImplementedInterfaces();
        }

        protected override void InstallPublic(SystemExports exports) {
            // Systems register their flows; GamePresenter runs them.
            exports.Expose<IFlowRegistry>();
            exports.Expose<IFlowRunner>();
        }
    }
}
