using System.Threading;
using Code.Util;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace Code.Match {

    [UsedImplicitly]
    public class MatchAssetLoader {

        private const string MATCH_VIEW_ADDRESS = "MatchView";
        private const string ARENA_ADDRESS = "Arena";

        private readonly IAddressableAssets _assets;

        public MatchAssetLoader(IAddressableAssets assets) {
            _assets = assets;
        }

        public UniTask<MatchView> CreateMatchView(CancellationToken cancellation) {
            return _assets.Instantiate<MatchView>(MATCH_VIEW_ADDRESS, cancellation);
        }

        public void DestroyMatchView(MatchView view) {
            _assets.Release(view);
        }

        public UniTask<Arena> CreateArena(CancellationToken cancellation) {
            return _assets.Instantiate<Arena>(ARENA_ADDRESS, cancellation);
        }

        public void DestroyArena(Arena arena) {
            _assets.Release(arena);
        }
    }
}
