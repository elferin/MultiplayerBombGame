namespace Code.MatchMenu {

    public class MatchMenuFlowFactory : IMatchMenuFlowFactory {

        private readonly IMatchMenuViewControllerFactory _viewControllerFactory;

        public MatchMenuFlowFactory(IMatchMenuViewControllerFactory viewControllerFactory) {
            _viewControllerFactory = viewControllerFactory;
        }

        public IMatchMenuFlow Create(MatchMenuFlowData data) {
            return new MatchMenuFlow(_viewControllerFactory);
        }
    }
}
