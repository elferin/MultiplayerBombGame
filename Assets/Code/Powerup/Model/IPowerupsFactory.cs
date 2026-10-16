namespace Code.Powerup {
    public interface IPowerupsFactory {

        // New for every round, so nothing carries over from the last one.
        IPowerups Create();
    }
}
