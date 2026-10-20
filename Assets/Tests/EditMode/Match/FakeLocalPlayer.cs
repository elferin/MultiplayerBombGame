using Code.Match;

namespace Code.Tests.Match {

    // Records the name instead of setting it on a spawned player object.
    public class FakeLocalPlayer : ILocalPlayer {

        public string LastName { get; private set; }

        public void SetName(string playerName) {
            LastName = playerName;
        }
    }
}
