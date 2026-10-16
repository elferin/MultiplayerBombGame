using System.Threading;
using Cysharp.Threading.Tasks;

namespace Code.Match {

    public class MatchViewController : IMatchViewController {

        private readonly MatchAssetLoader _assetLoader;
        private readonly IViewDataGatherer<MatchViewData> _dataGatherer;
        private readonly UniTaskCompletionSource _closed;

        private MatchView _view;

        public MatchViewController(MatchAssetLoader assetLoader, IViewDataGatherer<MatchViewData> dataGatherer) {
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
                    UnityEngine.Object.Destroy(_view.gameObject);
                }
            }
        }

        public void Close() {
            _closed.TrySetResult();
        }

        private async UniTask Initialize(CancellationToken cancellation) {
            _view = await _assetLoader.CreateMatchView(cancellation);
            Refresh();
        }
    }
}
