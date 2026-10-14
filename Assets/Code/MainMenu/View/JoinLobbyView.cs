using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MainMenu {

    public class JoinLobbyView : MonoBehaviour {

        public event Action<string> PlayerNameChanged = delegate { };
        public event Action<string> CodeChanged = delegate { };
        public event Action JoinClicked = delegate { };
        public event Action BackClicked = delegate { };
        public event Action PasteClicked = delegate { };

        [SerializeField] private TMP_InputField playerNameInput;
        [SerializeField] private TMP_InputField codeInput;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Button pasteButton;

        private void Awake() {
            // Lobby codes are upper case.
            codeInput.onValidateInput = (text, index, character) => char.ToUpperInvariant(character);
            playerNameInput.onValueChanged.AddListener(OnPlayerNameChanged);
            codeInput.onValueChanged.AddListener(OnCodeChanged);
            joinButton.onClick.AddListener(Join);
            backButton.onClick.AddListener(Back);
            pasteButton.onClick.AddListener(Paste);
        }

        public void Show(JoinLobbyViewData data) {
            if (playerNameInput.text != data.PlayerNameText) {
                playerNameInput.SetTextWithoutNotify(data.PlayerNameText);
            }
            if (codeInput.text != data.CodeText) {
                codeInput.SetTextWithoutNotify(data.CodeText);
            }
            statusText.text = data.StatusText;
            joinButton.interactable = data.CanJoin;
            backButton.interactable = data.CanGoBack;
            pasteButton.interactable = data.CanPaste;
        }

        private void OnPlayerNameChanged(string text) {
            PlayerNameChanged.Invoke(text);
        }

        private void OnCodeChanged(string text) {
            CodeChanged.Invoke(text);
        }

        private void Join() {
            JoinClicked.Invoke();
        }

        private void Back() {
            BackClicked.Invoke();
        }

        private void Paste() {
            PasteClicked.Invoke();
        }
    }
}
