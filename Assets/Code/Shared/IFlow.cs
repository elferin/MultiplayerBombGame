using System.Threading;
using Cysharp.Threading.Tasks;

namespace Code {
    public interface IFlow<T> : IAsyncRunnable where T : UniTaskResult {

        new UniTask<T> RunAsync(CancellationToken cancellation);

        // Lets a flow run as a plain IAsyncRunnable; the result is discarded.
        UniTask IAsyncRunnable.RunAsync(CancellationToken cancellation) => RunAsync(cancellation).AsUniTask();
    }
}
