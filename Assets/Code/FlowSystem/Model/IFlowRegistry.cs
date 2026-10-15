namespace Code.FlowSystem {
    // Where each system hands over the instantiator for its flow data.
    public interface IFlowRegistry {

        // Typed per flow: IFlowInstantiator<LobbyFlowResult, LobbyFlowData> can't be passed as IFlowInstantiator<UniTaskResult, ...>.
        void Register<TResult, TData>(IFlowInstantiator<TResult, TData> instantiator)
            where TResult : UniTaskResult where TData : IFlowData<TResult>;
    }
}