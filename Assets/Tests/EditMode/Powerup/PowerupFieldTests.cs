using Code.Powerup;
using Code.Tests.Util;
using NUnit.Framework;
using UnityEngine;

namespace Code.Tests.Powerup {

    public class PowerupFieldTests {

        private FakePowerupPlayers _players;
        private FakePowerupItems _items;
        private FakeRandom _random;
        private FakePowerupObstacles _obstacles;
        private PowerupSettings _settings;
        private PowerupField _field;

        [SetUp]
        public void SetUp() {
            _players = new FakePowerupPlayers();
            _items = new FakePowerupItems();
            _random = new FakeRandom();
            _obstacles = new FakePowerupObstacles();
            // Default values: 5s spawn delay, 1 unit pickup radius, 4s shield, arena half size 7.
            _settings = ScriptableObject.CreateInstance<PowerupSettings>();
            _field = new PowerupField(_players, _items, _random, _obstacles, _settings);

            _players.Add(1, new Vector3(0f, 1f, 0f));
            _players.Add(2, new Vector3(5f, 1f, 0f));
        }

        [TearDown]
        public void TearDown() {
            Object.DestroyImmediate(_settings);
        }

        [Test]
        public void StartSpawnsARandomPowerupAtARandomSpot() {
            _random.IntOffset = 1;
            _random.FloatFraction = 1f;

            _field.Start();

            Assert.AreEqual(1, _items.SpawnCount);
            var item = _items.GetItems()[0];
            Assert.AreEqual(PowerupType.Shield, item.Type);
            Assert.AreEqual(new Vector3(7f, 0.5f, 7f), item.Position);
        }

        [Test]
        public void SpotInsideCoverIsSkipped() {
            _obstacles.Block(7f, 7f);
            // The first spot tried is (7, 7), the next (-7, -7).
            _random.NextFloatFractions.Enqueue(1f);
            _random.NextFloatFractions.Enqueue(1f);

            _field.Start();

            Assert.AreEqual(new Vector3(-7f, 0.5f, -7f), _items.GetItems()[0].Position);
        }

        [Test]
        public void PlayerRunningOverItPicksItUp() {
            StartWithItemAt(new Vector3(3f, 0.5f, 3f));

            _players.Move(1, new Vector3(3.5f, 1f, 3f));
            _field.Tick(0.1f);

            Assert.AreEqual(PowerupType.Dash, _players.GetHeld(1));
            Assert.AreEqual(0, _items.GetItems().Count);
        }

        [Test]
        public void PlayerOutOfReachDoesNotPickItUp() {
            StartWithItemAt(new Vector3(3f, 0.5f, 3f));

            _players.Move(1, new Vector3(4.5f, 1f, 3f));
            _field.Tick(0.1f);

            Assert.AreEqual(PowerupType.None, _players.GetHeld(1));
            Assert.AreEqual(1, _items.GetItems().Count);
        }

        [Test]
        public void PlayerAlreadyHoldingOneRunsOverIt() {
            StartWithItemAt(new Vector3(3f, 0.5f, 3f));
            _players.SetHeld(1, PowerupType.Shield);

            _players.Move(1, new Vector3(3f, 1f, 3f));
            _field.Tick(0.1f);

            Assert.AreEqual(PowerupType.Shield, _players.GetHeld(1));
            Assert.AreEqual(1, _items.GetItems().Count);
        }

        [Test]
        public void NextOneAppearsAfterTheDelay() {
            StartWithItemAt(new Vector3(3f, 0.5f, 3f));
            _players.Move(1, new Vector3(3f, 1f, 3f));
            _field.Tick(0f);

            _field.Tick(4.9f);
            Assert.AreEqual(1, _items.SpawnCount);

            _field.Tick(0.2f);
            Assert.AreEqual(2, _items.SpawnCount);
        }

        [Test]
        public void NoNewOneWhileOneIsLyingInTheArena() {
            StartWithItemAt(new Vector3(3f, 0.5f, 3f));

            _field.Tick(20f);

            Assert.AreEqual(1, _items.SpawnCount);
        }

        [Test]
        public void UsingItClearsWhatThePlayerHolds() {
            _players.SetHeld(1, PowerupType.Dash);
            _players.RequestUse(1, PowerupType.Dash);

            _field.Tick(0.1f);

            Assert.AreEqual(PowerupType.None, _players.GetHeld(1));
            Assert.AreEqual(0, _players.Shielded.Count);
        }

        [Test]
        public void UsingOneThePlayerDoesNotHoldIsIgnored() {
            _players.SetHeld(1, PowerupType.Dash);
            _players.RequestUse(1, PowerupType.Shield);

            _field.Tick(0.1f);

            Assert.AreEqual(PowerupType.Dash, _players.GetHeld(1));
            Assert.AreEqual(0, _players.Shielded.Count);
        }

        [Test]
        public void ShieldLastsForItsDuration() {
            _players.SetHeld(1, PowerupType.Shield);
            _players.RequestUse(1, PowerupType.Shield);

            _field.Tick(0f);
            Assert.IsTrue(_players.Shielded.Contains(1));

            _field.Tick(3.9f);
            Assert.IsTrue(_players.Shielded.Contains(1));

            _field.Tick(0.2f);
            Assert.IsFalse(_players.Shielded.Contains(1));
        }

        // FakeRandom picks the same fraction for both axes, so the spot is (x, 0.5, x) for x in -7..7.
        private void StartWithItemAt(Vector3 position) {
            _random.FloatFraction = (position.x + 7f) / 14f;
            _field.Start();
            Assert.Less(Vector3.Distance(position, _items.GetItems()[0].Position), 0.001f);
        }
    }
}
