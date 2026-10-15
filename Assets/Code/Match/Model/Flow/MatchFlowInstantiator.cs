using System.Threading;
using Code.FlowSystem;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Code.Match {

    [UsedImplicitly]
    public class MatchFlowInstantiator : IFlowInstantiator<MatchFlowResult, MatchFlowData> {

        private readonly IMatchFlowFactory _flowFactory;

        public MatchFlowInstantiator(IMatchFlowFactory flowFactory) {
            _flowFactory = flowFactory;
        }

        public UniTask<MatchFlowResult> StartFlow(MatchFlowData data, CancellationToken cancellation) {
            return _flowFactory.Create(data).RunAsync(cancellation);
        }
    }
}
