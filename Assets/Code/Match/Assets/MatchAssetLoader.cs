using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Code.Match {

    [UsedImplicitly]
    public class MatchAssetLoader {

        private const string MATCH_VIEW_PATH = "MatchView";
        private const string ARENA_PATH = "Arena";

        public async UniTask<MatchView> CreateMatchView(CancellationToken cancellation) {
            var prefab = (MatchView)await Resources.LoadAsync<MatchView>(MATCH_VIEW_PATH).WithCancellation(cancellation);
            var view = Object.Instantiate(prefab);

            return view;
        }

        public async UniTask<Arena> CreateArena(CancellationToken cancellation) {
            var prefab = (Arena)await Resources.LoadAsync<Arena>(ARENA_PATH).WithCancellation(cancellation);
            return Object.Instantiate(prefab);
        }
    }
}
