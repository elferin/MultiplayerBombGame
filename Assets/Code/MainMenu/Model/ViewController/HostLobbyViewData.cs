namespace Code.MainMenu {

    public class HostLobbyViewData {

        public string PlayerNameText { get; }
        public string PlayerCountText { get; }
        public string HintText { get; }
        public bool CanCreate { get; }

        public HostLobbyViewData(string playerNameText, string playerCountText, string hintText, bool canCreate) {
            PlayerNameText = playerNameText;
            PlayerCountText = playerCountText;
            HintText = hintText;
            CanCreate = canCreate;
        }
    }
}
