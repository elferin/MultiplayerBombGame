using System.Collections.Generic;

namespace Code {

    // The components of one type currently enabled in the scene, so code doesn't search the scene for them each frame.
    // Each component keeps one static instance and adds itself in OnEnable, removes itself in OnDisable.
    public class ActiveRegistry<T> {

        private readonly List<T> _items = new();
        private int _lastId;

        public IReadOnlyList<T> Items => _items;

        public void Add(T item) {
            _items.Add(item);
        }

        public void Remove(T item) {
            _items.Remove(item);
        }

        // Ids only known on this machine, starting at 1 so 0 can mean nobody.
        public int NextId() {
            return ++_lastId;
        }
    }
}
