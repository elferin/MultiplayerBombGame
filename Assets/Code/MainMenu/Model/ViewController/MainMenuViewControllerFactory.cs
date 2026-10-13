using JetBrains.Annotations;

namespace Code.MainMenu {
    
    [UsedImplicitly]
    public class MainMenuViewControllerFactory : IMainMenuViewControllerFactory {
        private readonly MainMenuAssetLoader _assetLoader;
        private readonly IViewDataGatherer<MainMenuViewData> _dataGatherer;

        public MainMenuViewControllerFactory(MainMenuAssetLoader assetLoader, IViewDataGatherer<MainMenuViewData> dataGatherer) {
            _assetLoader = assetLoader;
            _dataGatherer = dataGatherer;
        }

        public IMainMenuViewController CreateViewController() {
            return new MainMenuViewController(_assetLoader, _dataGatherer);
        }
    }
}
