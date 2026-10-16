namespace Code.BombTag {
    public class BombTagFlowResult : UniTaskResult {

        // Empty when nobody was left.
        public string WinnerName { get; set; } = string.Empty;
    }
}
