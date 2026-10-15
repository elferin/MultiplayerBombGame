namespace Code.MainMenu {
    public class MainMenuFlowResult : UniTaskResult {

        public bool ShouldQuit { get; set; }
        public bool ShouldHost { get; set; }
        public int MaxPlayers { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        // The player tapped Join on the Join panel; the lobby flow joins with LobbyCode.
        public bool ShouldJoin { get; set; }
        public string LobbyCode { get; set; } = string.Empty;
    }
}
