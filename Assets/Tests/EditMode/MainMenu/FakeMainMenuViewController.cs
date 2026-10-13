using System;
using System.Threading;
using Code.MainMenu;
using Cysharp.Threading.Tasks;

namespace Code.Tests {

    // Stands in for the real view: tests raise its events as if buttons were clicked.
    public class FakeMainMenuViewController : IMainMenuViewController, IMainMenuViewControllerFactory {

        public event Action QuitClicked = delegate { };
        public event Action HostClicked = delegate { };
        public event Action JoinClicked = delegate { };
        public event Action<string> PlayerNameChanged = delegate { };
        public event Action<string> PlayerCountChanged = delegate { };
        public event Action<string> LobbyCodeChanged = delegate { };
        public event Action CreateLobbyClicked = delegate { };
        public event Action JoinLobbyClicked = delegate { };
        public event Action BackClicked = delegate { };

        private readonly UniTaskCompletionSource _closed = new UniTaskCompletionSource();

        public bool IsRunning { get; private set; }
        public bool IsClosed { get; private set; }
        public int RefreshCount { get; private set; }

        public IMainMenuViewController CreateViewController() {
            return this;
        }

        public async UniTask RunAsync(CancellationToken cancellation) {
            IsRunning = true;
            await _closed.Task.AttachExternalCancellation(cancellation);
        }

        public void Refresh() {
            RefreshCount++;
        }

        public void Close() {
            IsClosed = true;
            _closed.TrySetResult();
        }

        public void ClickQuit() => QuitClicked();
        public void ClickHost() => HostClicked();
        public void ClickJoin() => JoinClicked();
        public void ClickCreateLobby() => CreateLobbyClicked();
        public void ClickJoinLobby() => JoinLobbyClicked();
        public void ClickBack() => BackClicked();
        public void TypePlayerName(string text) => PlayerNameChanged(text);
        public void TypePlayerCount(string text) => PlayerCountChanged(text);
        public void TypeLobbyCode(string text) => LobbyCodeChanged(text);
    }
}
