using System;
using System.Collections;
using System.Threading;
using Code.Lobby;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Code.Tests.Lobby {

    public class LobbyCreatorTests {

        private const int TIMEOUT_MS = 5000;

        private FakeLobbyService _lobbyService;
        private LobbyStatusHolder _status;
        private CurrentLobbyHolder _currentLobby;
        private LobbyCreator _creator;
        private CancellationTokenSource _cancellation;

        [SetUp]
        public void SetUp() {
            _lobbyService = new FakeLobbyService();
            _status = new LobbyStatusHolder();
            _currentLobby = new CurrentLobbyHolder();
            _creator = new LobbyCreator(_lobbyService, _status, _currentLobby);
            _cancellation = new CancellationTokenSource(TIMEOUT_MS);
        }

        [TearDown]
        public void TearDown() {
            _cancellation.Cancel();
            _cancellation.Dispose();
        }

        [Test]
        public void IsCreatingWithoutALobbyAsSoonAsCalled() {
            _status.SetStatus(LobbyStatus.CreateFailed);
            _currentLobby.SetCurrentLobby(FakeLobbyService.CreateLobbyState("OLD123", 4, "Luka"));

            _creator.TryCreateLobby(6, "Luka", _cancellation.Token).Forget();

            Assert.AreEqual(LobbyStatus.Creating, _status.GetStatus());
            Assert.AreSame(LobbyState.None, _currentLobby.GetCurrentLobby());
            Assert.IsTrue(_lobbyService.IsCreatePending);
        }

        [Test]
        public void SendsPlayerCountAndName() {
            _creator.TryCreateLobby(6, "Luka", _cancellation.Token).Forget();

            Assert.AreEqual(6, _lobbyService.LastCreateMaxPlayers);
            Assert.AreEqual("Luka", _lobbyService.LastCreatePlayerName);
        }

        [UnityTest]
        public IEnumerator SuccessfulCreateStoresLobbyAndIsReady() => UniTask.ToCoroutine(async () => {
            var create = _creator.TryCreateLobby(6, "Luka", _cancellation.Token);

            _lobbyService.SucceedCreate();

            Assert.IsTrue(await create);
            Assert.AreEqual(LobbyStatus.Ready, _status.GetStatus());
            Assert.AreEqual("NEW123", _currentLobby.GetCurrentLobby().Code);
        });

        [UnityTest]
        public IEnumerator FailedCreateReturnsFalseAndFails() => UniTask.ToCoroutine(async () => {
            var create = _creator.TryCreateLobby(6, "Luka", _cancellation.Token);

            _lobbyService.FailCreate();

            Assert.IsFalse(await create);
            Assert.AreEqual(LobbyStatus.CreateFailed, _status.GetStatus());
            Assert.IsFalse(_currentLobby.GetCurrentLobby().IsValid);
        });

        [UnityTest]
        public IEnumerator CancellingThrows() => UniTask.ToCoroutine(async () => {
            var create = _creator.TryCreateLobby(6, "Luka", _cancellation.Token);

            _cancellation.Cancel();
            var wasCancelled = false;
            try {
                await create;
            }
            catch (OperationCanceledException) {
                wasCancelled = true;
            }

            Assert.IsTrue(wasCancelled);
        });
    }
}
