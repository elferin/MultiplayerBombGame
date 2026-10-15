namespace Code.Profile {
    public interface IPlayerNameSetter {

        void SetPlayerName(string playerName);
        // Replaces the current name with the one saved between sessions.
        void LoadPlayerName();
        // Saves the trimmed name between sessions. An empty name isn't saved.
        void SavePlayerName();
    }
}
