using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.BombTag {

    public class BombTagViewController : IBombTagViewController {

        private readonly BombTagAssetLoader _assetLoader;
        private readonly IViewDataGatherer<BombTagViewData> _dataGatherer;
        private readonly UniTaskCompletionSource _closed;

        private BombTagView _view;

        public BombTagViewController(BombTagAssetLoader assetLoader, IViewDataGatherer<BombTagViewData> dataGatherer) {
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

        public void ShowExplosion(Vector3 position) {
            if (_view == null) {
                return;
            }
            _view.ShowExplosion(position);
        }

        public async UniTask RunAsync(CancellationToken cancellation) {
            try {
                await Initialize(cancellation);
                await _closed.Task.AttachExternalCancellation(cancellation);
            }
            finally {
                if (_view != null) {
                    _assetLoader.DestroyBombTagView(_view);
                }
            }
        }

        public void Close() {
            _closed.TrySetResult();
        }

        private async UniTask Initialize(CancellationToken cancellation) {
            _view = await _assetLoader.CreateBombTagView(cancellation);
            Refresh();
        }
    }
}
