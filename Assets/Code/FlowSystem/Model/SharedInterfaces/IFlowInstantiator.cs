using System.Threading;
using Cysharp.Threading.Tasks;

namespace Code.FlowSystem {
    public interface IFlowInstantiator<TResult, in TData> where TResult : UniTaskResult where TData : IFlowData<TResult> {

        UniTask<TResult> StartFlow(TData data, CancellationToken cancellation);
    }
}