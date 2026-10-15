using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Code.FlowSystem {

    [UsedImplicitly]
    public class FlowManager : IFlowRunner, IFlowRegistry {

        // Keyed by data type. Each typed instantiator is captured in its lambda; only the dictionary forgets the types,
        // since for example IFlowInstantiator<LobbyFlowResult, LobbyFlowData> can't be stored as IFlowInstantiator<UniTaskResult, ...>.
        private readonly Dictionary<Type, Func<object, CancellationToken, UniTask<UniTaskResult>>> _starters = new();

        public void Register<TResult, TData>(IFlowInstantiator<TResult, TData> instantiator) where TResult : UniTaskResult where TData : IFlowData<TResult> {
            
            if (_starters.ContainsKey(typeof(TData))) {
                throw new InvalidOperationException($"A flow is already registered for {typeof(TData).Name}");
            }
            _starters[typeof(TData)] = async (data, cancellation) => await instantiator.StartFlow((TData)data, cancellation);
        }

        public async UniTask<TResult> Run<TResult>(IFlowData<TResult> data, CancellationToken cancellation) where TResult : UniTaskResult {
            
            if (!_starters.TryGetValue(data.GetType(), out var start)) {
                throw new InvalidOperationException($"No flow is registered for {data.GetType().Name}");
            }
            return (TResult)await start(data, cancellation);
        }
    }
}