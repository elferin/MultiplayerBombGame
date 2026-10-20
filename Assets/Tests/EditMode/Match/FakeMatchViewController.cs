using System;
using System.Collections.Generic;
using System.Threading;
using Code.Match;
using Cysharp.Threading.Tasks;

namespace Code.Tests.Match {

    // Stands in for the real match view and records the status shown at each refresh.
    public class FakeMatchViewController : IMatchViewController, IMatchViewControllerFactory {

        public event Action MenuRequested = delegate { };

        private readonly IMatchStatusGetter _statusGetter;
        private readonly UniTaskCompletionSource _closed = new UniTaskCompletionSource();

        // RunAsync throws at once, like a view that fails to load.
        public bool FailsToLoad { get; set; }
        public bool IsRunning { get; private set; }
        public bool IsClosed { get; private set; }
        public List<MatchStatus> RefreshedStatuses { get; } = new List<MatchStatus>();

        public FakeMatchViewController(IMatchStatusGetter statusGetter) {
            _statusGetter = statusGetter;
        }

        public IMatchViewController CreateViewController() {
            return this;
        }

        public async UniTask RunAsync(CancellationToken cancellation) {
            if (FailsToLoad) {
                throw new InvalidOperationException("Match view failed to load");
            }
            IsRunning = true;
            await _closed.Task.AttachExternalCancellation(cancellation);
        }

        public void Refresh() {
            RefreshedStatuses.Add(_statusGetter.GetStatus());
        }

        public void Close() {
            IsClosed = true;
            _closed.TrySetResult();
        }

        public void RaiseMenuRequested() => MenuRequested();
    }
}
