namespace Code.BombTag {
    public class BombTagViewControllerFactory : IBombTagViewControllerFactory {
        private readonly BombTagAssetLoader _assetLoader;
        private readonly IViewDataGatherer<BombTagViewData> _dataGatherer;

        public BombTagViewControllerFactory(BombTagAssetLoader assetLoader, IViewDataGatherer<BombTagViewData> dataGatherer) {
            _assetLoader = assetLoader;
            _dataGatherer = dataGatherer;
        }

        public IBombTagViewController CreateViewController() {
            return new BombTagViewController(_assetLoader, _dataGatherer);
        }
    }
}
