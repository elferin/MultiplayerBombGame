using System.Collections.Generic;

namespace Code {

    public static class IdListExtensions {

        // A linear search; the lists are a handful of players or power-ups. Generic, so structs aren't boxed.
        public static bool TryFindById<T>(this IReadOnlyList<T> items, int id, out T found) where T : IHasId {
            foreach (var item in items) {
                if (item.Id == id) {
                    found = item;
                    return true;
                }
            }
            found = default;
            return false;
        }
    }
}
