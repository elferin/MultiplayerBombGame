using Code.Util;

namespace Code.BombTag {

    public class BombTagFlowFactory : IBombTagFlowFactory {

        private readonly IBombTagViewControllerFactory _viewControllerFactory;
        private readonly IBombTagSync _sync;
        private readonly IBombTagPlayers _players;
        private readonly IBombTagStatus _status;
        private readonly IRandom _random;

        public BombTagFlowFactory(IBombTagViewControllerFactory viewControllerFactory, IBombTagSync sync,
            IBombTagPlayers players, IBombTagStatus status, IRandom random) {
            _viewControllerFactory = viewControllerFactory;
            _sync = sync;
            _players = players;
            _status = status;
            _random = random;
        }

        public IBombTagFlow Create(BombTagFlowData data) {
            // New rules for every match, so nothing carries over from the last one.
            return new BombTagFlow(data, _viewControllerFactory, _sync, _players, _status, new BombTagRules(_players, _random));
        }
    }
}
