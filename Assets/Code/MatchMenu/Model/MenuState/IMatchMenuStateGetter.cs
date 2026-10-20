using System;

namespace Code.MatchMenu {
    public interface IMatchMenuStateGetter {

        // Raised with the new value whenever the menu opens or closes.
        event Action<bool> OpenChanged;

        bool IsOpen();
    }
}
