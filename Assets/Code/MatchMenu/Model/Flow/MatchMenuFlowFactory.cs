namespace Code.MatchMenu {

    public class MatchMenuFlowFactory : IMatchMenuFlowFactory {

        private readonly IMatchMenuViewControllerFactory _viewControllerFactory;
        private readonly IMatchMenuStateSetter _state;

        public MatchMenuFlowFactory(IMatchMenuViewControllerFactory viewControllerFactory, IMatchMenuStateSetter state) {
            _viewControllerFactory = viewControllerFactory;
            _state = state;
        }

        public IMatchMenuFlow Create(MatchMenuFlowData data) {
            return new MatchMenuFlow(_viewControllerFactory, _state);
        }
    }
}
