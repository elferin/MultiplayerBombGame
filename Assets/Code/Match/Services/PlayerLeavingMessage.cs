using PurrNet.Packing;

namespace Code.Match {

    // A client sends it to the host before leaving, because through Relay the host only notices a client stopping after a timeout.
    public readonly struct PlayerLeavingMessage : IPackedAuto {
    }
}
