using UnityEngine;

namespace Code.BombTag {
    public interface IBombTagViewController : IViewController {

        // Plays the explosion where a bomb went off.
        void ShowExplosion(Vector3 position);
    }
}
