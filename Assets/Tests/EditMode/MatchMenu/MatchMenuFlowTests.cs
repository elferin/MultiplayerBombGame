using System.Collections;
using System.Threading;
using Code.MatchMenu;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Code.Tests.MatchMenu {

    public class MatchMenuFlowTests {

        private const int TIMEOUT_MS = 5000;

        private FakeMatchMenuViewController _view;
        private CancellationTokenSource _cancellation;

        [SetUp]
        public void SetUp() {
            _view = new FakeMatchMenuViewController();
            _cancellation = new CancellationTokenSource(TIMEOUT_MS);
        }

        [TearDown]
        public void TearDown() {
            _cancellation.Cancel();
            _cancellation.Dispose();
        }

        [UnityTest]
        public IEnumerator ContinueReturnsContinueAndClosesTheView() => UniTask.ToCoroutine(async () => {
            var run = StartFlow(_cancellation.Token);
            Assert.IsTrue(_view.IsRunning);

            _view.ClickContinue();
            var result = await run;

            Assert.AreEqual(MatchMenuChoice.Continue, result.Choice);
            Assert.IsTrue(_view.IsClosed);
            Assert.IsFalse(_view.IsRunning);
        });

        [UnityTest]
        public IEnumerator LeaveReturnsLeaveAndClosesTheView() => UniTask.ToCoroutine(async () => {
            var run = StartFlow(_cancellation.Token);

            _view.ClickLeave();
            var result = await run;

            Assert.AreEqual(MatchMenuChoice.Leave, result.Choice);
            Assert.IsTrue(_view.IsClosed);
        });

        [UnityTest]
        public IEnumerator CancellingClosesTheView() => UniTask.ToCoroutine(async () => {
            using (var menuCancellation = CancellationTokenSource.CreateLinkedTokenSource(_cancellation.Token)) {
                var run = StartFlow(menuCancellation.Token);

                menuCancellation.Cancel();
                var (isCanceled, _) = await run.SuppressCancellationThrow();

                Assert.IsTrue(isCanceled);
                Assert.IsTrue(_view.IsClosed);
                Assert.IsFalse(_view.IsRunning);
            }
        });

        [UnityTest]
        public IEnumerator ClicksAfterTheFirstChoiceAreIgnored() => UniTask.ToCoroutine(async () => {
            var run = StartFlow(_cancellation.Token);

            _view.ClickLeave();
            _view.ClickContinue();
            var result = await run;

            Assert.AreEqual(MatchMenuChoice.Leave, result.Choice);
        });

        private UniTask<MatchMenuFlowResult> StartFlow(CancellationToken cancellation) {
            return new MatchMenuFlow(_view).RunAsync(cancellation).Preserve();
        }
    }
}
