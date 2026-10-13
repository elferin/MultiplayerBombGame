using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Match {

    public class MatchView : MonoBehaviour {

        public event Action EndClicked = delegate { };

        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Button endButton;

        private void Awake() {
            endButton.onClick.AddListener(EndMatch);
        }

        public void Show(MatchViewData data) {
            statusText.text = data.StatusText;
            endButton.gameObject.SetActive(data.ShowsEndButton);
        }

        private void EndMatch() {
            EndClicked.Invoke();
        }
    }
}
