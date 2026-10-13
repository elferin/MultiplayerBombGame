namespace Code.Lobby {
    public interface IPlayerNameGetter {

        // The name as typed, for the input fields.
        string GetPlayerName();
        // The name without surrounding spaces, for creating, joining and saving.
        string GetTrimmedPlayerName();
        bool HasPlayerName();
    }
}
