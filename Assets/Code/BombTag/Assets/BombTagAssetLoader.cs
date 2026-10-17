using System.Threading;
using Code.Util;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Code.BombTag {

    [UsedImplicitly]
    public class BombTagAssetLoader {

        private const string BOMB_TAG_VIEW_ADDRESS = "BombTagView";

        private readonly IAddressableAssets _assets;

        public BombTagAssetLoader(IAddressableAssets assets) {
            _assets = assets;
        }

        public UniTask<BombTagView> CreateBombTagView(CancellationToken cancellation) {
            return _assets.Instantiate<BombTagView>(BOMB_TAG_VIEW_ADDRESS, cancellation);
        }

        public void DestroyBombTagView(BombTagView view) {
            _assets.Release(view);
        }
    }
}
