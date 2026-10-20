using System.Collections;
using System.Threading;
using Code.BombTag;
using Code.FlowSystem;
using Code.GameFlow;
using Code.Lobby;
using Code.Profile;
using Code.Tests.FlowSystem;
using Code.Tests.Lobby;
using Code.Tests.Profile;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Code.Tests.GameFlow {

    public class LobbyToMatchTests {

        private const int TIMEOUT_MS = 5000;

        private FakeLobbyService _lobbyService;
        private UsedJoinCodeHolder _usedJoinCode;
        private FakeFlowInstantiator<BombTagFlowResult, BombTagFlowData> _bombTag;
        private LobbyToMatch _transition;
        private CancellationTokenSource _cancellation;

        [SetUp]
        public void SetUp() {
            _lobbyService = new FakeLobbyService();
            _usedJoinCode = new UsedJoinCodeHolder();
            _bombTag = new FakeFlowInstantiator<BombTagFlowResult, BombTagFlowData>();
            var flowManager = new FlowManager();
            flowManager.Register(_bombTag);
            var playerName = new PlayerNameHolder(new FakePlayerNameStorage());
            playerName.SetPlayerName("  Ana ");
            _transition = new LobbyToMatch(new MatchJoinCodeSharer(_lobbyService), new MatchJoinCodeReceiver(_lobbyService),
                _usedJoinCode, playerName, new HostLobbySettings(), flowManager);
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
            Assert.AreEqual(2, data.MinPlayers);
            Assert.AreEqual("Ana", data.PlayerName);

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

        [UnityTest]
        public IEnumerator HostPlaysBombTagWithTheConnectedPlayers() => UniTask.ToCoroutine(async () => {
            var data = _transition.GetMatchData(new LobbyFlowResult { ShouldStartMatch = true, IsHost = true, MaxPlayers = 6, PlayerCount = 3 });

            var game = data.PlayGame(3, _cancellation.Token).Preserve();

            Assert.AreEqual(1, _bombTag.StartCount);
            Assert.IsTrue(_bombTag.LastData.IsHost);
            Assert.AreEqual(3, _bombTag.LastData.PlayerCount);
            Assert.AreEqual(UniTaskStatus.Pending, game.Status);

            _bombTag.Complete(new BombTagFlowResult());
            await game;
        });

        [UnityTest]
        public IEnumerator ClientPlaysBombTagAsClient() => UniTask.ToCoroutine(async () => {
            var data = _transition.GetMatchData(new LobbyFlowResult { ShouldStartMatch = true, IsHost = false });

            var game = data.PlayGame(0, _cancellation.Token).Preserve();

            Assert.AreEqual("Ana", data.PlayerName);
            Assert.IsFalse(_bombTag.LastData.IsHost);

            _bombTag.Complete(new BombTagFlowResult());
            await game;
        });

        private LobbyState CreateStartingLobby(string matchJoinCode) {
            return new LobbyState(true, "ABC123", 4, FakeLobbyService.CreatePlayers("Ana", "Luka"), false, matchJoinCode, true);
        }
    }
}
