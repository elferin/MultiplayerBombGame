using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.Util {

    // Loads assets marked addressable, by their address.
    public interface IAddressableAssets {

        // Creates a copy of the prefab at the address. Remove it with Release, not Object.Destroy, so the prefab can unload.
        UniTask<T> Instantiate<T>(string address, CancellationToken cancellation) where T : Component;
        // Waits until the asset is loaded. Only for small assets the game keeps until it closes.
        T LoadNow<T>(string address) where T : Object;
        // Destroys a copy made by Instantiate. Does nothing when it's already gone.
        void Release(Component instance);
    }
}
