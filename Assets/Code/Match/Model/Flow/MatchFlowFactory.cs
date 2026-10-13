namespace Code.Match {

    public class MatchFlowFactory : IMatchFlowFactory {

        private readonly IMatchViewControllerFactory _viewControllerFactory;
        private readonly IMatchNetwork _network;
        private readonly MatchPlayerSpawner _playerSpawner;
        private readonly IPlayerSpawning _spawning;
        private readonly IMatchStatus _status;

        public MatchFlowFactory(IMatchViewControllerFactory viewControllerFactory, IMatchNetwork network,
            MatchPlayerSpawner playerSpawner, IPlayerSpawning spawning, IMatchStatus status) {
            _viewControllerFactory = viewControllerFactory;
            _network = network;
            _playerSpawner = playerSpawner;
            _spawning = spawning;
            _status = status;
        }

        public IMatchFlow Create(MatchFlowArgs args) {
            return new MatchFlow(args, _viewControllerFactory, _network, _playerSpawner, _spawning, _status);
        }
    }
}
