namespace Code.MainMenu {

    public class JoinLobbyViewData {

        public string PlayerNameText { get; }
        public string CodeText { get; }
        public string StatusText { get; }
        public bool CanJoin { get; }
        public bool CanGoBack { get; }
        public bool CanPaste { get; }

        public JoinLobbyViewData(string playerNameText, string codeText, string statusText, bool canJoin, bool canGoBack,
            bool canPaste) {
            PlayerNameText = playerNameText;
            CodeText = codeText;
            StatusText = statusText;
            CanJoin = canJoin;
            CanGoBack = canGoBack;
            CanPaste = canPaste;
        }
    }
}
