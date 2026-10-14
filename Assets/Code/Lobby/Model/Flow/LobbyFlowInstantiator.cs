using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Code.Lobby {

    [UsedImplicitly]
    public class LobbyFlowInstantiator {

        private readonly ILobbyFlowFactory _flowFactory;

        public LobbyFlowInstantiator(ILobbyFlowFactory flowFactory) {
            _flowFactory = flowFactory;
        }

        public UniTask<LobbyFlowResult> StartFlow(LobbyFlowData data, CancellationToken cancellation) {
            return _flowFactory.Create(data).RunAsync(cancellation);
        }
    }
}
