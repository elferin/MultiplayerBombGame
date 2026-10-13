using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Code.Match {

    [UsedImplicitly]
    public class MatchFlowInstantiator {

        private readonly IMatchFlowFactory _flowFactory;

        public MatchFlowInstantiator(IMatchFlowFactory flowFactory) {
            _flowFactory = flowFactory;
        }

        public UniTask<MatchFlowResult> StartFlow(MatchFlowArgs args, CancellationToken cancellation) {
            return _flowFactory.Create(args).RunAsync(cancellation);
        }
    }
}
