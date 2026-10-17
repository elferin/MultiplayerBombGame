using PurrNet;
using UnityEngine;

namespace Code {

    public static class NetworkManagerLookup {

        // NetworkManager.main is only set once the manager has registered itself, so this falls back to searching the scene.
        public static NetworkManager Find() {
            return NetworkManager.main != null ? NetworkManager.main : Object.FindAnyObjectByType<NetworkManager>();
        }
    }
}
