using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Code.Lobby {

    public class LobbyViewController : ILobbyViewController {

        public event Action LeaveClicked = delegate { };
        public event Action StartClicked = delegate { };
        public event Action CopyCodeClicked = delegate { };

        private readonly LobbyAssetLoader _assetLoader;
        private readonly IViewDataGatherer<LobbyViewData> _dataGatherer;
        private readonly UniTaskCompletionSource _closed;

        private LobbyView _view;

        public LobbyViewController(LobbyAssetLoader assetLoader, IViewDataGatherer<LobbyViewData> dataGatherer) {
            _assetLoader = assetLoader;
            _dataGatherer = dataGatherer;
            _closed = new UniTaskCompletionSource();
        }

        public void Refresh() {
            // The view doesn't exist before Initialize finishes, and can already be destroyed when Play mode or the scene ends.
            if (_view == null) {
                return;
            }
            _view.Show(_dataGatherer.Get());
        }

        public async UniTask RunAsync(CancellationToken cancellation) {
            try {
                await Initialize(cancellation);
                await _closed.Task.AttachExternalCancellation(cancellation);
            }
            finally {
                if (_view != null) {
                    RemoveListeners();
                    _assetLoader.DestroyLobbyView(_view);
                }
            }
        }

        public void Close() {
            _closed.TrySetResult();
        }

        private async UniTask Initialize(CancellationToken cancellation) {
            _view = await _assetLoader.CreateLobbyView(cancellation);

            AddListeners();
            Refresh();
        }

        private void AddListeners() {
            _view.LeaveClicked += OnLeaveClicked;
            _view.StartClicked += OnStartClicked;
            _view.CopyCodeClicked += OnCopyCodeClicked;
        }

        private void RemoveListeners() {
            _view.LeaveClicked -= OnLeaveClicked;
            _view.StartClicked -= OnStartClicked;
            _view.CopyCodeClicked -= OnCopyCodeClicked;
        }

        private void OnLeaveClicked() {
            LeaveClicked.Invoke();
        }

        private void OnStartClicked() {
            StartClicked.Invoke();
        }

        private void OnCopyCodeClicked() {
            CopyCodeClicked.Invoke();
        }
    }
}
