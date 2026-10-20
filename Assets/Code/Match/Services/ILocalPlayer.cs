using System.Threading;
using Cysharp.Threading.Tasks;

namespace Code.Match {

    // The player object this client owns.
    public interface ILocalPlayer {

        // Waits for the object to spawn, then shows the name to everyone.
        UniTask SetName(string playerName, CancellationToken cancellation);
        // Stops the character taking move and power-up input while the match menu is open. Does nothing before it spawns.
        void SetMenuOpen(bool isMenuOpen);
    }
}
