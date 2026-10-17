using System.Threading;
using Code.Util;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Code.Lobby {

    [UsedImplicitly]
    public class LobbyAssetLoader {

        private const string LOBBY_VIEW_ADDRESS = "LobbyView";

        private readonly IAddressableAssets _assets;

        public LobbyAssetLoader(IAddressableAssets assets) {
            _assets = assets;
        }

        public UniTask<LobbyView> CreateLobbyView(CancellationToken cancellation) {
            return _assets.Instantiate<LobbyView>(LOBBY_VIEW_ADDRESS, cancellation);
        }

        public void DestroyLobbyView(LobbyView view) {
            _assets.Release(view);
        }
    }
}
