namespace Code.Match {
    public class MatchFlowResult : UniTaskResult {

        // The network couldn't start, so the match never ran.
        public bool StartFailed { get; set; }
        // The join code this match used; empty if the host failed before getting one.
        public string UsedJoinCode { get; set; } = string.Empty;
    }
}
