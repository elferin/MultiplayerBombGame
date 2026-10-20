using System;
using System.Threading;
using Code.MatchMenu;
using Cysharp.Threading.Tasks;

namespace Code.Tests.MatchMenu {

    // Stands in for the real match menu; tests click its buttons.
    public class FakeMatchMenuViewController : IMatchMenuViewController, IMatchMenuViewControllerFactory {

        public event Action ContinueClicked = delegate { };
        public event Action LeaveClicked = delegate { };

        private readonly UniTaskCompletionSource _closed = new();

        public bool IsRunning { get; private set; }
        public bool IsClosed { get; private set; }

        public IMatchMenuViewController CreateViewController() {
            return this;
        }

        public async UniTask RunAsync(CancellationToken cancellation) {
            IsRunning = true;
            try {
                await _closed.Task.AttachExternalCancellation(cancellation);
            }
            finally {
                IsRunning = false;
            }
        }

        public void Refresh() {
        }

        public void Close() {
            IsClosed = true;
            _closed.TrySetResult();
        }

        public void ClickContinue() => ContinueClicked();

        public void ClickLeave() => LeaveClicked();
    }
}
