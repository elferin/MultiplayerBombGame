using Code.Util;
using JetBrains.Annotations;

namespace Code.Powerup {

    [UsedImplicitly]
    public class PowerupsFactory : IPowerupsFactory {

        private readonly IPowerupPlayers _players;
        private readonly IPowerupItems _items;
        private readonly IRandom _random;
        private readonly IPowerupObstacles _obstacles;
        private readonly PowerupSettings _settings;

        public PowerupsFactory(IPowerupPlayers players, IPowerupItems items, IRandom random, IPowerupObstacles obstacles,
            PowerupSettings settings) {
            _players = players;
            _items = items;
            _random = random;
            _obstacles = obstacles;
            _settings = settings;
        }

        public IPowerups Create() {
            return new PowerupField(_players, _items, _random, _obstacles, _settings);
        }
    }
}
