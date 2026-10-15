using Code.DependancyInjection;
using VContainer;

namespace Code.FlowSystem {
    public static class FlowSystemExports {

        // Hands the system's instantiator for TData to the FlowManager once the container is built.
        public static void RegisterFlow<TResult, TData>(this SystemExports exports) where TResult : UniTaskResult where TData : IFlowData<TResult> {
            
            exports.OnContainerBuilt((systemScope, container) =>
                container.Resolve<IFlowRegistry>().Register(systemScope.Resolve<IFlowInstantiator<TResult, TData>>()));
        }
    }
}