namespace Code.Lobby {

    // How LobbyFlow gets its lobby: create a new one, show the one the player has already joined,
    // or show it again after a match.
    public class LobbyFlowData {

        public bool CreatesLobby { get; }
        public int MaxPlayers { get; }
        public string PlayerName { get; }
        public bool ReturnsFromMatch { get; }
        // The join code of the match just played, so it isn't mistaken for a new match.
        public string UsedJoinCode { get; }
        public bool MatchStartFailed { get; }

        private LobbyFlowData(bool createsLobby, int maxPlayers, string playerName,
            bool returnsFromMatch, string usedJoinCode, bool matchStartFailed) {
            CreatesLobby = createsLobby;
            MaxPlayers = maxPlayers;
            PlayerName = playerName;
            ReturnsFromMatch = returnsFromMatch;
            UsedJoinCode = usedJoinCode;
            MatchStartFailed = matchStartFailed;
        }

        public static LobbyFlowData CreateLobby(int maxPlayers, string playerName) {
            return new LobbyFlowData(true, maxPlayers, playerName, false, string.Empty, false);
        }

        public static LobbyFlowData JoinedLobby() {
            return new LobbyFlowData(false, 0, string.Empty, false, string.Empty, false);
        }

        public static LobbyFlowData ReturnFromMatch(string usedJoinCode, bool matchStartFailed) {
            return new LobbyFlowData(false, 0, string.Empty, true, usedJoinCode ?? string.Empty, matchStartFailed);
        }
    }
}
