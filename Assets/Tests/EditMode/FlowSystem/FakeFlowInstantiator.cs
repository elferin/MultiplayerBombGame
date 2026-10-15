using System.Threading;
using Code.FlowSystem;
using Cysharp.Threading.Tasks;

namespace Code.Tests {

    // Records how it was started; the flow stays pending until the test completes it.
    public class FakeFlowInstantiator<TResult, TData> : IFlowInstantiator<TResult, TData>
        where TResult : UniTaskResult where TData : IFlowData<TResult> {

        private UniTaskCompletionSource<TResult> _pending;

        public int StartCount { get; private set; }
        public TData LastData { get; private set; }
        public CancellationToken LastCancellation { get; private set; }

        public UniTask<TResult> StartFlow(TData data, CancellationToken cancellation) {
            StartCount++;
            LastData = data;
            LastCancellation = cancellation;
            _pending = new UniTaskCompletionSource<TResult>();
            return _pending.Task.AttachExternalCancellation(cancellation);
        }

        public void Complete(TResult result) {
            _pending.TrySetResult(result);
        }
    }
}
