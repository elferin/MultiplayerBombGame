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

        private readonly MainMenuFlowData _data;
        private readonly IMainMenuViewControllerFactory _viewControllerFactory;
        private readonly IMainMenuScreenLocation _screenLocation;
        private readonly IPlayerName _playerName;
        private readonly IHostLobbySettings _hostLobbySettings;
        private readonly IJoinLobby _joinLobby;
        private readonly IMainMenuLobbyRules _lobbyRules;
        private readonly IClipboard _clipboard;

        private IMainMenuViewController _viewController;
        private UniTaskCompletionSource<MainMenuAction> _action;

        public MainMenuFlow(MainMenuFlowData data, IMainMenuViewControllerFactory viewControllerFactory,
            IMainMenuScreenLocation screenLocation, IPlayerName playerName,
            IHostLobbySettings hostLobbySettings, IJoinLobby joinLobby, IMainMenuLobbyRules lobbyRules, IClipboard clipboard) {
            _data = data;
            _viewControllerFactory = viewControllerFactory;
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
                        // The lobby flow joins; a failed join reopens the menu on the Join panel.
                        _playerName.SavePlayerName();
                        flowResult.ShouldJoin = true;
                        flowResult.LobbyCode = _joinLobby.GetLobbyCode().Trim();
                        flowResult.PlayerName = _playerName.GetTrimmedPlayerName();
                        break;
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

        // The menu opens on its main buttons, or on the Join panel with the reason the last join failed.
        // Host settings and the join code keep their last values.
        private void ResetState() {
            _playerName.LoadPlayerName();
            if (_data.JoinFailure == LobbyJoinFailure.None) {
                _screenLocation.ResetLocation();
                return;
            }
            _screenLocation.SetLocation(MainMenuScreen.JoinLobby);
            _joinLobby.SetJoinStatus(GetFailedStatus(_data.JoinFailure));
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

        private JoinLobbyStatus GetFailedStatus(LobbyJoinFailure failure) {
            switch (failure) {
                case LobbyJoinFailure.MatchInProgress: return JoinLobbyStatus.MatchInProgress;
                case LobbyJoinFailure.LobbyFull: return JoinLobbyStatus.LobbyFull;
                default: return JoinLobbyStatus.Failed;
            }
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
            // The same as typing it: lobby codes are upper case, and copied text often has stray spaces.
            OnLobbyCodeChanged(_clipboard.GetText().Trim().ToUpperInvariant());
        }

        private void OnLobbyCodeChanged(string text) {
            _joinLobby.SetLobbyCode(text);
            // A new code makes the last failure's message out of date.
            _joinLobby.SetJoinStatus(JoinLobbyStatus.Idle);
            _viewController.Refresh();
        }
    }
}
