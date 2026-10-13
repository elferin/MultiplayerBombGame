namespace Code.Match {
    public interface IMatchStatusGetter {

        MatchStatus GetStatus();
        // The local player hosts the match.
        bool IsHost();
    }
}
