namespace Code.Match {

    // The player object this client owns.
    public interface ILocalPlayer {

        // Shows the name to everyone on the local character, now or as soon as it appears.
        void SetName(string playerName);
    }
}
