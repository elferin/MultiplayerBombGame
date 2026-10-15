namespace Code.MainMenu {

    public class JoinLobbyViewData {

        public string PlayerNameText { get; }
        public string CodeText { get; }
        public string StatusText { get; }
        public bool CanJoin { get; }

        public JoinLobbyViewData(string playerNameText, string codeText, string statusText, bool canJoin) {
            PlayerNameText = playerNameText;
            CodeText = codeText;
            StatusText = statusText;
            CanJoin = canJoin;
        }
    }
}
