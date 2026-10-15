namespace Code.FlowSystem {
    // Data that starts a flow; TResult is what that flow returns, so IFlowRunner.Run knows the result type.
    public interface IFlowData<TResult> where TResult : UniTaskResult {
    }
}