namespace Code.BombTag {

    // Everything BombTagView displays, already formatted.
    public class BombTagViewData {

        // The countdown or the winner; empty while playing.
        public string CenterText { get; }

        public BombTagViewData(string centerText) {
            CenterText = centerText;
        }
    }
}
