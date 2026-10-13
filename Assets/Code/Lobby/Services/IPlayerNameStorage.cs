namespace Code.Lobby {

    // Remembers the player's name between sessions.
    public interface IPlayerNameStorage {

        string Load();
        // An empty name isn't saved.
        void Save(string playerName);
    }
}
