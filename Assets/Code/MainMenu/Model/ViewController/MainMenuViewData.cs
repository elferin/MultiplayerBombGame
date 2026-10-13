namespace Code.MainMenu {

    public class MainMenuViewData {

        public bool ShowMainMenu { get; }
        public bool ShowHostLobby { get; }
        public bool ShowJoinLobby { get; }
        public HostLobbyViewData HostLobby { get; }
        public JoinLobbyViewData JoinLobby { get; }

        public MainMenuViewData(bool showMainMenu, bool showHostLobby, bool showJoinLobby,
            HostLobbyViewData hostLobby, JoinLobbyViewData joinLobby) {
            ShowMainMenu = showMainMenu;
            ShowHostLobby = showHostLobby;
            ShowJoinLobby = showJoinLobby;
            HostLobby = hostLobby;
            JoinLobby = joinLobby;
        }
    }
}
