using Code.BombTag;
using NUnit.Framework;
using UnityEngine;

namespace Code.Tests.BombTag {

    public class AlivePlayersTests {

        private FakeBombTagPlayers _players;
        private AlivePlayers _alivePlayers;

        [SetUp]
        public void SetUp() {
            _players = new FakeBombTagPlayers();
            _alivePlayers = new AlivePlayers(_players);

            _players.Add(1, "Ana", Vector3.zero);
            _players.Add(2, "Luka", Vector3.right);
            _alivePlayers.TakeSpawnedPlayers();
        }

        [Test]
        public void RefreshTakesNewPositions() {
            _players.Move(2, new Vector3(4f, 0f, 0f));

            _alivePlayers.Refresh();

            Assert.IsTrue(_alivePlayers.TryGet(2, out var player));
            Assert.AreEqual(new Vector3(4f, 0f, 0f), player.Position);
        }

        [Test]
        public void RefreshDropsPlayersWhoLeft() {
            _players.Remove(1);

            _alivePlayers.Refresh();

            Assert.AreEqual(1, _alivePlayers.GetCount());
            Assert.IsFalse(_alivePlayers.TryGet(1, out _));
        }

        [Test]
        public void RefreshDoesNotAddPlayersWhoSpawnLater() {
            _players.Add(3, "Late", Vector3.zero);

            _alivePlayers.Refresh();

            Assert.AreEqual(2, _alivePlayers.GetCount());
        }

        [Test]
        public void EliminateDespawnsAndRemovesThePlayer() {
            _alivePlayers.Eliminate(1);

            CollectionAssert.AreEqual(new[] { 1 }, _players.Eliminated);
            Assert.AreEqual(1, _alivePlayers.GetCount());
        }

        [Test]
        public void WinnerIsTheLastPlayerLeft() {
            Assert.AreEqual(string.Empty, _alivePlayers.GetWinnerName());

            _alivePlayers.Eliminate(1);
            Assert.AreEqual("Luka", _alivePlayers.GetWinnerName());

            _alivePlayers.Eliminate(2);
            Assert.AreEqual(string.Empty, _alivePlayers.GetWinnerName());
        }
    }
}
