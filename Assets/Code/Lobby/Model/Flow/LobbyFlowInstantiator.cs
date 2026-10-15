using System.Threading;
using Code.FlowSystem;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Code.Lobby {

    [UsedImplicitly]
    public class LobbyFlowInstantiator : IFlowInstantiator<LobbyFlowResult, LobbyFlowData> {

        private readonly ILobbyFlowFactory _flowFactory;

        public LobbyFlowInstantiator(ILobbyFlowFactory flowFactory) {
            _flowFactory = flowFactory;
        }

        public UniTask<LobbyFlowResult> StartFlow(LobbyFlowData data, CancellationToken cancellation) {
            return _flowFactory.Create(data).RunAsync(cancellation);
        }
    }
}
