using Code.FlowSystem;

namespace Code.Match {

    public class MatchFlowFactory : IMatchFlowFactory {

        private readonly IMatchViewControllerFactory _viewControllerFactory;
        private readonly IMatchNetwork _network;
        private readonly MatchPlayerSpawner _playerSpawner;
        private readonly IPlayerSpawning _spawning;
        private readonly IMatchArena _arena;
        private readonly ILocalPlayer _localPlayer;
        private readonly IMatchStatus _status;
        private readonly IFlowRunner _flowRunner;

        public MatchFlowFactory(IMatchViewControllerFactory viewControllerFactory, IMatchNetwork network,
            MatchPlayerSpawner playerSpawner, IPlayerSpawning spawning, IMatchArena arena, ILocalPlayer localPlayer,
            IMatchStatus status, IFlowRunner flowRunner) {
            _viewControllerFactory = viewControllerFactory;
            _network = network;
            _playerSpawner = playerSpawner;
            _spawning = spawning;
            _arena = arena;
            _localPlayer = localPlayer;
            _status = status;
            _flowRunner = flowRunner;
        }

        public IMatchFlow Create(MatchFlowData data) {
            return new MatchFlow(data, _viewControllerFactory, _network, _playerSpawner, _spawning, _arena, _localPlayer, _status,
                _flowRunner);
        }
    }
}
