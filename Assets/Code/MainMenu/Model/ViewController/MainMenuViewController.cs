using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Code.MainMenu {

    public class MainMenuViewController : IMainMenuViewController {

        public event Action QuitClicked = delegate { };
        public event Action HostClicked = delegate { };
        public event Action JoinClicked = delegate { };
        public event Action<string> PlayerNameChanged = delegate { };
        public event Action<string> PlayerCountChanged = delegate { };
        public event Action<string> LobbyCodeChanged = delegate { };
        public event Action CreateLobbyClicked = delegate { };
        public event Action JoinLobbyClicked = delegate { };
        public event Action BackClicked = delegate { };
        public event Action PasteCodeClicked = delegate { };

        private readonly MainMenuAssetLoader _assetLoader;
        private readonly IViewDataGatherer<MainMenuViewData> _dataGatherer;
        private readonly UniTaskCompletionSource _closed;

        private MainMenuView _view;

        public MainMenuViewController(MainMenuAssetLoader assetLoader, IViewDataGatherer<MainMenuViewData> dataGatherer) {
            _assetLoader = assetLoader;
            _dataGatherer = dataGatherer;
            _closed = new UniTaskCompletionSource();
        }

        public void Refresh() {
            if (_view == null) {
                return;
            }
            _view.Show(_dataGatherer.Get());
        }

        public async UniTask RunAsync(CancellationToken cancellation) {
            try {
                await Initialize(cancellation);
                await _closed.Task.AttachExternalCancellation(cancellation);
            }
            finally {
                // The view can already be destroyed when Play mode or the scene ends.
                if (_view != null) {
                    RemoveListeners();
                    _assetLoader.DestroyMainMenu(_view);
                }
            }
        }

        public void Close() {
            _closed.TrySetResult();
        }

        private async UniTask Initialize(CancellationToken cancellation) {
            _view = await _assetLoader.CreateMainMenu(cancellation);

            AddListeners();
            Refresh();
        }

        private void AddListeners() {
            _view.QuitClicked += OnQuitClicked;
            _view.HostClicked += OnHostClicked;
            _view.JoinClicked += OnJoinClicked;
            _view.PlayerNameChanged += OnPlayerNameChanged;
            _view.PlayerCountChanged += OnPlayerCountChanged;
            _view.LobbyCodeChanged += OnLobbyCodeChanged;
            _view.CreateLobbyClicked += OnCreateLobbyClicked;
            _view.JoinLobbyClicked += OnJoinLobbyClicked;
            _view.BackClicked += OnBackClicked;
            _view.PasteCodeClicked += OnPasteCodeClicked;
        }

        private void RemoveListeners() {
            _view.QuitClicked -= OnQuitClicked;
            _view.HostClicked -= OnHostClicked;
            _view.JoinClicked -= OnJoinClicked;
            _view.PlayerNameChanged -= OnPlayerNameChanged;
            _view.PlayerCountChanged -= OnPlayerCountChanged;
            _view.LobbyCodeChanged -= OnLobbyCodeChanged;
            _view.CreateLobbyClicked -= OnCreateLobbyClicked;
            _view.JoinLobbyClicked -= OnJoinLobbyClicked;
            _view.BackClicked -= OnBackClicked;
            _view.PasteCodeClicked -= OnPasteCodeClicked;
        }

        private void OnQuitClicked() {
            QuitClicked.Invoke();
        }

        private void OnHostClicked() {
            HostClicked.Invoke();
        }

        private void OnJoinClicked() {
            JoinClicked.Invoke();
        }

        private void OnPlayerNameChanged(string text) {
            PlayerNameChanged.Invoke(text);
        }

        private void OnPlayerCountChanged(string text) {
            PlayerCountChanged.Invoke(text);
        }

        private void OnLobbyCodeChanged(string text) {
            LobbyCodeChanged.Invoke(text);
        }

        private void OnCreateLobbyClicked() {
            CreateLobbyClicked.Invoke();
        }

        private void OnJoinLobbyClicked() {
            JoinLobbyClicked.Invoke();
        }

        private void OnBackClicked() {
            BackClicked.Invoke();
        }

        private void OnPasteCodeClicked() {
            PasteCodeClicked.Invoke();
        }
    }
}
