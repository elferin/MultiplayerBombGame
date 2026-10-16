namespace Code.Util {

    // Random numbers, behind an interface so tests can pick them.
    public interface IRandom {

        // A whole number from min up to, but not including, max.
        int Range(int min, int max);
        // A number from min to max, both included.
        float Range(float min, float max);
    }
}
