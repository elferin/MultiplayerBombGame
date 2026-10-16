using Code.Powerup;

namespace Code.Tests.Powerup {

    // Counts calls instead of spawning anything.
    public class FakePowerups : IPowerups {

        public int StartCount { get; private set; }
        public int TickCount { get; private set; }

        public void Start() {
            StartCount++;
        }

        public void Tick(float deltaTime) {
            TickCount++;
        }
    }
}
