using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Code.BombTag {

    [UsedImplicitly]
    public class BombTagAssetLoader {

        private const string BOMB_TAG_VIEW_PATH = "BombTagView";

        public async UniTask<BombTagView> CreateBombTagView(CancellationToken cancellation) {
            var prefab = (BombTagView)await Resources.LoadAsync<BombTagView>(BOMB_TAG_VIEW_PATH).WithCancellation(cancellation);
            var view = Object.Instantiate(prefab);

            return view;
        }
    }
}
