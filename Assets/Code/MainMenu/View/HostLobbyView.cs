using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainMenu {

    public class HostLobbyView : MonoBehaviour {

        public event Action<string> PlayerNameChanged = delegate { };
        public event Action<string> PlayerCountChanged = delegate { };
        public event Action CreateClicked = delegate { };
        public event Action BackClicked = delegate { };

        [SerializeField] private TMP_InputField playerNameInput;
        [SerializeField] private TMP_InputField playerCountInput;
        [SerializeField] private TMP_Text hintText;
        [SerializeField] private Button createButton;
        [SerializeField] private Button backButton;

        private void Awake() {
            playerNameInput.onValueChanged.AddListener(OnPlayerNameChanged);
            playerCountInput.onValueChanged.AddListener(OnPlayerCountChanged);
            createButton.onClick.AddListener(Create);
            backButton.onClick.AddListener(Back);
        }

        public void Show(HostLobbyViewData data) {
            // Only overwrite the text when it differs, so the caret isn't reset while typing.
            if (playerNameInput.text != data.PlayerNameText) {
                playerNameInput.SetTextWithoutNotify(data.PlayerNameText);
            }
            if (playerCountInput.text != data.PlayerCountText) {
                playerCountInput.SetTextWithoutNotify(data.PlayerCountText);
            }
            hintText.text = data.HintText;
            createButton.interactable = data.CanCreate;
        }

        private void OnPlayerNameChanged(string text) {
            PlayerNameChanged.Invoke(text);
        }

        private void OnPlayerCountChanged(string text) {
            PlayerCountChanged.Invoke(text);
        }

        private void Create() {
            CreateClicked.Invoke();
        }

        private void Back() {
            BackClicked.Invoke();
        }
    }
}
