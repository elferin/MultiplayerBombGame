using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Lobby {

    public class LobbyView : MonoBehaviour {

        public event Action LeaveClicked = delegate { };
        public event Action StartClicked = delegate { };

        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text lobbyCodeText;
        [SerializeField] private TMP_Text membersText;
        [SerializeField] private Button leaveButton;
        [SerializeField] private Button startButton;

        private void Awake() {
            leaveButton.onClick.AddListener(Leave);
            startButton.onClick.AddListener(StartMatch);
        }

        public void Show(LobbyViewData data) {
            statusText.text = data.StatusText;
            lobbyCodeText.text = data.LobbyCodeText;
            membersText.text = data.PlayersText;
            startButton.gameObject.SetActive(data.ShowsStartButton);
            startButton.interactable = data.CanStartMatch;
        }

        private void Leave() {
            LeaveClicked.Invoke();
        }

        private void StartMatch() {
            StartClicked.Invoke();
        }
    }
}
