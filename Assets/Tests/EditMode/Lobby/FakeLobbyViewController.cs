using System;
using System.Collections.Generic;
using System.Threading;
using Code.Lobby;
using Cysharp.Threading.Tasks;

namespace Code.Tests {

    // Stands in for the real lobby view and records the status shown at each refresh.
    public class FakeLobbyViewController : ILobbyViewController, ILobbyViewControllerFactory {

        public event Action LeaveClicked = delegate { };
        public event Action StartClicked = delegate { };
        public event Action CopyCodeClicked = delegate { };

        private readonly ILobbyStatusGetter _statusGetter;
        private readonly UniTaskCompletionSource _closed = new UniTaskCompletionSource();

        public bool IsRunning { get; private set; }
        public bool IsClosed { get; private set; }
        public List<LobbyStatus> RefreshedStatuses { get; } = new List<LobbyStatus>();

        public FakeLobbyViewController(ILobbyStatusGetter statusGetter) {
            _statusGetter = statusGetter;
        }

        public ILobbyViewController CreateViewController() {
            return this;
        }

        public async UniTask RunAsync(CancellationToken cancellation) {
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

        public void ClickLeave() => LeaveClicked();
        public void ClickStart() => StartClicked();
        public void ClickCopyCode() => CopyCodeClicked();
    }
}
