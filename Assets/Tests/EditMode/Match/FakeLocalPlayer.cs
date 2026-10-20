using System.Threading;
using Code.Match;
using Cysharp.Threading.Tasks;

namespace Code.Tests.Match {

    // Records the name instead of waiting for a spawned player object.
    public class FakeLocalPlayer : ILocalPlayer {

        public string LastName { get; private set; }

        public UniTask SetName(string playerName, CancellationToken cancellation) {
            LastName = playerName;
            return UniTask.CompletedTask;
        }
    }
}
