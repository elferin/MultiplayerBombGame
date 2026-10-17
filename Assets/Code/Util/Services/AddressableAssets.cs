using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Code.Util {

    [UsedImplicitly]
    public class AddressableAssets : IAddressableAssets {

        public async UniTask<T> Instantiate<T>(string address, CancellationToken cancellation) where T : Component {
            // The handle can already be invalid once awaited, so the result is read in its callback.
            var created = new UniTaskCompletionSource<GameObject>();
            Addressables.InstantiateAsync(address).Completed += handle => {
                if (handle.Status == AsyncOperationStatus.Succeeded) {
                    created.TrySetResult(handle.Result);
                }
                else {
                    // Read before releasing: the handle is invalid afterwards.
                    var error = new InvalidOperationException($"Couldn't create '{address}'.", handle.OperationException);
                    // A failed load has no instance for ReleaseInstance to take, so its handle is released here.
                    Addressables.Release(handle);
                    created.TrySetException(error);
                }
            };

            // Not cancelled midway: the copy would still be created, so it's removed once it exists.
            var instance = await created.Task;
            if (cancellation.IsCancellationRequested) {
                Addressables.ReleaseInstance(instance);
                cancellation.ThrowIfCancellationRequested();
            }

            var component = instance.GetComponent<T>();
            if (component == null) {
                Addressables.ReleaseInstance(instance);
                throw new InvalidOperationException($"'{address}' has no {typeof(T).Name}.");
            }
            return component;
        }

        public T LoadNow<T>(string address) where T : Object {
            return Addressables.LoadAssetAsync<T>(address).WaitForCompletion();
        }

        // Already gone when Play mode or the scene ended first.
        public void Release(Component instance) {
            if (instance != null) {
                Addressables.ReleaseInstance(instance.gameObject);
            }
        }
    }
}
