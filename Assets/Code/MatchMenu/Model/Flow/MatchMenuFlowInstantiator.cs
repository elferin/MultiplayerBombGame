using System.Threading;
using Code.FlowSystem;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Code.MatchMenu {

    [UsedImplicitly]
    public class MatchMenuFlowInstantiator : IFlowInstantiator<MatchMenuFlowResult, MatchMenuFlowData> {

        private readonly IMatchMenuFlowFactory _flowFactory;

        public MatchMenuFlowInstantiator(IMatchMenuFlowFactory flowFactory) {
            _flowFactory = flowFactory;
        }

        public UniTask<MatchMenuFlowResult> StartFlow(MatchMenuFlowData data, CancellationToken cancellation) {
            return _flowFactory.Create(data).RunAsync(cancellation);
        }
    }
}
