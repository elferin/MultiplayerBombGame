using Code.Lobby;
using Code.MainMenu;
using JetBrains.Annotations;

namespace Code.GameFlow {

    [UsedImplicitly]
    public class MainMenuToLobby {

        private readonly IUsedJoinCodeSetter _usedJoinCode;

        public MainMenuToLobby(IUsedJoinCodeSetter usedJoinCode) {
            _usedJoinCode = usedJoinCode;
        }

        // Null when the player neither hosted nor joined.
        public LobbyFlowData GetLobbyData(MainMenuFlowResult result) {
            // A new lobby has had no match yet.
            _usedJoinCode.SetUsedJoinCode(string.Empty);

            if (result.ShouldHost) {
                return LobbyFlowData.CreateLobby(result.MaxPlayers, result.PlayerName);
            }
            if (result.ShouldJoin) {
                return LobbyFlowData.JoinLobby(result.LobbyCode, result.PlayerName);
            }
            return null;
        }
    }
}
