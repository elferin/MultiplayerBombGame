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

        public UniTask<LobbyFlowResult> StartFlow(LobbyFlowArgs args, CancellationToken cancellation) {
            return _flowFactory.Create(args).RunAsync(cancellation);
        }
    }
}
