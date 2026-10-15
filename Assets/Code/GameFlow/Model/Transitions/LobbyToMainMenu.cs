using Code.Lobby;
using Code.MainMenu;
using JetBrains.Annotations;

namespace Code.GameFlow {

    [UsedImplicitly]
    public class LobbyToMainMenu {

        // Leaving the lobby opens the main menu; a failed join opens its Join panel with the reason.
        public MainMenuFlowData GetMainMenuData(LobbyFlowResult result) {
            if (result.JoinFailure == LobbyJoinFailure.None) {
                return MainMenuFlowData.Default();
            }
            return MainMenuFlowData.JoinFailed(result.JoinFailure);
        }
    }
}
