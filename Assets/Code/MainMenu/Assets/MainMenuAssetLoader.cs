using System.Threading;
using Code.Util;
using Cysharp.Threading.Tasks;

namespace Code.MainMenu {
    public class MainMenuAssetLoader {

        private const string MAIN_MENU_VIEW_ADDRESS = "MainMenuView";

        private readonly IAddressableAssets _assets;

        public MainMenuAssetLoader(IAddressableAssets assets) {
            _assets = assets;
        }

        public UniTask<MainMenuView> CreateMainMenu(CancellationToken cancellation) {
            return _assets.Instantiate<MainMenuView>(MAIN_MENU_VIEW_ADDRESS, cancellation);
        }

        public void DestroyMainMenu(MainMenuView view) {
            _assets.Release(view);
        }
    }
}
