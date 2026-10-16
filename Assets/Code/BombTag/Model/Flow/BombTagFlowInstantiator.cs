using System.Threading;
using Code.FlowSystem;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Code.BombTag {

    [UsedImplicitly]
    public class BombTagFlowInstantiator : IFlowInstantiator<BombTagFlowResult, BombTagFlowData> {

        private readonly IBombTagFlowFactory _flowFactory;

        public BombTagFlowInstantiator(IBombTagFlowFactory flowFactory) {
            _flowFactory = flowFactory;
        }

        public UniTask<BombTagFlowResult> StartFlow(BombTagFlowData data, CancellationToken cancellation) {
            return _flowFactory.Create(data).RunAsync(cancellation);
        }
    }
}
