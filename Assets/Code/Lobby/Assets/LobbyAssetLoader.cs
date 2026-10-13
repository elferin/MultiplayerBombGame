using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Code.Lobby {
    
    [UsedImplicitly]
    public class LobbyAssetLoader {

        private const string LOBBY_VIEW_PATH = "LobbyView";

        public async UniTask<LobbyView> CreateLobbyView(CancellationToken cancellation) {
            var prefab = (LobbyView)await Resources.LoadAsync<LobbyView>(LOBBY_VIEW_PATH).WithCancellation(cancellation);
            var view = Object.Instantiate(prefab);

            return view;
        }
    }
}
