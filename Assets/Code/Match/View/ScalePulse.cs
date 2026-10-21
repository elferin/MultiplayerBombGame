using UnityEngine;

namespace Code.Match {

    // Makes an object throb by scaling it up and down around the size it was given, e.g. the bomb or the shield bubble.
    public class ScalePulse : MonoBehaviour {

        [SerializeField] private float pulsesPerSecond = 2f;
        [Tooltip("How much bigger than its rest size it gets at the top of a pulse, e.g. 0.15 for 15%.")]
        [SerializeField] private float amount = 0.15f;

        private Vector3 _restScale;

        private void Awake() {
            _restScale = transform.localScale;
        }

        private void Update() {
            // Sharp at the top and slow at the bottom, like a heartbeat.
            var wave = Mathf.Pow(Mathf.Abs(Mathf.Sin(Time.time * pulsesPerSecond * Mathf.PI)), 4f);
            transform.localScale = _restScale * (1f + wave * amount);
        }
    }
}
