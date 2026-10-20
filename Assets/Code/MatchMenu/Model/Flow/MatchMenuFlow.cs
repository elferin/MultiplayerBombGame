using System.Threading;
using Cysharp.Threading.Tasks;

namespace Code.MatchMenu {

    // Shows the menu until the player picks Continue or Leave. The match closes it by cancelling, e.g. on a second Esc.
    public class MatchMenuFlow : IMatchMenuFlow {

        private readonly IMatchMenuViewControllerFactory _viewControllerFactory;

        private IMatchMenuViewController _viewController;
        private UniTaskCompletionSource<MatchMenuChoice> _choice;

        public MatchMenuFlow(IMatchMenuViewControllerFactory viewControllerFactory) {
            _viewControllerFactory = viewControllerFactory;
        }

        public async UniTask<MatchMenuFlowResult> RunAsync(CancellationToken cancellation) {
            var flowResult = new MatchMenuFlowResult();
            _viewController = _viewControllerFactory.CreateViewController();
            _choice = new UniTaskCompletionSource<MatchMenuChoice>();

            AddListeners();
            try {
                var viewRun = _viewController.RunAsync(cancellation);

                flowResult.Choice = await _choice.Task.AttachExternalCancellation(cancellation);

                _viewController.Close();
                await viewRun;
            }
            finally {
                RemoveListeners();
                _viewController.Close();
            }

            return flowResult;
        }

        private void AddListeners() {
            _viewController.ContinueClicked += OnContinueClicked;
            _viewController.LeaveClicked += OnLeaveClicked;
        }

        private void RemoveListeners() {
            _viewController.ContinueClicked -= OnContinueClicked;
            _viewController.LeaveClicked -= OnLeaveClicked;
        }

        private void OnContinueClicked() {
            _choice.TrySetResult(MatchMenuChoice.Continue);
        }

        private void OnLeaveClicked() {
            _choice.TrySetResult(MatchMenuChoice.Leave);
        }
    }
}
