namespace Code.MatchMenu {
    public class MatchMenuViewControllerFactory : IMatchMenuViewControllerFactory {
        private readonly MatchMenuAssetLoader _assetLoader;

        public MatchMenuViewControllerFactory(MatchMenuAssetLoader assetLoader) {
            _assetLoader = assetLoader;
        }

        public IMatchMenuViewController CreateViewController() {
            return new MatchMenuViewController(_assetLoader);
        }
    }
}
