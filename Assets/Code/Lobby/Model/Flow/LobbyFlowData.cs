namespace Code.Lobby {

    // How LobbyFlow gets its lobby: create a new one, join one by code, or show it again after a match.
    public class LobbyFlowData {

        public LobbyEntry Entry { get; }
        public int MaxPlayers { get; }
        // The name the other players see; set when creating or joining.
        public string PlayerName { get; }
        public string LobbyCode { get; }
        // The join code of the match just played, so it isn't mistaken for a new match.
        public string UsedJoinCode { get; }
        public bool MatchStartFailed { get; }

        private LobbyFlowData(LobbyEntry entry, int maxPlayers, string playerName, string lobbyCode,
            string usedJoinCode, bool matchStartFailed) {
            Entry = entry;
            MaxPlayers = maxPlayers;
            PlayerName = playerName;
            LobbyCode = lobbyCode;
            UsedJoinCode = usedJoinCode;
            MatchStartFailed = matchStartFailed;
        }

        public static LobbyFlowData CreateLobby(int maxPlayers, string playerName) {
            return new LobbyFlowData(LobbyEntry.Create, maxPlayers, playerName, string.Empty, string.Empty, false);
        }

        public static LobbyFlowData JoinLobby(string lobbyCode, string playerName) {
            return new LobbyFlowData(LobbyEntry.Join, 0, playerName, lobbyCode ?? string.Empty, string.Empty, false);
        }

        public static LobbyFlowData ReturnFromMatch(string usedJoinCode, bool matchStartFailed) {
            return new LobbyFlowData(LobbyEntry.ReturnFromMatch, 0, string.Empty, string.Empty, usedJoinCode ?? string.Empty,
                matchStartFailed);
        }
    }
}
