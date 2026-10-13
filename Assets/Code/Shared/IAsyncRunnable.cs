using System.Threading;
using Cysharp.Threading.Tasks;

namespace Code {

    public interface IAsyncRunnable {
        // Cancelling ends RunAsync with an OperationCanceledException, after its cleanup has run.
        UniTask RunAsync(CancellationToken cancellation);
    }
}
