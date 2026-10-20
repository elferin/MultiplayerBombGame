using System;
using JetBrains.Annotations;

namespace Code.MatchMenu {

    // Whether this client's match menu is open. Others react through OpenChanged, e.g. to stop the character taking input.
    [UsedImplicitly]
    public class MatchMenuStateHolder : IMatchMenuState {

        public event Action<bool> OpenChanged = delegate { };

        private bool _isOpen;

        // Only a real change is announced.
        public void SetOpen(bool isOpen) {
            if (_isOpen == isOpen) {
                return;
            }
            _isOpen = isOpen;
            OpenChanged.Invoke(isOpen);
        }

        public bool IsOpen() {
            return _isOpen;
        }
    }
}
