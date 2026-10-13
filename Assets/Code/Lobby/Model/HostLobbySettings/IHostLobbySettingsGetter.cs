namespace Code.Lobby {
    public interface IHostLobbySettingsGetter {

        int GetMinPlayers();
        int GetMaxPlayers();
        string GetPlayerCountText();
        // 0 while the entered count is invalid.
        int GetPlayerCount();
        bool IsPlayerCountValid();
    }
}
