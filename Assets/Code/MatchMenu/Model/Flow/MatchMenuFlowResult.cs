namespace Code.MatchMenu {
    public class MatchMenuFlowResult : UniTaskResult {

        public MatchMenuChoice Choice { get; set; } = MatchMenuChoice.Continue;
    }
}
