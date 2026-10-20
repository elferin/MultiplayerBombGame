using System.Threading;
using Code.Util;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Code.MatchMenu {

    [UsedImplicitly]
    public class MatchMenuAssetLoader {

        private const string MATCH_MENU_VIEW_ADDRESS = "MatchMenuView";

        private readonly IAddressableAssets _assets;

        public MatchMenuAssetLoader(IAddressableAssets assets) {
            _assets = assets;
        }

        public UniTask<MatchMenuView> CreateMatchMenuView(CancellationToken cancellation) {
            return _assets.Instantiate<MatchMenuView>(MATCH_MENU_VIEW_ADDRESS, cancellation);
        }

        public void DestroyMatchMenuView(MatchMenuView view) {
            _assets.Release(view);
        }
    }
}
