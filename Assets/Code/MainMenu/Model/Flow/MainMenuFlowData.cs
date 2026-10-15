using Code.Lobby;

namespace Code.MainMenu {

    // How the main menu opens: on its main buttons, or back on the Join panel after a failed join.
    public class MainMenuFlowData {

        // None unless the last join failed.
        public LobbyJoinFailure JoinFailure { get; }

        private MainMenuFlowData(LobbyJoinFailure joinFailure) {
            JoinFailure = joinFailure;
        }

        public static MainMenuFlowData Default() {
            return new MainMenuFlowData(LobbyJoinFailure.None);
        }

        public static MainMenuFlowData JoinFailed(LobbyJoinFailure joinFailure) {
            return new MainMenuFlowData(joinFailure);
        }
    }
}
