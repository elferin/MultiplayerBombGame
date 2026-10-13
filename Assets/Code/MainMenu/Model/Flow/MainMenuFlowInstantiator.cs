using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Code.MainMenu {

    [UsedImplicitly]
    public class MainMenuFlowInstantiator {

        private readonly IMainMenuFlowFactory _flowFactory;

        public MainMenuFlowInstantiator(IMainMenuFlowFactory flowFactory) {
            _flowFactory = flowFactory;
        }

        public UniTask<MainMenuFlowResult> StartFlow(CancellationToken cancellation) {
            return _flowFactory.Create().RunAsync(cancellation);
        }
    }
}
