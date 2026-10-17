using System;
using Code.Powerup;
using Code.Util;

namespace Code.BombTag {

    public class BombTagFlowFactory : IBombTagFlowFactory {

        private static readonly TimeSpan WINNER_SCREEN_TIME = TimeSpan.FromSeconds(3);

        private readonly IBombTagViewControllerFactory _viewControllerFactory;
        private readonly IBombTagSync _sync;
        private readonly IBombTagPlayers _players;
        private readonly IBombTagStatus _status;
        private readonly IRandom _random;
        private readonly IPowerupsFactory _powerupsFactory;

        public BombTagFlowFactory(IBombTagViewControllerFactory viewControllerFactory, IBombTagSync sync,
            IBombTagPlayers players, IBombTagStatus status, IRandom random, IPowerupsFactory powerupsFactory) {
            _viewControllerFactory = viewControllerFactory;
            _sync = sync;
            _players = players;
            _status = status;
            _random = random;
            _powerupsFactory = powerupsFactory;
        }

        public IBombTagFlow Create(BombTagFlowData data) {
            // A new round for every match, so nothing carries over from the last one.
            return new BombTagFlow(data, _viewControllerFactory, _sync, _players, _status, new BombTagRound(_players, _random, _powerupsFactory.Create()),
                WINNER_SCREEN_TIME);
        }
    }
}
