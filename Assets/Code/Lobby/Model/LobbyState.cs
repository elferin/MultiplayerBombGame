using System.Collections.Generic;

namespace Code.Lobby {

    // The lobby as the rest of the game sees it, independent of the lobby provider.
    public class LobbyState {

        public static readonly LobbyState None = new LobbyState(false, string.Empty, 0, new List<string>(), false, string.Empty);

        public bool IsValid { get; }
        public string Code { get; }
        public int MaxPlayers { get; }
        public IReadOnlyList<string> PlayerNames { get; }
        // The local player created the lobby and is its host.
        public bool IsOwner { get; }
        // The relay join code the host shares when it starts the match; empty until then.
        public string MatchJoinCode { get; }

        public LobbyState(bool isValid, string code, int maxPlayers, IReadOnlyList<string> playerNames, bool isOwner, string matchJoinCode) {
            IsValid = isValid;
            Code = code;
            MaxPlayers = maxPlayers;
            PlayerNames = playerNames;
            IsOwner = isOwner;
            MatchJoinCode = matchJoinCode;
        }
    }
}
