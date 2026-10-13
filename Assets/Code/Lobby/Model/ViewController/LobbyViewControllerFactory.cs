namespace Code.Lobby {
    public class LobbyViewControllerFactory : ILobbyViewControllerFactory {
        private readonly LobbyAssetLoader _assetLoader;
        private readonly IViewDataGatherer<LobbyViewData> _dataGatherer;

        public LobbyViewControllerFactory(LobbyAssetLoader assetLoader, IViewDataGatherer<LobbyViewData> dataGatherer) {
            _assetLoader = assetLoader;
            _dataGatherer = dataGatherer;
        }

        public ILobbyViewController CreateViewController() {
            return new LobbyViewController(_assetLoader, _dataGatherer);
        }
    }
}
