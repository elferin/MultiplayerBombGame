using PurrNet.Packing;

namespace Code.Match {

    // The host sends it before leaving, because through Relay clients only notice the host stopping after a timeout.
    public readonly struct MatchEndedMessage : IPackedAuto {
    }
}
