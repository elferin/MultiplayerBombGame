namespace Code.MainMenu {
    public class MainMenuFlowResult : UniTaskResult {

        public bool ShouldQuit { get; set; }
        public bool ShouldHost { get; set; }
        public int MaxPlayers { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        // The player joined a lobby from the Join panel; it is LobbyService.CurrentLobby.
        public bool HasJoinedLobby { get; set; }
    }
}
