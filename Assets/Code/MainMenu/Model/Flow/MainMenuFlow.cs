using System.Threading;
using Code.Lobby;
using Code.Util;
using Cysharp.Threading.Tasks;

namespace Code.MainMenu {

    public class MainMenuFlow : IMainMenuFlow {

        private enum MainMenuAction {
            Quit,
            Host,
            Join,
            Back,
            CreateLobby,
            JoinLobby
        }

        private readonly IMainMenuViewControllerFactory _viewControllerFactory;
        private readonly LobbyJoiner _lobbyJoiner;
        private readonly IMainMenuScreenLocation _screenLocation;
        private readonly IPlayerName _playerName;
        private readonly IHostLobbySettings _hostLobbySettings;
        private readonly IJoinLobby _joinLobby;
        private readonly IMainMenuLobbyRules _lobbyRules;
        private readonly IClipboard _clipboard;

        private IMainMenuViewController _viewController;
        private UniTaskCompletionSource<MainMenuAction> _action;

        public MainMenuFlow(IMainMenuViewControllerFactory viewControllerFactory, LobbyJoiner lobbyJoiner,
            IMainMenuScreenLocation screenLocation, IPlayerName playerName,
            IHostLobbySettings hostLobbySettings, IJoinLobby joinLobby, IMainMenuLobbyRules lobbyRules, IClipboard clipboard) {
            _viewControllerFactory = viewControllerFactory;
            _lobbyJoiner = lobbyJoiner;
            _screenLocation = screenLocation;
            _playerName = playerName;
            _hostLobbySettings = hostLobbySettings;
            _joinLobby = joinLobby;
            _lobbyRules = lobbyRules;
            _clipboard = clipboard;
        }

        public async UniTask<MainMenuFlowResult> RunAsync(CancellationToken cancellation) {

            await UniTask.Yield(cancellation);
            var flowResult = new MainMenuFlowResult();
            ResetState();
            _viewController = _viewControllerFactory.CreateViewController();

            AddListeners();
            try {
                var viewRun = _viewController.RunAsync(cancellation);

                while (true) {
                    _action = new UniTaskCompletionSource<MainMenuAction>();
                    var action = await _action.Task.AttachExternalCancellation(cancellation);

                    if (action == MainMenuAction.Quit) {
                        flowResult.ShouldQuit = true;
                        break;
                    }
                    if (action == MainMenuAction.CreateLobby) {
                        _playerName.SavePlayerName();
                        flowResult.ShouldHost = true;
                        flowResult.MaxPlayers = _hostLobbySettings.GetPlayerCount();
                        flowResult.PlayerName = _playerName.GetTrimmedPlayerName();
                        break;
                    }
                    if (action == MainMenuAction.JoinLobby) {
                        if (await TryJoinLobby(cancellation)) {
                            _playerName.SavePlayerName();
                            flowResult.HasJoinedLobby = true;
                            flowResult.PlayerName = _playerName.GetTrimmedPlayerName();
                            break;
                        }
                        continue;
                    }

                    ShowScreen(action);
                }

                _viewController.Close();
                await viewRun;
            }
            finally {
                RemoveListeners();
                _viewController.Close();
            }

            return flowResult;
        }

        // The menu always opens on its main buttons. Host settings and the join code keep their last values.
        private void ResetState() {
            _screenLocation.ResetLocation();
            _playerName.LoadPlayerName();
        }

        private void ShowScreen(MainMenuAction action) {
            switch (action) {
                case MainMenuAction.Host:
                    _screenLocation.SetLocation(MainMenuScreen.HostLobby);
                    break;
                case MainMenuAction.Join:
                    _screenLocation.SetLocation(MainMenuScreen.JoinLobby);
                    _joinLobby.SetJoinStatus(JoinLobbyStatus.Idle);
                    break;
                case MainMenuAction.Back:
                    _playerName.SavePlayerName();
                    _screenLocation.ResetLocation();
                    break;
            }
            _viewController.Refresh();
        }

