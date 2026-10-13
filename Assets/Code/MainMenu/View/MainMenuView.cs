using System;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainMenu {

    public class MainMenuView : MonoBehaviour {

        // Main buttons
        public event Action QuitClicked = delegate { };
        public event Action HostClicked = delegate { };
        public event Action JoinClicked = delegate { };

        // Host and Join panels
        public event Action<string> PlayerNameChanged = delegate { };
        public event Action<string> PlayerCountChanged = delegate { };
        public event Action<string> LobbyCodeChanged = delegate { };
        public event Action CreateLobbyClicked = delegate { };
        public event Action JoinLobbyClicked = delegate { };
        public event Action BackClicked = delegate { };

        [SerializeField] private Button quitButton;
        [SerializeField] private Button hostButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private GameObject buttonContainer;
        [SerializeField] private HostLobbyView hostLobbyView;
        [SerializeField] private JoinLobbyView joinLobbyView;

        private void Awake() {
            quitButton.onClick.AddListener(Quit);
            hostButton.onClick.AddListener(Host);
            joinButton.onClick.AddListener(Join);

            hostLobbyView.PlayerNameChanged += OnPlayerNameChanged;
            hostLobbyView.PlayerCountChanged += OnPlayerCountChanged;
            hostLobbyView.CreateClicked += OnCreateLobbyClicked;
            hostLobbyView.BackClicked += OnBackClicked;

            joinLobbyView.PlayerNameChanged += OnPlayerNameChanged;
            joinLobbyView.CodeChanged += OnLobbyCodeChanged;
            joinLobbyView.JoinClicked += OnJoinLobbyClicked;
            joinLobbyView.BackClicked += OnBackClicked;
        }

        public void Show(MainMenuViewData data) {
            buttonContainer.SetActive(data.ShowMainMenu);
            hostLobbyView.gameObject.SetActive(data.ShowHostLobby);
            joinLobbyView.gameObject.SetActive(data.ShowJoinLobby);
            hostLobbyView.Show(data.HostLobby);
            joinLobbyView.Show(data.JoinLobby);
        }

        private void Quit() {
            QuitClicked.Invoke();
        }

        private void Host() {
            HostClicked.Invoke();
        }

        private void Join() {
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
    }
}
