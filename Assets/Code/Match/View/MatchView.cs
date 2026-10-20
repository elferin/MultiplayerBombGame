using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Match {

    public class MatchView : MonoBehaviour {

        // Esc: opens the match menu, or closes it when it's open.
        public event Action MenuRequested = delegate { };

        [SerializeField] private TMP_Text statusText;
        [SerializeField] private InputActionReference menuAction;

        private void OnEnable() {
            menuAction.action.Enable();
        }

        private void OnDisable() {
            menuAction.action.Disable();
        }

        private void Update() {
            if (menuAction.action.WasPressedThisFrame()) {
                MenuRequested.Invoke();
            }
        }

        public void Show(MatchViewData data) {
            statusText.text = data.StatusText;
        }
    }
}
