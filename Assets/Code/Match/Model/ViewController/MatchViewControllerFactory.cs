namespace Code.Match {
    public class MatchViewControllerFactory : IMatchViewControllerFactory {
        private readonly MatchAssetLoader _assetLoader;
        private readonly IViewDataGatherer<MatchViewData> _dataGatherer;

        public MatchViewControllerFactory(MatchAssetLoader assetLoader, IViewDataGatherer<MatchViewData> dataGatherer) {
            _assetLoader = assetLoader;
            _dataGatherer = dataGatherer;
        }

        public IMatchViewController CreateViewController() {
            return new MatchViewController(_assetLoader, _dataGatherer);
        }
    }
}
