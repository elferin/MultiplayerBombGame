using JetBrains.Annotations;
using UnityEngine;

namespace Code.Util {

    // Reaches the real clipboard in the Editor and desktop builds; WebGL and some mobile platforms need their own code.
    [UsedImplicitly]
    public class Clipboard : IClipboard {

        public string GetText() {
            return GUIUtility.systemCopyBuffer ?? string.Empty;
        }

        public void SetText(string text) {
            GUIUtility.systemCopyBuffer = text;
        }
    }
}
