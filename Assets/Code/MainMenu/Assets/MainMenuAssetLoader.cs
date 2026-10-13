using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.MainMenu {
    public class MainMenuAssetLoader {

        private const string MAIN_MENU_VIEW_PATH = "MainMenuView";

        public async UniTask<MainMenuView> CreateMainMenu(CancellationToken cancellation) {
            var prefab = (MainMenuView)await Resources.LoadAsync<MainMenuView>(MAIN_MENU_VIEW_PATH).WithCancellation(cancellation);
            var view = Object.Instantiate(prefab);

            return view;
        }
    }
}
