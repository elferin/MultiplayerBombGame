using System.Threading;
using Cysharp.Threading.Tasks;

namespace Code.FlowSystem {
    
    public interface IFlowRunner {
        
        UniTask<TResult> Run<TResult>(IFlowData<TResult> data, CancellationToken cancellation) where TResult : UniTaskResult;
    }
}