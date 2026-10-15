using System.Threading;
using Code.FlowSystem;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Code.MainMenu {

    [UsedImplicitly]
    public class MainMenuFlowInstantiator : IFlowInstantiator<MainMenuFlowResult, MainMenuFlowData> {

        private readonly IMainMenuFlowFactory _flowFactory;

        public MainMenuFlowInstantiator(IMainMenuFlowFactory flowFactory) {
            _flowFactory = flowFactory;
        }

        public UniTask<MainMenuFlowResult> StartFlow(MainMenuFlowData data, CancellationToken cancellation) {
            return _flowFactory.Create(data).RunAsync(cancellation);
        }
    }
}
