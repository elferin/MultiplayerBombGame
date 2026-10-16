using Code.FlowSystem;

namespace Code.BombTag {

    // The host runs the rules; clients only show what the host sends.
    public class BombTagFlowData : IFlowData<BombTagFlowResult> {

        public bool IsHost { get; }
        // Player objects the host waits for before the countdown starts.
        public int PlayerCount { get; }

        private BombTagFlowData(bool isHost, int playerCount) {
            IsHost = isHost;
            PlayerCount = playerCount;
        }

        public static BombTagFlowData Host(int playerCount) {
            return new BombTagFlowData(true, playerCount);
        }

        public static BombTagFlowData Client() {
            return new BombTagFlowData(false, 0);
        }
    }
}
