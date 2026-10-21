using System.Collections.Generic;
using System.Threading;
using Code.BombTag;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.Tests.BombTag {

    // Stands in for the real bomb tag view and records the state shown at each refresh.
    public class FakeBombTagViewController : IBombTagViewController, IBombTagViewControllerFactory {

        private readonly IBombTagStatusGetter _statusGetter;
        private readonly UniTaskCompletionSource _closed = new();

        public bool IsClosed { get; private set; }
        public List<BombTagState> RefreshedStates { get; } = new();
        public List<Vector3> Explosions { get; } = new();

        public FakeBombTagViewController(IBombTagStatusGetter statusGetter) {
            _statusGetter = statusGetter;
        }

        public IBombTagViewController CreateViewController() {
            return this;
        }

        public async UniTask RunAsync(CancellationToken cancellation) {
            await _closed.Task.AttachExternalCancellation(cancellation);
        }

        public void Refresh() {
            RefreshedStates.Add(_statusGetter.GetState());
        }

        public void ShowExplosion(Vector3 position) {
            Explosions.Add(position);
        }

        public void Close() {
            IsClosed = true;
            _closed.TrySetResult();
        }
    }
}
