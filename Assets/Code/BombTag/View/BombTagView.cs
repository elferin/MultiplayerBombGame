using Code.Match;
using TMPro;
using UnityEngine;

namespace Code.BombTag {

    public class BombTagView : MonoBehaviour {

        [SerializeField] private TMP_Text centerText;
        // Removes itself when it finishes (its stop action is Destroy), so nothing has to keep track of it.
        [SerializeField] private ParticleSystem explosionPrefab;
        [SerializeField] private float explosionShakeStrength = 0.6f;
        [SerializeField] private float explosionShakeSeconds = 0.4f;

        public void Show(BombTagViewData data) {
            centerText.text = data.CenterText;
        }

        public void ShowExplosion(Vector3 position) {
            Instantiate(explosionPrefab, position, Quaternion.identity);
            var view = Camera.main;
            if (view != null && view.TryGetComponent(out CameraShake shake)) {
                shake.Shake(explosionShakeStrength, explosionShakeSeconds);
            }
        }
    }
}
