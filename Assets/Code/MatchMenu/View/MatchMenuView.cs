using System;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MatchMenu {

    // Esc isn't read here: MatchView reads it, so one press can't both close the menu and open it again.
    public class MatchMenuView : MonoBehaviour {

        public event Action ContinueClicked = delegate { };
        public event Action LeaveClicked = delegate { };

        [SerializeField] private Button continueButton;
        [SerializeField] private Button leaveButton;

        private void Awake() {
            continueButton.onClick.AddListener(Continue);
            leaveButton.onClick.AddListener(Leave);
        }

        private void Continue() {
            ContinueClicked.Invoke();
        }

        private void Leave() {
            LeaveClicked.Invoke();
        }
    }
}
