using TMPro;
using UnityEngine;

namespace Code.Match {

    public class MatchView : MonoBehaviour {

        [SerializeField] private TMP_Text statusText;

        public void Show(MatchViewData data) {
            statusText.text = data.StatusText;
        }
    }
}
