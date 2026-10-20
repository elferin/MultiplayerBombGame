using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Code.MatchMenu {

    public class MatchMenuViewController : IMatchMenuViewController {

        public event Action ContinueClicked = delegate { };
        public event Action LeaveClicked = delegate { };

        private readonly MatchMenuAssetLoader _assetLoader;
        private readonly UniTaskCompletionSource _closed;

        private MatchMenuView _view;

        public MatchMenuViewController(MatchMenuAssetLoader assetLoader) {
            _assetLoader = assetLoader;
            _closed = new UniTaskCompletionSource();
        }

        // The menu only shows fixed buttons, so there is no data to show again.
        public void Refresh() {
        }

        public async UniTask RunAsync(CancellationToken cancellation) {
            try {
                _view = await _assetLoader.CreateMatchMenuView(cancellation);
                AddListeners();
                await _closed.Task.AttachExternalCancellation(cancellation);
            }
            finally {
                if (_view != null) {
                    RemoveListeners();
                    _assetLoader.DestroyMatchMenuView(_view);
                }
            }
        }

        public void Close() {
            _closed.TrySetResult();
        }

        private void AddListeners() {
            _view.ContinueClicked += OnContinueClicked;
            _view.LeaveClicked += OnLeaveClicked;
        }

        private void RemoveListeners() {
            _view.ContinueClicked -= OnContinueClicked;
            _view.LeaveClicked -= OnLeaveClicked;
        }

        private void OnContinueClicked() {
            ContinueClicked.Invoke();
        }

        private void OnLeaveClicked() {
            LeaveClicked.Invoke();
        }
    }
}
