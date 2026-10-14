using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Lobby {

    public class LobbyView : MonoBehaviour {

        public event Action LeaveClicked = delegate { };
        public event Action StartClicked = delegate { };
        public event Action CopyCodeClicked = delegate { };

        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text lobbyCodeText;
        [SerializeField] private TMP_Text membersText;
        [SerializeField] private Button leaveButton;
        [SerializeField] private Button startButton;
        [SerializeField] private Button copyCodeButton;

        private void Awake() {
            leaveButton.onClick.AddListener(Leave);
            startButton.onClick.AddListener(StartMatch);
            copyCodeButton.onClick.AddListener(CopyCode);
        }

        public void Show(LobbyViewData data) {
            statusText.text = data.StatusText;
            lobbyCodeText.text = data.LobbyCodeText;
            copyCodeButton.gameObject.SetActive(data.ShowsCopyButton);
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

        private void CopyCode() {
            CopyCodeClicked.Invoke();
        }
    }
}
