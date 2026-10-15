using Code.Util;

namespace Code.Tests.Util {

    public class FakeClipboard : IClipboard {

        public string Text { get; set; } = string.Empty;

        public string GetText() {
            return Text;
        }

        public void SetText(string text) {
            Text = text;
        }
    }
}
