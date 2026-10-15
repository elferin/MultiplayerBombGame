namespace Code.FlowSystem {
    // Where each system hands over the instantiator for its flow data.
    public interface IFlowRegistry {

        void Register<TResult, TData>(IFlowInstantiator<TResult, TData> instantiator) where TResult : UniTaskResult where TData : IFlowData<TResult>;
    }
}