        private async UniTask<bool> TryJoinLobby(CancellationToken cancellation) {
            // The joiner sets the Joining status before its first await, so this refresh shows it.
            var join = _lobbyJoiner.TryJoinLobby(_playerName.GetTrimmedPlayerName(), cancellation);
            _viewController.Refresh();

            var hasJoined = await join;
            if (!hasJoined) {
                // Stay on the Join panel so the player can fix the code.
                _viewController.Refresh();
            }
            return hasJoined;
        }

        private void AddListeners() {
            _viewController.QuitClicked += OnQuitClicked;
            _viewController.HostClicked += OnHostClicked;
            _viewController.JoinClicked += OnJoinClicked;
            _viewController.PlayerNameChanged += OnPlayerNameChanged;
            _viewController.PlayerCountChanged += OnPlayerCountChanged;
            _viewController.LobbyCodeChanged += OnLobbyCodeChanged;
            _viewController.CreateLobbyClicked += OnCreateLobbyClicked;
            _viewController.JoinLobbyClicked += OnJoinLobbyClicked;
            _viewController.BackClicked += OnBackClicked;
            _viewController.PasteCodeClicked += OnPasteCodeClicked;
        }

        private void RemoveListeners() {
            _viewController.QuitClicked -= OnQuitClicked;
            _viewController.HostClicked -= OnHostClicked;
            _viewController.JoinClicked -= OnJoinClicked;
            _viewController.PlayerNameChanged -= OnPlayerNameChanged;
            _viewController.PlayerCountChanged -= OnPlayerCountChanged;
            _viewController.LobbyCodeChanged -= OnLobbyCodeChanged;
            _viewController.CreateLobbyClicked -= OnCreateLobbyClicked;
            _viewController.JoinLobbyClicked -= OnJoinLobbyClicked;
            _viewController.BackClicked -= OnBackClicked;
            _viewController.PasteCodeClicked -= OnPasteCodeClicked;
        }

        private void OnQuitClicked() {
            _action.TrySetResult(MainMenuAction.Quit);
        }

        private void OnHostClicked() {
            _action.TrySetResult(MainMenuAction.Host);
        }

        private void OnJoinClicked() {
            _action.TrySetResult(MainMenuAction.Join);
        }

        private void OnBackClicked() {
            if (!_lobbyRules.CanGoBack()) {
                return;
            }
            _action.TrySetResult(MainMenuAction.Back);
        }

        private void OnCreateLobbyClicked() {
            if (!_lobbyRules.CanCreateLobby()) {
                return;
            }
            _action.TrySetResult(MainMenuAction.CreateLobby);
        }

        private void OnJoinLobbyClicked() {
            if (!_lobbyRules.CanJoinLobby()) {
                return;
            }
            _action.TrySetResult(MainMenuAction.JoinLobby);
        }

        private void OnPlayerNameChanged(string text) {
            _playerName.SetPlayerName(text);
            _viewController.Refresh();
        }

        private void OnPlayerCountChanged(string text) {
            _hostLobbySettings.SetPlayerCount(text);
            _viewController.Refresh();
        }

        private void OnPasteCodeClicked() {
            if (!_lobbyRules.CanPasteLobbyCode()) {
                return;
            }
            // The same as typing it: lobby codes are upper case, and copied text often has stray spaces.
            OnLobbyCodeChanged(_clipboard.GetText().Trim().ToUpperInvariant());
        }

        private void OnLobbyCodeChanged(string text) {
            _joinLobby.SetLobbyCode(text);
            // A new code makes the last failure's message out of date.
            if (IsJoinFailure(_joinLobby.GetJoinStatus())) {
                _joinLobby.SetJoinStatus(JoinLobbyStatus.Idle);
            }
            _viewController.Refresh();
        }

        private bool IsJoinFailure(JoinLobbyStatus status) {
            return status == JoinLobbyStatus.Failed
                   || status == JoinLobbyStatus.MatchInProgress
                   || status == JoinLobbyStatus.LobbyFull;
        }
    }
}
