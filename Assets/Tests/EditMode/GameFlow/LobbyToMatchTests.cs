using System.Collections;
using System.Threading;
using Code.GameFlow;
using Code.Lobby;
using Code.Tests.Lobby;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Code.Tests.GameFlow {

    public class LobbyToMatchTests {

        private const int TIMEOUT_MS = 5000;

        private FakeLobbyService _lobbyService;
        private UsedJoinCodeHolder _usedJoinCode;
        private LobbyToMatch _transition;
        private CancellationTokenSource _cancellation;

        [SetUp]
        public void SetUp() {
            _lobbyService = new FakeLobbyService();
            _usedJoinCode = new UsedJoinCodeHolder();
            _transition = new LobbyToMatch(new MatchJoinCodeSharer(_lobbyService), new MatchJoinCodeReceiver(_lobbyService),
                _usedJoinCode);
            _cancellation = new CancellationTokenSource(TIMEOUT_MS);
        }

        [TearDown]
        public void TearDown() {
            _cancellation.Cancel();
            _cancellation.Dispose();
        }

        [UnityTest]
        public IEnumerator HostSharesItsJoinCodeThroughTheLobby() => UniTask.ToCoroutine(async () => {
            var data = _transition.GetMatchData(new LobbyFlowResult { ShouldStartMatch = true, IsHost = true, MaxPlayers = 6, PlayerCount = 3 });

            Assert.IsTrue(data.IsHost);
            Assert.AreEqual(6, data.MaxPlayers);
            Assert.AreEqual(3, data.PlayerCount);

            await data.ShareJoinCode("RELAY1");

            Assert.AreEqual("RELAY1", _lobbyService.MatchJoinCode);
            Assert.IsTrue(_lobbyService.IsLocked);
        });

        [UnityTest]
        public IEnumerator ClientWaitsForACodeOtherThanTheLastMatches() => UniTask.ToCoroutine(async () => {
            _usedJoinCode.SetUsedJoinCode("OLD");
            _lobbyService.CurrentLobby = CreateStartingLobby("OLD");

            var data = _transition.GetMatchData(new LobbyFlowResult { ShouldStartMatch = true, IsHost = false });
            var wait = data.GetJoinCode(_cancellation.Token).Preserve();

            Assert.IsFalse(data.IsHost);
            Assert.AreEqual(UniTaskStatus.Pending, wait.Status);

            _lobbyService.RaiseLobbyUpdated(CreateStartingLobby("NEW"));

            Assert.AreEqual("NEW", await wait);
        });

        private LobbyState CreateStartingLobby(string matchJoinCode) {
            return new LobbyState(true, "ABC123", 4, FakeLobbyService.CreatePlayers("Ana", "Luka"), false, matchJoinCode, true);
        }
    }
}
