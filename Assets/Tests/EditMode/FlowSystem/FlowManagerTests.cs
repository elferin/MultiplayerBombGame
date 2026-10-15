using System;
using System.Collections;
using System.Threading;
using Code.FlowSystem;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Code.Tests {

    public class FlowManagerTests {

        private const int TIMEOUT_MS = 5000;

        private class FirstResult : UniTaskResult {
        }

        private class FirstData : IFlowData<FirstResult> {
        }

        private class SecondResult : UniTaskResult {
        }

        private class SecondData : IFlowData<SecondResult> {
        }

        private FlowManager _flowManager;
        private FakeFlowInstantiator<FirstResult, FirstData> _firstFlow;
        private FakeFlowInstantiator<SecondResult, SecondData> _secondFlow;
        private CancellationTokenSource _cancellation;

        [SetUp]
        public void SetUp() {
            _flowManager = new FlowManager();
            _firstFlow = new FakeFlowInstantiator<FirstResult, FirstData>();
            _secondFlow = new FakeFlowInstantiator<SecondResult, SecondData>();
            _cancellation = new CancellationTokenSource(TIMEOUT_MS);
        }

        [TearDown]
        public void TearDown() {
            _cancellation.Cancel();
            _cancellation.Dispose();
        }

        [UnityTest]
        public IEnumerator RunStartsTheRegisteredFlowAndReturnsItsResult() => UniTask.ToCoroutine(async () => {
            _flowManager.Register(_firstFlow);
            var data = new FirstData();

            var run = _flowManager.Run(data, _cancellation.Token).Preserve();

            Assert.AreEqual(1, _firstFlow.StartCount);
            Assert.AreSame(data, _firstFlow.LastData);
            Assert.AreEqual(_cancellation.Token, _firstFlow.LastCancellation);
            Assert.AreEqual(UniTaskStatus.Pending, run.Status);

            var result = new FirstResult();
            _firstFlow.Complete(result);

            Assert.AreSame(result, await run);
        });

        [UnityTest]
        public IEnumerator RunPicksTheFlowByDataType() => UniTask.ToCoroutine(async () => {
            _flowManager.Register(_firstFlow);
            _flowManager.Register(_secondFlow);

            var run = _flowManager.Run(new SecondData(), _cancellation.Token).Preserve();
            var result = new SecondResult();
            _secondFlow.Complete(result);

            Assert.AreSame(result, await run);
            Assert.AreEqual(0, _firstFlow.StartCount);
            Assert.AreEqual(1, _secondFlow.StartCount);
        });

        [Test]
        public void RegisteringTheSameDataTypeTwiceThrows() {
            _flowManager.Register(_firstFlow);

            Assert.Throws<InvalidOperationException>(() =>
                _flowManager.Register(new FakeFlowInstantiator<FirstResult, FirstData>()));
        }

        [UnityTest]
        public IEnumerator RunWithoutARegisteredFlowThrows() => UniTask.ToCoroutine(async () => {
            _flowManager.Register(_firstFlow);

            var hasThrown = false;
            try {
                await _flowManager.Run(new SecondData(), _cancellation.Token);
            }
            catch (InvalidOperationException) {
                hasThrown = true;
            }

            Assert.IsTrue(hasThrown);
        });

        [UnityTest]
        public IEnumerator CancellingEndsTheRunningFlow() => UniTask.ToCoroutine(async () => {
            _flowManager.Register(_firstFlow);
            var run = _flowManager.Run(new FirstData(), _cancellation.Token);

            _cancellation.Cancel();
            var wasCancelled = false;
            try {
                await run;
            }
            catch (OperationCanceledException) {
                wasCancelled = true;
            }

            Assert.IsTrue(wasCancelled);
        });
    }
}
