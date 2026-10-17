namespace Code {

    // Something the host looks up by a local id, e.g. a player or a power-up.
    public interface IHasId {

        int Id { get; }
    }
}
