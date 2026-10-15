using Cysharp.Threading.Tasks;

namespace Code.Lobby {

    // How the host tells everyone in the lobby about the match.
    public interface ILobbyMatchSignals {

        // Tells everyone in the lobby that the host tapped Start, before the join code is ready.
        UniTask AnnounceMatchStart();
        // Locks the lobby so nobody new can join, then shares the match's join code with everyone in it.
        UniTask StartMatch(string joinCode);
        // Clears the starting flag and the join code, then unlocks the lobby, so the next match can start.
        UniTask EndMatch();
    }
}
