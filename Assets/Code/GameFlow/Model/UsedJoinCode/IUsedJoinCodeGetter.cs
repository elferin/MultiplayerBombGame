namespace Code.GameFlow {
    public interface IUsedJoinCodeGetter {

        // The relay join code of the last match in this lobby; empty before the first one.
        string GetUsedJoinCode();
    }
}
