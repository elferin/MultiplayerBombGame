namespace Code.Match {
    public interface IMatchStatusSetter {

        void SetStatus(MatchStatus status);
        void SetIsHost(bool isHost);
    }
}
